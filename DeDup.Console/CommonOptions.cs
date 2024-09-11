using CommandLine;

namespace DuplicateFinder.Console;

public class CommonOptions
{
    [Option('o', "output", Required = true, HelpText = "Output directory for processed images.")]
    public string OutputDirectory { get; set; }

    [Option('t', "threshold", Required = true, HelpText = "Similarity threshold (0 to 100).")]
    public int SimilarityThresholdPercentage { get; set; }

    [Option('i', "input", Required = true, Min = 1, HelpText = "Input directories to search for images.")]
    public IEnumerable<string> InputDirectories { get; set; }
}