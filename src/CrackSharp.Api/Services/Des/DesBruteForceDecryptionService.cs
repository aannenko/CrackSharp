using System.Collections.Concurrent;
using CrackSharp.Api.Utils;
using CrackSharp.Core.Common.BruteForce;
using CrackSharp.Core.Des;
using CrackSharp.Core.Des.BruteForce;

namespace CrackSharp.Api.Services.Des;

public sealed class DesBruteForceDecryptionService
{
    private readonly record struct HashAndParams(string Hash, DesBruteForceParams Parameters);

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

    public ValueTask<string> DecryptAsync(
        string hash,
        int maxTextLength,
        string chars,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            $"Decryption of a {nameof(hash)} '{{{nameof(hash)}}}' with {nameof(maxTextLength)} " +
            $"'{{{nameof(maxTextLength)}}}' and {nameof(chars)} '{{{nameof(chars)}}}' requested.",
            hash, maxTextLength, chars);

        if (_cache.TryGetValue(hash, out var text))
        {
            _logger.LogInformation(
                $"Decrypted value of the {nameof(hash)} '{{{nameof(hash)}}}' was found in cache; " +
                $"the value is '{{{nameof(text)}}}'.",
                hash, text);

            return new ValueTask<string>(text);
        }

        return new ValueTask<string>(StartDecryptionAsync(hash, maxTextLength, chars, cancellationToken));
    }

    private async Task<string> StartDecryptionAsync(
        string hash,
        int maxTextLength,
        string chars,
        CancellationToken cancellationToken)
    {
        var taskId = $"{hash} - {DateTime.UtcNow:o}";
        _logger.LogInformation(
            $"Starting a decryption task '{{{nameof(taskId)}}}'. Parameters used: " +
            $"{nameof(maxTextLength)} = {{{nameof(maxTextLength)}}}, {nameof(chars)} = '{{{nameof(chars)}}}'.",
            taskId, maxTextLength, chars);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var cacheTask = _cache.AwaitValue(hash, linkedCts.Token);
        var decryptTask = _awaiters.GetOrAdd(new(hash, new(maxTextLength, chars)), StartDecryptionAndGetAwaiter)
            .GetAwaiterTask(linkedCts.Token);

        var firstToComplete = await Task.WhenAny(cacheTask, decryptTask);
        linkedCts.Cancel();
        var text = await firstToComplete;

        _cache.GetOrCreate(hash, text);

        if (firstToComplete == decryptTask)
            _logger.LogInformation(
                $"Decryption task '{{{nameof(taskId)}}}' succeeded. Parameters used: " +
                $"{nameof(maxTextLength)} = {{{nameof(maxTextLength)}}}, {nameof(chars)} = '{{{nameof(chars)}}}'. " +
                $"The {nameof(hash)} '{{{nameof(hash)}}}' corresponds to '{{{nameof(text)}}}'.",
                taskId, maxTextLength, chars, hash, text);
        else
            _logger.LogInformation(
                $"Decryption task '{{{nameof(taskId)}}}' succeeded. Another task successfully decrypted " +
                $"the {nameof(hash)} '{{{nameof(hash)}}}' and it corresponds to '{{{nameof(text)}}}'.",
                taskId, hash, text);

        return text;
    }

    private AwaiterTaskSource<string> StartDecryptionAndGetAwaiter(HashAndParams hashAndParams)
    {
        var (hash, parameters) = hashAndParams;
        var awaiter = new AwaiterTaskSource<string>(token =>
            DesDecryptor.DecryptAsync(hash, new BruteForceEnumerable(parameters), token));

        awaiter.Task.ContinueWith(task => _awaiters.TryRemove(hashAndParams, out _), TaskScheduler.Default);
        return awaiter;
    }
}
