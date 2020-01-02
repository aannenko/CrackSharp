using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CrackSharp.Core.Des;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CrackSharp.Api.Services.Des
{
    public class DesDecryptionService
    {
        private readonly ILogger<DesDecryptionService> _logger;
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, HashDecryption> _runningDecryptions =
            new ConcurrentDictionary<string, HashDecryption>();

        public DesDecryptionService(ILogger<DesDecryptionService> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public ValueTask<string> DecryptAsync(string hash, int maxTextLength, string chars, CancellationToken token)
        {
            _logger.LogInformation($"Decryption of {nameof(hash)} '{hash}' with " +
                $"{nameof(maxTextLength)} = {maxTextLength} and {nameof(chars)} = '{chars}' requested.");

            if (_cache.TryGetValue<string>(hash, out var text))
            {
                _logger.LogInformation($"Decrypted value of the {nameof(hash)} '{hash}' " +
                    $"was found in cache; the value is '{text}'.");

                return new ValueTask<string>(text);
            }

            _logger.LogInformation($"Decrypting {nameof(hash)} '{hash}' " +
                $"with {nameof(maxTextLength)} = {maxTextLength} and {nameof(chars)} = '{chars}'.");

            return new ValueTask<string>(StartDecryptionAsync(hash, maxTextLength, chars, token));
        }

        private Task<string> StartDecryptionAsync(string hash, int maxTextLength, string chars, CancellationToken token)
        {
            var hashDecryption = _runningDecryptions.GetOrAdd(hash, _ => new HashDecryption());
            return hashDecryption.GetOrAdd(maxTextLength, chars, async _ =>
            {
                using var linkedCts = hashDecryption.CreateLinkedTokenSource(token);
                Func<Task<string>> decryption = async () =>
                {
                    try
                    {
                        var text = await DesDecryptor.DecryptAsync(hash, maxTextLength, chars, linkedCts.Token);
                        _runningDecryptions.TryRemove(hash, out var _);
                        _logger.LogInformation($"Decryption of the {nameof(hash)} '{hash}' with " +
                            $"{nameof(maxTextLength)} = {maxTextLength} and {nameof(chars)} = '{chars}' succeeded. " +
                            $"The {nameof(hash)} '{hash}' corresponds to '{text}'.");

                        return text;
                    }
                    catch (OperationCanceledException)
                    {
                        if (_cache.TryGetValue(hash, out string text))
                        {
                            _logger.LogInformation($"Another task successfully decrypted " +
                                $"the {nameof(hash)} '{hash}' and it corresponds to '{text}'.");

                            return text;
                        }

                        throw;
                    }
                    finally
                    {
                        hashDecryption.TryRemove(maxTextLength, chars, out var _);
                    }
                };

                var text = await _cache.GetOrCreateAsync(hash, cacheEntry => Task.Run(async () =>
                {
                    var text = await decryption();
                    cacheEntry.Size = text.Length;
                    return text;
                }, token));

                hashDecryption.CancelAll();
                return text;
            });
        }

        private class HashDecryption
        {
            private readonly ConcurrentDictionary<(int, string), Task<string>> _tasks =
                new ConcurrentDictionary<(int, string), Task<string>>();

            private readonly CancellationTokenSource _cts = new CancellationTokenSource();

            public CancellationTokenSource CreateLinkedTokenSource(CancellationToken token) =>
                CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, token);

            public void CancelAll() => _cts.Cancel();

            public Task<string> GetOrAdd(int maxTextLength, string chars, Func<(int, string), Task<string>> taskFactory) =>
                _tasks.GetOrAdd((maxTextLength, chars), taskFactory);

            public bool TryRemove(int maxTextLength, string chars, out Task<string>? value) =>
                _tasks.TryRemove((maxTextLength, chars), out value);

        }
    }
}