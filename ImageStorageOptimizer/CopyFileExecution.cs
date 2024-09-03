using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CSharpFunctionalExtensions;
using ReactiveUI;
using Serilog;
using Zafiro.DataModel;
using Zafiro.FileSystem.Mutable;
using Zafiro.FileSystem.Readonly;
using Zafiro.Jobs.Execution;
using Zafiro.Jobs.Progress;

public class CopyFileExecution : IExecution
{
    public Maybe<ILogger> Logger { get; }
    private readonly BehaviorSubject<IProgress> progressSubject;

    public CopyFileExecution(IFile file, IMutableDirectory directory, Maybe<ILogger> logger)
    {
        Logger = logger;
        progressSubject = new BehaviorSubject<IProgress>(new CurrentOver<long>(0, file.Length));
        ReactiveCommand.Create(() => Copy(file, directory));
    }

    private async Task<Result> Copy(IFile file, IMutableDirectory directory)
    {
        var longSubject = new Subject<long>();
        var progress = longSubject.Select(l => new CurrentOver<long>(l, file.Length));
        using (progress.Subscribe(progressSubject))
        {
            using (new ProgressWatcher(file, longSubject))
            {
                return await file.CopyAndPreserveExisting(directory, Logger);
            }    
        }
    }

    public ReactiveCommandBase<Unit, Unit> Start { get; }
    public ReactiveCommandBase<Unit, Unit>? Stop { get; }
    
    public IObservable<IProgress> Progress => progressSubject.AsObservable();
}