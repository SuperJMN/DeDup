using DbscanImplementation;

namespace ImageStorageOptimizer.Tests;

public class ImageClustererTests
{
    [Fact]
    public void Test_clustering()
    {
        var imageProcessor = new ImageClusterer<TestHashedImageFile>(
            (a, b) => CalcularDistanciaNormalizada(a.X, a.Y, b.X, b.Y),
            images => images.OrderByDescending(x => x.Quality).First()
        );
        
        var list = imageProcessor.GetUniqueItems([
            new TestHashedImageFile(0, 0, 98),
            new TestHashedImageFile(0, 1, 90),
            new TestHashedImageFile(4, 4, 100),
            new TestHashedImageFile(20000, 2000, 94),
            new TestHashedImageFile(2000, 2000, 94),
            new TestHashedImageFile(400, 340, 94),
        ]);

        Assert.Equal(3, list.Count); // Esperamos 3 clusters
    }

    static double CalcularDistanciaNormalizada(double x1, double y1, double x2, double y2)
    {
        // Normalizar las coordenadas a un rango de 0 a 1
        double maxCoord = 20000; // El valor máximo de coordenada en tus datos
        x1 /= maxCoord; x2 /= maxCoord;
        y1 /= maxCoord; y2 /= maxCoord;

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

