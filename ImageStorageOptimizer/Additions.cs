using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CSharpFunctionalExtensions;

namespace ImageStorageOptimizer;

public static class Additions
{
    public static Task<Result> CombineInOrder<TResult>(this Task<Result<IEnumerable<Task<Result>>>> task, IScheduler? scheduler = default, int maxConcurrency = 1)
    {
        return task.Bind(tasks => CombineInOrder(tasks, scheduler));
    }

    public static async Task<Result> CombineInOrder(this IEnumerable<Task<Result>> enumerableOfTaskResults, IScheduler? scheduler = default)
    {
        var results = await enumerableOfTaskResults
            .Select(task => Observable.FromAsync(() => task, scheduler ?? Scheduler.Default))
            .Concat()
            .ToList();

        return results.Combine();
    }
}