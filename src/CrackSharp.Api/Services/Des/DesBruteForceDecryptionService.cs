using System.Collections.Concurrent;
using CrackSharp.Api.Utils;
using CrackSharp.Core.Common.BruteForce;
using CrackSharp.Core.Des;
using CrackSharp.Core.Des.BruteForce;

namespace CrackSharp.Api.Services.Des;

public class DesBruteForceDecryptionService
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

    public ValueTask<string> DecryptAsync(string hash, int maxTextLength, string chars,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Decryption of a {HashParam} '{Hash}' with {MaxTextLengthParam} = {MaxTextLength} " +
            "and {CharsParam} = '{Chars}' requested.",
            nameof(hash), hash, nameof(maxTextLength), maxTextLength, nameof(chars), chars);

        if (_cache.TryGetValue(hash, out var text))
        {
            _logger.LogInformation(
                "Decrypted value of the {HashParam} '{Hash}' was found in cache; the value is '{Text}'.",
                nameof(hash), hash, text);

            return new ValueTask<string>(text);
        }

        return new ValueTask<string>(StartDecryptionAsync(hash, maxTextLength, chars, cancellationToken));
    }

    private async Task<string> StartDecryptionAsync(string hash, int maxTextLength, string chars,
        CancellationToken cancellationToken)
    {
        var taskId = $"{hash} - {DateTime.UtcNow:o}";
        _logger.LogInformation(
            "Starting a decryption task '{TaskId}'. Parameters used: " +
            "{MaxTextLengthParam} = {MaxTextLength}, {CharsParam} = '{Chars}'.",
            taskId, nameof(maxTextLength), maxTextLength, nameof(chars), chars);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var cacheTask = _cache.AwaitValue(hash, linkedCts.Token);
        var decryptTask = _awaiters.GetOrAdd(new(hash, new(maxTextLength, chars)), GetAwaiter)
            .GetAwaiterTask(linkedCts.Token);

        var firstToComplete = await Task.WhenAny(cacheTask, decryptTask);
        linkedCts.Cancel();
        var text = await firstToComplete;

        _cache.GetOrCreate(hash, text);

        if (firstToComplete == decryptTask)
            _logger.LogInformation(
                "Decryption task '{TaskId}' succeeded. Parameters used: {MaxTextLengthParam} = {MaxTextLength}, " +
                "{CharsParam} = '{Chars}'. The {HashParam} '{Hash}' corresponds to '{Text}'.",
                taskId, nameof(maxTextLength), maxTextLength, nameof(chars), chars, nameof(hash), hash, text);
        else
            _logger.LogInformation(
                "Decryption task '{TaskId}' succeeded. Another task successfully " +
                "decrypted the {HashParam} '{Hash}' and it corresponds to '{Text}'.",
                taskId, nameof(hash), hash, text);

        return text;
    }

    private AwaiterTaskSource<string> GetAwaiter(HashAndParams hashAndParams)
    {
        var (hash, prm) = hashAndParams;
        var awaiter = new AwaiterTaskSource<string>(token =>
            DesDecryptor.DecryptAsync(hash, new BruteForceEnumerable(prm), token));

        awaiter.Task.ContinueWith(t =>
            _awaiters.TryRemove(hashAndParams, out _), TaskScheduler.Default);

        return awaiter;
    }
}
