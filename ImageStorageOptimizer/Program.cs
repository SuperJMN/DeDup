using CoenM.ImageHash.HashAlgorithms;
using CSharpFunctionalExtensions;
using ImageStorageOptimizer;
using Serilog;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.FileSystem.Core;
using FileSystem = Zafiro.FileSystem.Local.FileSystem;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var algorithm = new AverageHash();

var fileSystem = new FileSystem(new System.IO.Abstractions.FileSystem());

var sources = new ZafiroPath[]
{
    "/media/jmn/Store/OneDrive/Fotos",
};

var destination = "home/jmn/Escritorio/Output";

var allFilesResult = await Operations.GetFilesFrom(fileSystem, sources).LogInfo("Getting files from {Sources}", sources.Cast<object>().ToArray());
var directoryResult = await fileSystem.GetDirectory(destination);
var uniqueFilesResult = await allFilesResult.Bind(files => Operations.GetUniqueFiles(algorithm, files)).LogInfo("Getting unique files");

var execution = from outputDirectory in directoryResult
    from files in uniqueFilesResult
    select Operations.CopyFilesTo(files, outputDirectory);

await execution
    .UnrollBind()
    .LogInfo("Execution completed");

public static class Mixin
{
    public static Result LogInfo(this Result result, string str, params object[] propertyValues) => result
        .Tap(() => Log.Information(str, propertyValues))
        .TapError(Log.Error);
    
    public static Result<T> LogInfo<T>(this Result<T> result, string str, params object[] propertyValues) => result
        .Tap(() => Log.Information(str, propertyValues))
        .TapError(Log.Error);
    
    public static Task<Result<T>> LogInfo<T>(this Task<Result<T>> result, string str, params object[] propertyValues) => result
        .Tap(() => Log.Information(str, propertyValues))
        .TapError(Log.Error);
    
    public static Task<Result> LogInfo(this Task<Result> result, string str, params object[] propertyValues) => result
        .Tap(() => Log.Information(str, propertyValues))
        .TapError(Log.Error);
}