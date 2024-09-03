using CoenM.ImageHash.HashAlgorithms;
using CSharpFunctionalExtensions;
using ImageStorageOptimizer;
using Serilog;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.FileSystem.Local;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var algo = new AverageHash();

var fs = new FileSystem(new System.IO.Abstractions.FileSystem());

var input = await Operations.GetFilesFrom(fs, "home/jmn/Escritorio/Test");
var output = await fs.GetDirectory("home/jmn/Escritorio/Output");
var files = await input.Bind(x => Operations.GetFiles(algo, x));

var execution = from o in output
    from f in files
    select Operations.CopyFilesTo(f, o);

await execution.UnrollBind().Log();