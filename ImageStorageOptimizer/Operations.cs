using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using CSharpFunctionalExtensions;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Zafiro;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.FileSystem.Core;
using Zafiro.FileSystem.Mutable;
using Zafiro.FileSystem.Readonly;
using Zafiro.Misc;
using Zafiro.Mixins;
using IFile = Zafiro.FileSystem.Readonly.IFile;

namespace ImageStorageOptimizer;

public static class Operations
{
    public static Task<Result<IEnumerable<IFile>>> GetFilesFrom(IMutableFileSystem filesystem, params ZafiroPath[] paths)
    {
        return paths.Select(x => filesystem.GetDirectory(x)
                .Bind(dir => dir.ToDirectory())
                .Map(dir => dir.AllFiles()))
            .CombineSequentially()
            .Map(x => x.Flatten());
    } 
    
    public static async Task<Result<IEnumerable<IFile>>> GetUniqueFiles(AverageHash algo, IEnumerable<IFile> files)
    {
        var filtered = files.Where(x => new[] { "jpg", "bmp", "gif", "png" }.Contains(((ZafiroPath)x.Name).Extension().GetValueOrDefault("")));

        var hashedFiles = filtered
            .Select(file =>
            {
                return Result.Success()
                    .LogInfo("Processing file {File}", file)
                    .MapTry(() => Image.Load<Rgba32>(file.Bytes()))
                    .Map(x => (Hash: algo.Hash(x), File: file));
            });

        var combined = hashedFiles.Combine();

        return combined.Map(hf => SelectFiles(hf.ToList()));
    }
    
    public static IEnumerable<IFile> SelectFiles(IList<(ulong Hash, IFile File)> hashedFiles)
    {
        var filesWithCandidates = hashedFiles
            .Select(current => hashedFiles.Where(b => CompareHash.Similarity(current.Hash, b.Hash) > 95).Select(a => a.File).ToHashSet())
            .ToHashSet(new LambdaComparer<HashSet<IFile>>((a, b) => a.SetEquals(b)));
        
        var selectFiles = from f in filesWithCandidates
            select (from c in f
                let score = Score(c)
                orderby score descending
                select c).First();
        
        return selectFiles;
    }

    private static double Score(IFile file)
    {
        return file.Length;
    }

    public static async Task<Result> CopyFilesTo(IEnumerable<IFile> files, IMutableDirectory output)
    {
        var enumerableOfTaskResults = files.ToList().Select(file => CreateNewFile(output, file));
        return await enumerableOfTaskResults.CombineInOrder();
    }

    private static Task<Result> CreateNewFile(IMutableDirectory output, IFile file)
    {
        var name = StringUtil.GetNewName(
            file.Name, 
            name => output.HasFile(name)
                .TapIf(exists => exists, () => Log.Warning("Filename '{Name}' exists. Choosing a new one.", name))
                .Map(x => !x),
            (s, i) =>
            {
                var zafiroPath = (ZafiroPath)s;
                var newName = zafiroPath.NameWithoutExtension() + "_conflict_" + i + "." + zafiroPath.Extension();
                return newName;
            });
        
        return name
            .Tap(n => Log.Information("Copying file {File} to {Name}", file, n))
            .Bind(finalName => output.CreateFileWithContents(finalName, file));        
    }
}