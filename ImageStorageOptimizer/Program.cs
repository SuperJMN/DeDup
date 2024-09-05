using CoenM.ImageHash.HashAlgorithms;
using ImageStorageOptimizer;
using Serilog;
using Zafiro.FileSystem.Core;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var destination = "home/jmn/Escritorio/Output";

var sources = new ZafiroPath[]
{
    //"/media/jmn/Store/OneDrive/Fotos",
    //"home/jmn/Escritorio/Test",
    "media/jmn/Store/FotoSource/Cámara de Pablo",
};

var store = new ImageStore(new AverageHash());
await store.Simplify(sources, destination);