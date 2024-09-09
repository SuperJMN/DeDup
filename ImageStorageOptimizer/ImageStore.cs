using System.IO.Abstractions;
using CoenM.ImageHash;
using CSharpFunctionalExtensions;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.FileSystem.Core;

namespace ImageStorageOptimizer;

public class ImageStore
{
    private readonly IImageHash algorithm;

    public ImageStore(IImageHash algorithm)
    {
        this.algorithm = algorithm;
    }

    public async Task<Result> CopyUnique(IEnumerable<ZafiroPath> sources, ZafiroPath destination, double similarityThreshold)
    {
        var fileSystem = new Zafiro.FileSystem.Local.FileSystem(new FileSystem());
        var allFilesResult = await Result.Success()
            .Bind(() => Operations.GetFilesFrom(fileSystem, sources.ToArray()));
        var outputDirectoryResult = await fileSystem.GetDirectory(destination);

        return await allFilesResult
             .CombineAndMap(outputDirectoryResult, (files, output) => Actions.CopyUniqueFiles(files, output, similarityThreshold, algorithm))
             .Map(detupTaskResult => detupTaskResult);
    }
    
    public async Task<Result> CopyDuplicates(IEnumerable<ZafiroPath> sources, ZafiroPath destination, double similarityThreshold)
    {
        var fileSystem = new Zafiro.FileSystem.Local.FileSystem(new FileSystem());
        var allFilesResult = await Result.Success()
            .Bind(() => Operations.GetFilesFrom(fileSystem, sources.ToArray()));
        var outputDirectoryResult = await fileSystem.GetDirectory(destination);

        return await allFilesResult
            .CombineAndMap(outputDirectoryResult, (files, output) => Actions.CopyDuplicates(files, output, similarityThreshold, algorithm))
            .Map(detupTaskResult => detupTaskResult);
    }
}