using System.Collections.Concurrent;
using CrackSharp.Api.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace CrackSharp.Api.Services;

public sealed class DecryptionMemoryCache<TKey, TValue> : IDisposable where TKey : notnull
{
    private readonly IMemoryCache _cache;

    private readonly ConcurrentDictionary<TKey, AwaiterTaskSource<TValue, TaskCompletionSource<TValue>>> _awaiters;

    public DecryptionMemoryCache(IMemoryCache cache, IEqualityComparer<TKey>? keyComparer = null)
    {
        _cache = cache;
        _awaiters = new(keyComparer);
    }

    public Task<TValue> AwaitValue(TKey key, CancellationToken cancellationToken)
    {
        static async Task<T> GetCancellableTcsTask<T>(TaskCompletionSource<T> tcs, CancellationToken cancellationToken)
        {
            using (cancellationToken.UnsafeRegister((tcs, token) =>
                ((TaskCompletionSource<T>)tcs!).TrySetCanceled(token), tcs))
                    return await tcs.Task;
        }

        var awaiter = _awaiters.GetOrAdd(key, key =>
        {
            var tcs = new TaskCompletionSource<TValue>();
            var awaiter = new AwaiterTaskSource<TValue, TaskCompletionSource<TValue>>(ct =>
                GetCancellableTcsTask(tcs, ct), tcs);

            awaiter.Task.ContinueWith(t => _awaiters.TryRemove(key, out _), TaskScheduler.Default);
            return awaiter;
        });

        if (TryGetValue(key, out var value))
            awaiter.State.TrySetResult(value);

        return awaiter.GetAwaiterTask(cancellationToken);
    }

    public TValue GetOrCreate(TKey key, Func<ICacheEntry, TValue> factory)
    {
        var value = _cache.GetOrCreate(key, factory);
        if (_awaiters.TryGetValue(key, out var awaiter))
            awaiter.State.TrySetResult(value!);

        return value!;
    }

    public bool TryGetValue(TKey key, out TValue value) =>
        _cache.TryGetValue(key, out value!);

    public void Remove(TKey key) =>
        _cache.Remove(key);

    public void Dispose() =>
        _cache.Dispose();
}
