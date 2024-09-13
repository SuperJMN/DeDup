using CoenM.ImageHash.HashAlgorithms;
using CommandLine;
using CSharpFunctionalExtensions;
using Serilog;
using Zafiro.FileSystem.Core;

namespace DuplicateFinder.Console;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        return await Parser.Default.ParseArguments<CopyUniqueImagesOptions, CopyDuplicateImagesOptions>(args)
            .MapResult(
                (CopyUniqueImagesOptions opts) => RunDeduplicateAsync(opts),
                (CopyDuplicateImagesOptions opts) => RunCopyDuplicatesAsync(opts),
                errs => Task.FromResult(1));
    }

    private static async Task<int> RunDeduplicateAsync(CommonOptions opts)
    {
        return await RunOperationAsync(opts, (store, sources, destination, threshold) =>
            store.CopyUnique(sources, destination, threshold));
    }

    private static async Task<int> RunCopyDuplicatesAsync(CommonOptions opts)
    {
        return await RunOperationAsync(opts, (store, sources, destination, threshold) =>
            store.CopyDuplicates(sources, destination, threshold));
    }

    private static async Task<int> RunOperationAsync(CommonOptions opts,
        Func<global::DuplicateFinder.DuplicateFinder, IEnumerable<ZafiroPath>, ZafiroPath, double, Task<Result>> operation)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        if (opts.SimilarityThresholdPercentage < 0 || opts.SimilarityThresholdPercentage > 100)
        {
            Log.Error("Similarity threshold percentage must be between 0 and 100.");
            return 1;
        }

        var similarityThreshold = opts.SimilarityThresholdPercentage / 100.0;

        var destination = opts.OutputDirectory;
        var sources = opts.InputDirectories.Select(path => (ZafiroPath)path).ToArray();

        var store = new global::DuplicateFinder.DuplicateFinder(new AverageHash());
        var result = await operation(store, sources, destination, similarityThreshold);

        if (result.IsFailure)
        {
            Log.Error("Operation failed: {Error}", result.Error);
            return 1;
        }

        Log.Information("Operation completed successfully.");
        return 0;
    }
}