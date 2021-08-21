using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrackSharp.Api.Utils
{
    // https://github.com/StephenCleary/AsyncEx/blob/master/src/Nito.AsyncEx.Tasks/CancellationTokenTaskSource.cs
    public sealed class CancellationTokenTaskSource<T> : IDisposable
    {
        private readonly IDisposable? _registration;

        public CancellationTokenTaskSource(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Task = System.Threading.Tasks.Task.FromCanceled<T>(cancellationToken);
                return;
            }
            
            var tcs = new TaskCompletionSource<T>();
            _registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken), false);
            Task = tcs.Task;
        }

        public Task<T> Task { get; private set; }

        public void Dispose() => 
            _registration?.Dispose();
    }
}