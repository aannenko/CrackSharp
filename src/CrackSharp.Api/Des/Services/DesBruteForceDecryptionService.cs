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
    private readonly ConcurrentDictionary<DesDecryptRequest, Lazy<AwaiterTaskSource<string>>> _awaiters = new();
    private readonly ILogger<DesBruteForceDecryptionService> _logger;
    private readonly AwaitableMemoryCache<string, string> _cache;

    public DesBruteForceDecryptionService(
        ILogger<DesBruteForceDecryptionService> logger,
        AwaitableMemoryCache<string, string> cache)
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
        var cacheTask = _cache.AwaitValueAsync(hash, linkedCts.Token);
        var decryptTask = _awaiters.GetOrAdd(request, DecryptLazy, _awaiters).Value.GetAwaiterTask(linkedCts.Token);

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

    public void Dispose() =>
        _cache.Dispose();

    private static Lazy<AwaiterTaskSource<string>> DecryptLazy(
        DesDecryptRequest request,
        ConcurrentDictionary<DesDecryptRequest, Lazy<AwaiterTaskSource<string>>> awaiters)
    {
        return new(() =>
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
                (request, awaiters)));
    }
}
