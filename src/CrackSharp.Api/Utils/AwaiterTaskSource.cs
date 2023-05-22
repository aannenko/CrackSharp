namespace CrackSharp.Api.Utils;

public class AwaiterTaskSource<T>
{
    private readonly CancellationTokenSource _taskCts = new();
    private int _awaitersCount = 0;

    internal AwaiterTaskSource(Func<CancellationToken, Task<T>> taskFactory) =>
        Task = taskFactory(_taskCts.Token);

    public Task<T> Task { get; }

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
                _taskCts.Cancel();
        }
    }
}

public sealed class AwaiterTaskSource<T, K> : AwaiterTaskSource<T>
{
    internal AwaiterTaskSource(Func<CancellationToken, Task<T>> taskFactory, K state) : base(taskFactory) =>
        State = state;

    public K State { get; }
}
