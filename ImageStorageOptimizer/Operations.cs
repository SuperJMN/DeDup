using CoenM.ImageHash;
using CoenM.ImageHash.HashAlgorithms;
using CSharpFunctionalExtensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Zafiro;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.FileSystem.Core;
using Zafiro.FileSystem.Mutable;
using Zafiro.FileSystem.Readonly;

namespace ImageStorageOptimizer;

public static class Operations
{
    public static async Task<Result<IEnumerable<IFile>>> GetFiles(AverageHash algo, IMutableDirectory dir)
    {
        return await dir.ToDirectory()
            .Map(directory => directory.RootedFiles().Where(x => new[] { "jpg", "bmp", "gif", "png" }.Contains(x.FullPath().Extension().GetValueOrDefault(""))))
            .MapEach(async file =>
            {
                var image = Image.Load<Rgba32>(file.Bytes());
                return (Hash: algo.Hash(image), File: file.Value);
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

    public static async Task CopyFilesTo(IEnumerable<IFile> files, IMutableDirectory output)
    {
        var enumerableOfTaskResults = files.Select(file => output.CreateFileWithContents(file.Name, file));
        await enumerableOfTaskResults.CombineInOrder();
    }
}