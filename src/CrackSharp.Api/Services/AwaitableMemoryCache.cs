using CrackSharp.Api.Awaiting;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace CrackSharp.Api.Services;

internal sealed class AwaitableMemoryCache<TKey, TValue>(
    IMemoryCache cache,
    IEqualityComparer<TKey>? keyComparer = null)
    where TKey : notnull
{
    private readonly record struct AwaiterCompletionSourcePair(
        AwaiterTaskSource<TValue> Awaiter,
        TaskCompletionSource<TValue> CompletionSource);

    private readonly ConcurrentDictionary<TKey, Lazy<AwaiterCompletionSourcePair>> _awaiters = new(keyComparer);

    public Task<TValue> AwaitValueAsync(TKey key, CancellationToken cancellationToken)
    {
        if (TryGetValue(key, out var value))
            return Task.FromResult(value);

        var (awaiter, taskCompletionSource) = _awaiters.GetOrAdd(
            key,
            static (key, awaiters) => new(() =>
            {
                var taskCompletionSource = new TaskCompletionSource<TValue>();
                var awaiter = AwaiterTaskSource<TValue>.Run(
                    static (tcs, ct) => tcs.Task.WaitAsync(ct),
                    taskCompletionSource);

                awaiter.Task.ContinueWith(task => awaiters.TryRemove(key, out _), TaskScheduler.Default);
                return new(awaiter, taskCompletionSource);
            }),
            _awaiters).Value;

        if (TryGetValue(key, out value))
            taskCompletionSource.TrySetResult(value);

        return awaiter.GetAwaiterTask(cancellationToken);
    }

    public TValue GetOrCreate(TKey key, Func<ICacheEntry, TValue> factory)
    {
        var value = cache.GetOrCreate(key, factory);
        if (_awaiters.TryGetValue(key, out var lazyPair))
            lazyPair.Value.CompletionSource.TrySetResult(value!);

        return value!;
    }

    public bool TryGetValue(TKey key, out TValue value) =>
        cache.TryGetValue(key, out value!);

    public void Remove(TKey key) =>
        cache.Remove(key);
}
