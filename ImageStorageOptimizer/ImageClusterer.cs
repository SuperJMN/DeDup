using Dbscan;
using Serilog;
using Zafiro.Mixins;

namespace ImageStorageOptimizer;

public class ImageClusterer<T>
{
    // Method that calculates similarity between two images (simulated)
    public ImageClusterer(Func<T, T, double> imageDistance, Func<IEnumerable<T>, T> chooseBestImage)
    {
        ImageDistance = imageDistance;
        ChooseBestImage = chooseBestImage;
    }

    // Method that chooses the best image among several similar ones
    private Func<IEnumerable<T>, T> ChooseBestImage { get; }

    // Distance function based on similarity
    private Func<T, T, double> ImageDistance { get; }

    public List<T> GetUniqueItems(List<T> images)
    {
        var dbscan = new DbscanAlgorithm<T>((arg1, arg2) => ImageDistance(arg1, arg2));
        
        // Set DBSCAN parameters
        var result = dbscan.ComputeClusterDbscan(images.ToArray(), epsilon: 0.05, minimumPoints: 2);

        // Process the results
        var clusteredImages = result.Clusters
            .Select(cluster =>
            {
                var bestImage = ChooseBestImage(cluster.Value.Select(x => x.Feature));
                LogCluster(bestImage, cluster.Value.Select(x => x.Feature).ToList());
                return bestImage;
            })
            .ToList();

        // Add noise points (not clustered) to the final list
        clusteredImages.AddRange(result.Noise.Select(n => n.Feature));

        return clusteredImages;
    }

    private static void LogCluster(T chosen, List<T> cluster)
    {
        Log.Information("Detected duplication: {Cluster} are duplicates of {Original}", cluster.JoinWithCommas(), chosen);
    }
}
