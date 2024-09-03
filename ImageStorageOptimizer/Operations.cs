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
using Zafiro.FileSystem.Core;
using Zafiro.FileSystem.Mutable;
using Zafiro.FileSystem.Readonly;
using Zafiro.Misc;

namespace ImageStorageOptimizer;

public static class Operations
{
    public static async Task<Result<IEnumerable<IFile>>> GetFiles(AverageHash algo, IMutableDirectory dir)
    {
        return await dir.ToDirectory()
            .Map(directory => directory.AllFiles().Where(x => new[] { "jpg", "bmp", "gif", "png" }.Contains(((ZafiroPath)x.Name).Extension().GetValueOrDefault(""))))
            .Bind(files =>
            {
                return files.Select(file =>
                {
                    return Result.Try(() => Image.Load<Rgba32>(file.Bytes()))
                        .Map(x => (Hash: algo.Hash(x), File: file));
                }).Combine();
            }).Map(hashedFiles => Operations.SelectFiles(hashedFiles.ToList()));
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