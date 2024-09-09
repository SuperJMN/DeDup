using Dbscan;

namespace ImageStorageOptimizer;

public class Clusterer<T>(Func<T, T, double> imageDistance)
{
    private Func<T, T, double> ImageDistance { get; } = imageDistance;

    public List<List<T>> GetClusters(IList<T> images, double epsilon)
    {
        var dbscan = new DbscanAlgorithm<T>(ImageDistance);

        var result = dbscan.ComputeClusterDbscan(images.ToArray(), epsilon, 1);

        return result.Clusters
            .Select(cluster => cluster.Value.Select(x => x.Feature).ToList())
            .ToList();
    }
}