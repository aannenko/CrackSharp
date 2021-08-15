using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CrackSharp.Api.Utils;
using CrackSharp.Core.Common.BruteForce;
using CrackSharp.Core.Des;
using CrackSharp.Core.Des.BruteForce;
using Microsoft.Extensions.Logging;

namespace CrackSharp.Api.Services.Des
{
    public class DesBruteForceDecryptionService
    {
        private sealed record HashAndParams(string Hash, DesBruteForceParams Parameters);

        private readonly ConcurrentDictionary<HashAndParams, AwaiterTaskSource<string>> _awaiters = new();
        private readonly ILogger<DesBruteForceDecryptionService> _logger;
        private readonly DecryptionMemoryCache<string, string> _cache;

        public DesBruteForceDecryptionService(ILogger<DesBruteForceDecryptionService> logger,
            DecryptionMemoryCache<string, string> cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public ValueTask<string> DecryptAsync(string hash, int maxTextLength, string chars,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Decryption of a {nameof(hash)} '{hash}' with " +
                $"{nameof(maxTextLength)} = {maxTextLength} and {nameof(chars)} = '{chars}' requested.");

            if (_cache.TryGetValue(hash, out var text))
            {
                _logger.LogInformation($"Decrypted value of the {nameof(hash)} '{hash}' " +
                    $"was found in cache; the value is '{text}'.");

                return new ValueTask<string>(text);
            }

            return new ValueTask<string>(StartDecryptionAsync(hash, maxTextLength, chars, cancellationToken));
        }

        private async Task<string> StartDecryptionAsync(string hash, int maxTextLength, string chars,
            CancellationToken cancellationToken)
        {
            var taskId = $"{hash} - {DateTime.UtcNow.ToString("o")}";
            _logger.LogInformation($"Starting a decryption task '{taskId}' for a {nameof(hash)} '{hash}' " +
                $"with {nameof(maxTextLength)} = {maxTextLength} and {nameof(chars)} = '{chars}'.");

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var cacheTask = _cache.AwaitValue(hash, linkedCts.Token);
            var decryptTask = _awaiters.GetOrAdd(new(hash, new(maxTextLength, chars)), hp =>
            {
                var awaiter = new AwaiterTaskSource<string>(async token =>
                {
                    var (hash, prm) = hp;
                    var text = await DesDecryptor.DecryptAsync(hp.Hash, new BruteForceEnumerable(prm), token);
                    _logger.LogInformation($"Decryption task '{taskId}' for the {nameof(hash)} '{hash}' " +
                        $"with {nameof(prm.MaxTextLength)} = {prm.MaxTextLength} " +
                        $"and {nameof(prm.Characters)} = '{prm.Characters}' succeeded. " +
                        $"The {nameof(hash)} '{hash}' corresponds to '{text}'.");

                    return text;
                });

                _ = awaiter.Completion.ContinueWith(t => _awaiters.TryRemove(hp, out _));
                return awaiter;
            }).GetAwaiterTask(linkedCts.Token);

            var firstToComplete = await Task.WhenAny(cacheTask, decryptTask);
            var text = await firstToComplete;
            linkedCts.Cancel();
            _cache.GetOrCreate(hash, cacheEntry =>
            {
                cacheEntry.Size = text.Length;
                return text;
            });

            if (firstToComplete == cacheTask)
                _logger.LogInformation($"Decryption task '{taskId}' succeeded. Another task successfully " +
                    $"decrypted the {nameof(hash)} '{hash}' and it corresponds to '{text}'.");

            return text;
        }
    }
}