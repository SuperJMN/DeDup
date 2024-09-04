using CoenM.ImageHash;
using CSharpFunctionalExtensions;
using Serilog;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.FileSystem.Core;
using Zafiro.FileSystem.Local;
using Zafiro.Misc;

namespace ImageStorageOptimizer;

public class ImageStore(IImageHash algorithm)
{
    public IImageHash Algorithm { get; } = algorithm;

    public async Task<Result> Simplify(IEnumerable<ZafiroPath> sources, ZafiroPath destination)
    {
        var fileSystem = new FileSystem(new System.IO.Abstractions.FileSystem());
        
        var imageProcessor = new ImageClusterer<HashedImageFile>(Operations.CalculateHash, Operations.ChooseImage);

        var allFilesResult = await Result.Success()
            .Bind(() => Operations.GetFilesFrom(fileSystem, sources.ToArray()));

        var directoryResult = await fileSystem.GetDirectory(destination);

        var uniqueImagesResult = await Result.Success()
            .LogInfo("Getting files from {Sources}", sources.Cast<object>().ToArray())
            .Bind(() => allFilesResult.Map(x => x.Where(f => Operations.IsImage(f.Name)).Take(100)))
            .Map(files => files.Select(file => HashedImageFile.Create(file, Algorithm)).Concat().Successes())
            .Map(images => imageProcessor.GetUniqueItems(images.ToList()))
            .Map(list => list.Select(x => x.File));

        var execution = directoryResult.SelectMany(outputDirectory => uniqueImagesResult, (outputDirectory, files) =>
        {
            Log.Information("Copying files to {Output}", outputDirectory);
            return Operations.CopyFilesTo(files, outputDirectory);
        });

        return await execution.UnrollBind().LogInfo("Execution completed");
    }
}