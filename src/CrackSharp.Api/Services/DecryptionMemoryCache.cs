using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrackSharp.Api.Utils;
using Microsoft.Extensions.Caching.Memory;

namespace CrackSharp.Api.Services
{
    public class DecryptionMemoryCache<TKey, TValue> : IDisposable where TKey : notnull
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
            var tcs = new TaskCompletionSource<TValue>();
            var awaiterTask = _awaiters.GetOrAdd(key, k =>
            {
                var awaiter = new AwaiterTaskSource<TValue, TaskCompletionSource<TValue>>(() => tcs.Task, tcs);
                _ = awaiter.Completion.ContinueWith(t => _awaiters.TryRemove(k, out _));
                return awaiter;
            }).GetAwaiterTask(cancellationToken);

            if (TryGetValue(key, out var value))
                tcs.SetResult(value);

            return awaiterTask;
        }

        public TValue GetOrCreate(TKey key, Func<ICacheEntry, TValue> factory)
        {
            var value = _cache.GetOrCreate(key, factory);
            if (_awaiters.TryGetValue(key, out var awaiter))
                awaiter.State.TrySetResult(value);

            return value;
        }

        public bool TryGetValue(TKey key, out TValue value) =>
            _cache.TryGetValue(key, out value);

        public void Remove(TKey key) =>
            _cache.Remove(key);

        public void Dispose() =>
            _cache.Dispose();
    }
}