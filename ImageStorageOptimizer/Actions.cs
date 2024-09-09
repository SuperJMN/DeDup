using System.Reactive.Linq;
using CoenM.ImageHash;
using CSharpFunctionalExtensions;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.FileSystem.Mutable;
using Zafiro.FileSystem.Readonly;

namespace ImageStorageOptimizer;

public abstract class Actions
{
    public static async Task CopyUniqueFiles(IEnumerable<IFile> files, IMutableDirectory output, double similarityThreshold, IImageHash algorithm)
    {
        var imageProcessor = new Clusterer<HashedImageFile>(Operations.CalculateDistance);

        await GetClusters(files, similarityThreshold, algorithm, imageProcessor)
            .Select(clusters => clusters.Select(Operations.ChooseImage).ToList())
            .SelectMany(list => list)
            .SelectMany(file => output.CreateFileWithContents(file.File))
            .Successes();
    }

    public static async Task CopyDuplicates(IEnumerable<IFile> images, IMutableDirectory output, double similarityThreshold, IImageHash algorithm)
    {
        var imageProcessor = new Clusterer<HashedImageFile>(Operations.CalculateDistance);

        await GetClusters(images, similarityThreshold, algorithm, imageProcessor)
            .Select(clusters => clusters.Where(x => x.Count > 1))
            .SelectMany(list => list.Select((imageFiles, i) => (imageFiles, i)))
            .SelectMany(list => CopyCluster(list.i, list.imageFiles, output))
            .Successes();
    }

    private static async Task<Result> CopyCluster(int index, List<HashedImageFile> images, IMutableDirectory output)
    {
        var copies = await images
            .Select((file, i) => (file, i))
            .ToObservable()
            .SelectMany(x => output.CreateFileWithContents($"Group {index} - Version {x.i} - {x.file.File.Name}", x.file.File));
        return copies;
    }

    private static IObservable<List<List<HashedImageFile>>> GetClusters(IEnumerable<IFile> files, double similarityThreshold, IImageHash algorithm, Clusterer<HashedImageFile> imageProcessor)
    {
        return files
            .ToObservable()
            .Where(f => Operations.IsImage(f.Name))
            .SelectMany(image => HashedImageFile.Create(image, algorithm))
            .Successes()
            .ToList()
            .Select(images => imageProcessor.GetClusters(images, 1 - similarityThreshold));
    }
}