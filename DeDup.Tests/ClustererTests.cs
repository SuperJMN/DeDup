namespace DuplicateFinder.Tests;

public class ClustererTests
{
    [Fact]
    public void Test_clustering()
    {
        var imageProcessor = new Clusterer<TestHashedImage>(
            (a, b) => CalcularDistanciaNormalizada(a.X, a.Y, b.X, b.Y));

        var list = imageProcessor.GetClusters([
            new TestHashedImage(0, 0, 98),
            new TestHashedImage(0, 1, 90),
            new TestHashedImage(4, 4, 100),
            new TestHashedImage(20000, 2000, 94),
            new TestHashedImage(2000, 2000, 94),
            new TestHashedImage(400, 340, 94)
        ], 0.95);

        Assert.Equal(3, list.Count); // Esperamos 3 clusters
    }

    private static double CalcularDistanciaNormalizada(double x1, double y1, double x2, double y2)
    {
        // Normalizar las coordenadas a un rango de 0 a 1
        double maxCoord = 20000; // El valor máximo de coordenada en tus datos
        x1 /= maxCoord;
        x2 /= maxCoord;
        y1 /= maxCoord;
        y2 /= maxCoord;

        return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }
}

public interface IImage
{
    public string Id { get; }
}

public class Image : IImage
{
    public string Id { get; }
}