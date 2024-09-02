using CoenM.ImageHash.HashAlgorithms;
using CSharpFunctionalExtensions;
using ImageStorageOptimizer;
using Zafiro.FileSystem.Local;

var algo = new AverageHash();

var fs = new FileSystem(new System.IO.Abstractions.FileSystem());

var input = fs.GetDirectory("home/jmn/Escritorio/Test");
var output = fs.GetDirectory("home/jmn/Escritorio/Output");
var files = input.Bind(x => Operations.GetFiles(algo, x));

var result = await from o in output
    from f in files
    select Operations.CopyFilesTo(f, o);