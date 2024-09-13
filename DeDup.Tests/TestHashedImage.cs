namespace DuplicateFinder.Tests;

public class TestHashedImage
{
    public TestHashedImage(double x, double y, int quality)
    {
        X = x;
        Y = y;
        Quality = quality;
    }

    public int Quality { get; }
    public double X { get; }
    public double Y { get; }
}