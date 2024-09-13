using CoenM.ImageHash;
using CSharpFunctionalExtensions;
using Zafiro.FileSystem.Core;

namespace DuplicateFinder;

public static class Misc
{
    public static bool IsImage(string argName)
    {
        var extensions = new[] { "png", "jpg", "jpeg", "bmp", "gif" };
        var maybeExtension = ((ZafiroPath)argName).Extension();
        return maybeExtension.Map(ext => extensions.Contains(ext)).GetValueOrDefault();
    }

    public static double CalculateDistance(HashedImage a, HashedImage b)
    {
        return 1 - CompareHash.Similarity(a.Hash, b.Hash) / 100;
    }

    public static HashedImage ChooseImage(IEnumerable<HashedImage> enumerable)
    {
        return enumerable.OrderByDescending(x => x.File.Length).First();
    }
}