using System.IO.Abstractions;
using CoenM.ImageHash;
using CSharpFunctionalExtensions;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.FileSystem.Core;

namespace DuplicateFinder;

public class DuplicateFinder(IImageHash algorithm)
{
    public async Task<Result> CopyUnique(IEnumerable<ZafiroPath> sources, ZafiroPath destination, double similarityThreshold)
    {
        var fileSystem = new Zafiro.FileSystem.Local.FileSystem(new FileSystem());
        var allFilesResult = await Result.Success()
            .Bind(() => FileOperations.GetFilesFrom(fileSystem, sources.ToArray()));
        var outputDirectoryResult = await fileSystem.GetDirectory(destination);

        return await allFilesResult
             .CombineAndMap(outputDirectoryResult, (files, output) => FileOperations.CopyUniqueFiles(files, output, similarityThreshold, algorithm))
             .Map(copyUnique => copyUnique);
    }
    
    public async Task<Result> CopyDuplicates(IEnumerable<ZafiroPath> sources, ZafiroPath destination, double similarityThreshold)
    {
        var fileSystem = new Zafiro.FileSystem.Local.FileSystem(new FileSystem());
        var allFilesResult = await Result.Success()
            .Bind(() => FileOperations.GetFilesFrom(fileSystem, sources.ToArray()));
        var outputDirectoryResult = await fileSystem.GetDirectory(destination);

        return await allFilesResult
            .CombineAndMap(outputDirectoryResult, (files, output) => FileOperations.CopyDuplicates(files, output, similarityThreshold, algorithm))
            .Map(copyDuplicate => copyDuplicate);
    }
}