namespace CrackSharp.Api.Common;

public class AwaiterTaskSource<TResult>
{
    private readonly CancellationTokenSource _taskCts = new();
    private int _awaitersCount = 0;

    internal AwaiterTaskSource(Func<CancellationToken, Task<TResult>> taskFactory) =>
        Task = taskFactory(_taskCts.Token);

    public Task<TResult> Task { get; }

    public async Task<TResult> GetAwaiterTask(CancellationToken cancellationToken = default)
    {
        if (Task.IsCompleted)
            return await Task.ConfigureAwait(false);

        Interlocked.Increment(ref _awaitersCount);

        using var cancellation = new CancellationTokenTaskSource<TResult>(cancellationToken);
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

public sealed class AwaiterTaskSource<TResult, TState> : AwaiterTaskSource<TResult>
{
    internal AwaiterTaskSource(Func<CancellationToken, Task<TResult>> taskFactory, TState state) : base(taskFactory) =>
        State = state;

    public TState State { get; }
}
