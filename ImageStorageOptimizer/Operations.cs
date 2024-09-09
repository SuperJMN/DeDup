using CoenM.ImageHash;
using CSharpFunctionalExtensions;
using Serilog;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.FileSystem.Core;
using Zafiro.FileSystem.Mutable;
using Zafiro.FileSystem.Readonly;
using Zafiro.Misc;
using Zafiro.Mixins;

namespace ImageStorageOptimizer;

public static class Operations
{
    public static Task<Result<IEnumerable<IFile>>> GetFilesFrom(IMutableFileSystem filesystem, params ZafiroPath[] paths)
    {
        return paths.Select(x => filesystem.GetDirectory(x)
                .Bind(dir => dir.ToDirectory())
                .Map(dir => dir.AllFiles()))
            .CombineSequentially()
            .Map(x => x.Flatten());
    }

    private static Task<Result> CreateNewFile(IMutableDirectory output, IFile file)
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

    public static bool IsImage(string argName)
    {
        var extensions = new[] { "png", "jpg", "jpeg", "bmp", "gif" };
        var maybeExtension = ((ZafiroPath)argName).Extension();
        return maybeExtension.Map(ext => extensions.Contains(ext)).GetValueOrDefault();
    }

    public static double CalculateDistance(HashedImageFile a, HashedImageFile b)
    {
        return 1 - CompareHash.Similarity(a.Hash, b.Hash) / 100;
    }

    public static HashedImageFile ChooseImage(IEnumerable<HashedImageFile> enumerable)
    {
        return enumerable.OrderByDescending(x => x.File.Length).First();
    }
}