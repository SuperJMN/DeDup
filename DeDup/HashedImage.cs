using ByteSizeLib;
using CoenM.ImageHash;
using CSharpFunctionalExtensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Zafiro.FileSystem.Core;
using Zafiro.FileSystem.Readonly;

namespace DuplicateFinder;

public class HashedImage
{
    private HashedImage(Image<Rgba32> image, ulong hash, IFile file)
    {
        Image = image;
        Hash = hash;
        File = file;
    }

    public Image<Rgba32> Image { get; }
    public ulong Hash { get; }
    public IFile File { get; }

    public override string ToString()
    {
        return $"{File} ({Image.Width}x{Image.Height} - {ByteSize.FromBytes(File.Length)})";
    }

    public static async Task<Result<HashedImage>> Create(IFile file, IImageHash imageHash)
    {
        return Result.Try(() => SixLabors.ImageSharp.Image.Load<Rgba32>(file.Bytes()))
            .Map(image =>
            {
                var hash = imageHash.Hash(image);
                return new HashedImage(image, hash, file);
            });
    }
}