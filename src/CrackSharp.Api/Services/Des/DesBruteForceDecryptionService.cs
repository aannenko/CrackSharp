using System.Collections.Concurrent;
using CrackSharp.Api.Utils;
using CrackSharp.Core.Common.BruteForce;
using CrackSharp.Core.Des;
using CrackSharp.Core.Des.BruteForce;

namespace CrackSharp.Api.Services.Des;

public class DesBruteForceDecryptionService
{
    private sealed record HashAndParams(string Hash, DesBruteForceParams Parameters);

    private readonly ConcurrentDictionary<HashAndParams, AwaiterTaskSource<string>> _awaiters = new();
    private readonly ILogger<DesBruteForceDecryptionService> _logger;
    private readonly DecryptionMemoryCache<string, string> _cache;

    public DesBruteForceDecryptionService(
        ILogger<DesBruteForceDecryptionService> logger,
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
        _logger.LogInformation($"Starting a decryption task '{taskId}'. Parameters used: " +
            $"{nameof(maxTextLength)} = {maxTextLength}, {nameof(chars)} = '{chars}'.");

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var cacheTask = _cache.AwaitValue(hash, linkedCts.Token);
        var decryptTask = _awaiters.GetOrAdd(new(hash, new(maxTextLength, chars)), GetAwaiter)
            .GetAwaiterTask(linkedCts.Token);

        var firstToComplete = await Task.WhenAny(cacheTask, decryptTask);
        linkedCts.Cancel();
        var text = await firstToComplete;

        _cache.GetOrCreate(hash, cacheEntry =>
        {
            cacheEntry.Size = text.Length;
            return text;
        });

        _logger.LogInformation(firstToComplete == decryptTask
            ? $"Decryption task '{taskId}' succeeded. Parameters used: " +
                $"{nameof(maxTextLength)} = {maxTextLength}, {nameof(chars)} = '{chars}'. " +
                $"The {nameof(hash)} '{hash}' corresponds to '{text}'."
            : $"Decryption task '{taskId}' succeeded. Another task successfully " +
                $"decrypted the {nameof(hash)} '{hash}' and it corresponds to '{text}'.");

        return text;
    }

    private AwaiterTaskSource<string> GetAwaiter(HashAndParams hashAndParams)
    {
        var (hash, prm) = hashAndParams;
        var awaiter = new AwaiterTaskSource<string>(token =>
            DesDecryptor.DecryptAsync(hash, new BruteForceEnumerable(prm), token));

        _ = awaiter.Completion.ContinueWith(t =>
            _awaiters.TryRemove(hashAndParams, out _), TaskScheduler.Default);

        return awaiter;
    }
}
