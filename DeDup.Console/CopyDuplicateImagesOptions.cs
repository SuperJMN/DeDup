using CommandLine;

namespace DuplicateFinder.Console;

[Verb("copyduplicates", HelpText = "Copy duplicate images to the output directory.")]
public class CopyDuplicateImagesOptions : CommonOptions
{
}