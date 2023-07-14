namespace CrackSharp.Api.Common.Utils;

// https://github.com/StephenCleary/AsyncEx/blob/master/src/Nito.AsyncEx.Tasks/CancellationTokenTaskSource.cs
public sealed class CancellationTokenTaskSource<TResult> : IDisposable
{
    private readonly IDisposable? _registration;

    public CancellationTokenTaskSource(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            Task = System.Threading.Tasks.Task.FromCanceled<TResult>(cancellationToken);
            return;
        }

        var tcs = new TaskCompletionSource<TResult>();
        _registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken), false);
        Task = tcs.Task;
    }

    public Task<TResult> Task { get; private set; }

    public void Dispose() =>
        _registration?.Dispose();
}
