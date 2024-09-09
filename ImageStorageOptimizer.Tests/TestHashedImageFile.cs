namespace ImageStorageOptimizer.Tests;

public class TestHashedImageFile
{
    public TestHashedImageFile(double x, double y, int quality)
    {
        X = x;
        Y = y;
        Quality = quality;
    }

    public int Quality { get; }
    public double X { get; }
    public double Y { get; }
}