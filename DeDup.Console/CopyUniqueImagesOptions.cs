using CommandLine;

namespace DuplicateFinder.Console;

[Verb("copyunique", HelpText = "Keep unique images and remove duplicates.")]
public class CopyUniqueImagesOptions : CommonOptions
{
}