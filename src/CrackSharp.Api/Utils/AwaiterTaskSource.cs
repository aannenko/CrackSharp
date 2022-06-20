namespace CrackSharp.Api.Utils;

public class AwaiterTaskSource<T>
{
    private readonly Lazy<Task<T>> _lazyTask;
    private readonly CancellationTokenSource? _taskCts;
    private int _awaitersCount = 0;

    internal AwaiterTaskSource(Func<Task<T>> taskFactory)
    {
        _lazyTask = new(() => taskFactory(), true);
    }

    internal AwaiterTaskSource(Func<CancellationToken, Task<T>> taskFactory)
    {
        _taskCts = new();
        _lazyTask = new(() => taskFactory(_taskCts.Token), true);
    }

    public Task<T> Task => _lazyTask.Value;

    public async Task<T> GetAwaiterTask(CancellationToken cancellationToken = default)
    {
        if (Task.IsCompleted)
            return await Task.ConfigureAwait(false);

        Interlocked.Increment(ref _awaitersCount);

        using var cancellation = new CancellationTokenTaskSource<T>(cancellationToken);
        var mainOrCancellation = await System.Threading.Tasks.Task.WhenAny(Task, cancellation.Task)
            .ConfigureAwait(false);

        try
        {
            return await mainOrCancellation.ConfigureAwait(false);
        }
        finally
        {
            if (Interlocked.Decrement(ref _awaitersCount) == 0 && mainOrCancellation != Task)
                _taskCts?.Cancel();
        }
    }
}

public class AwaiterTaskSource<T, K> : AwaiterTaskSource<T>
{
    internal AwaiterTaskSource(Func<Task<T>> taskFactory, K state) : base(taskFactory)
    {
        State = state;
    }

    internal AwaiterTaskSource(Func<CancellationToken, Task<T>> taskFactory, K state) : base(taskFactory)
    {
        State = state;
    }

    public K State { get; init; }
}
