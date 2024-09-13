using System.Reactive.Linq;
using CoenM.ImageHash;
using CSharpFunctionalExtensions;
using Serilog;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.FileSystem.Core;
using Zafiro.FileSystem.Mutable;
using Zafiro.FileSystem.Readonly;
using Zafiro.Misc;
using Zafiro.Mixins;

namespace DuplicateFinder;

public static class FileOperations
{
    public static async Task CopyUniqueFiles(IEnumerable<IFile> files, IMutableDirectory output, double similarityThreshold, IImageHash algorithm)
    {
        var imageProcessor = new Clusterer<HashedImage>(Misc.CalculateDistance);

        await GetClusters(files, similarityThreshold, algorithm, imageProcessor)
            .Select(clusters => clusters.Select(Misc.ChooseImage).ToList())
            .SelectMany(list => list)
            .SelectMany(hashedImageFile => output.CreateNewFile(hashedImageFile.File))
            .Successes();
    }

    public static async Task CopyDuplicates(IEnumerable<IFile> images, IMutableDirectory output, double similarityThreshold, IImageHash algorithm)
    {
        var imageProcessor = new Clusterer<HashedImage>(Misc.CalculateDistance);

        await GetClusters(images, similarityThreshold, algorithm, imageProcessor)
            .Select(clusters => clusters.Where(x => x.Count > 1))
            .SelectMany(list => list.Select((imageFiles, i) => (imageFiles, i)))
            .SelectMany(list => CopyCluster(list.i, list.imageFiles, output))
            .Successes();
    }

    private static async Task<Result> CopyCluster(int index, List<HashedImage> images, IMutableDirectory output)
    {
        var copies = await images
            .Select((file, i) => (file, i))
            .ToObservable()
            .SelectMany(x => output.CreateFileWithContents($"Group {index} - Version {x.i} - {x.file.File.Name}", x.file.File));
        return copies;
    }

    private static IObservable<List<List<HashedImage>>> GetClusters(IEnumerable<IFile> files, double similarityThreshold, IImageHash algorithm, Clusterer<HashedImage> imageProcessor)
    {
        return files
            .ToObservable()
            .Where(f => Misc.IsImage(f.Name))
            .SelectMany(image => HashedImage.Create(image, algorithm))
            .Successes()
            .ToList()
            .Select(images => imageProcessor.GetClusters(images, 1 - similarityThreshold));
    }
    
    public static Task<Result<IEnumerable<IFile>>> GetFilesFrom(IMutableFileSystem filesystem, params ZafiroPath[] paths)
    {
        return paths.Select(x => filesystem.GetDirectory(x)
                .Bind(dir => dir.ToDirectory())
                .Map(dir => dir.AllFiles()))
            .CombineSequentially()
            .Map(x => x.Flatten());
    }

    private static Task<Result> CreateNewFile(this IMutableDirectory output, IFile file)
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
            .Bind(finalName =>
            {
                Log.Information("Copying file {File} to {Name}", file, finalName);
                return output.CreateFileWithContents(finalName, file);
            });
    }
}