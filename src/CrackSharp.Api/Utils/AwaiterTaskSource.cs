using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrackSharp.Api.Utils
{
    public class AwaiterTaskSource<T>
    {
        private readonly TaskCompletionSource _completion = new();
        private readonly Lazy<Task<T>> _mainTask;
        private readonly CancellationTokenSource? _mainTaskCts;
        private int _awaitersCount = 0;

        internal AwaiterTaskSource(Func<Task<T>> taskFactory)
        {
            _mainTask = new(() => taskFactory(), true);
        }

        internal AwaiterTaskSource(Func<CancellationToken, Task<T>> taskFactory)
        {
            _mainTaskCts = new();
            _mainTask = new(() => taskFactory(_mainTaskCts.Token), true);
        }

        public Task Completion => _completion.Task;

        public async Task<T> GetAwaiterTask(CancellationToken cancellationToken = default)
        {
            if (Completion.IsCompleted)
                return await _mainTask.Value.ConfigureAwait(false);

            Interlocked.Increment(ref _awaitersCount);
            using var cancellation = new CancellationTokenTaskSource<T>(cancellationToken);
            var mainOrCancellation = await Task.WhenAny(_mainTask.Value, cancellation.Task).ConfigureAwait(false);
            try
            {
                return await mainOrCancellation.ConfigureAwait(false);
            }
            finally
            {
                if (!Completion.IsCompleted && Interlocked.Decrement(ref _awaitersCount) == 0)
                {
                    _completion.TrySetResult();
                    if (mainOrCancellation != _mainTask.Value)
                        _mainTaskCts?.Cancel();
                }
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
}