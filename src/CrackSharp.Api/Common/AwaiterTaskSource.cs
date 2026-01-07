namespace CrackSharp.Api.Common;

public sealed class AwaiterTaskSource<TResult>
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private int _awaitersCount = 0;

    private AwaiterTaskSource(Task<TResult> task, CancellationTokenSource cancellationTokenSource)
    {
        Task = task;
        _cancellationTokenSource = cancellationTokenSource;
    }

    public Task<TResult> Task { get; }

    public async Task<TResult> GetAwaiterTask(CancellationToken cancellationToken = default)
    {
        if (Task.IsCompleted)
            return await Task.ConfigureAwait(false);

        Interlocked.Increment(ref _awaitersCount);

        try
        {
            return await Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (Interlocked.Decrement(ref _awaitersCount) is 0 && !Task.IsCompleted)
                await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
        }
    }

    public static AwaiterTaskSource<TResult> Run(Func<CancellationToken, Task<TResult>> taskFactory)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        return new(taskFactory(cancellationTokenSource.Token), cancellationTokenSource);
    }

    public static AwaiterTaskSource<TResult> Run<TArg>(
        Func<TArg, CancellationToken, Task<TResult>> taskFactory,
        TArg factoryArgument)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        return new(taskFactory(factoryArgument, cancellationTokenSource.Token), cancellationTokenSource);
    }
}
