using CrackSharp.Api.Common;
using CrackSharp.Api.Common.Services;
using CrackSharp.Api.Des.DTO;
using CrackSharp.Core.Common.BruteForce;
using CrackSharp.Core.Des;
using CrackSharp.Core.Des.BruteForce;
using System.Collections.Concurrent;

namespace CrackSharp.Api.Des.Services;

public sealed class DesBruteForceDecryptionService : IDisposable
{
    private readonly ConcurrentDictionary<DesDecryptRequest, AwaiterTaskSource<string>> _awaiters = new();
    private readonly ILogger<DesBruteForceDecryptionService> _logger;
    private readonly DecryptionMemoryCache<string, string> _cache;

    public DesBruteForceDecryptionService(
        ILogger<DesBruteForceDecryptionService> logger,
        DecryptionMemoryCache<string, string> cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public ValueTask<string> DecryptAsync(DesDecryptRequest request, CancellationToken cancellationToken)
    {
        var (hash, _, _) = request;
        if (_cache.TryGetValue(hash, out var text))
        {
            _logger.LogDebug($"Decrypted value of the {nameof(hash)} '{{{nameof(hash)}}}' was found in cache.", hash);
            return new ValueTask<string>(text);
        }

        return new ValueTask<string>(StartDecryptionAsync(request, cancellationToken));
    }

    private async Task<string> StartDecryptionAsync(DesDecryptRequest request, CancellationToken cancellationToken)
    {
        var (hash, maxTextLength, chars) = request;
        var taskId = $"{hash}-{DateTime.UtcNow:o}";
        _logger.LogDebug(
            $"Starting a decryption task '{{{nameof(taskId)}}}'. Parameters used: " +
            $"{nameof(maxTextLength)} = {{{nameof(maxTextLength)}}}, {nameof(chars)} = '{{{nameof(chars)}}}'.",
            taskId, maxTextLength, chars);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var cacheTask = _cache.AwaitValue(hash, linkedCts.Token);
        var decryptTask = _awaiters.GetOrAdd(request, StartDecryptionAndGetAwaiter).GetAwaiterTask(linkedCts.Token);

        var firstToComplete = await Task.WhenAny(cacheTask, decryptTask).ConfigureAwait(false);
        linkedCts.Cancel();
        var text = await firstToComplete;

        _cache.GetOrCreate(hash, text);

        if (firstToComplete == decryptTask)
            _logger.LogDebug($"Decryption task '{{{nameof(taskId)}}}' succeeded.", taskId);
        else
            _logger.LogDebug($"Decryption task '{{{nameof(taskId)}}}' succeeded, will use the cached value.", taskId);

        return text;
    }

    private AwaiterTaskSource<string> StartDecryptionAndGetAwaiter(DesDecryptRequest request) =>
        AwaiterTaskSource<string>.Run(
            static async (requestAwaitersPair, cancellationToken) =>
            {
                var (request, awaiters) = requestAwaitersPair;
                var (hash, maxTextLength, characters) = request;
                try
                {
                    return await DesDecryptor.DecryptAsync(
                        hash,
                        new BruteForceEnumerable(new DesBruteForceParams(maxTextLength, characters)),
                        cancellationToken)
                        .ConfigureAwait(false);
                }
                finally
                {
                    awaiters.TryRemove(request, out _);
                }
            },
            (request, _awaiters));

    public void Dispose() =>
        _cache.Dispose();
}
