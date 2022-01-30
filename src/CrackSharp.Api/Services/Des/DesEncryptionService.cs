using CrackSharp.Core.Des;

namespace CrackSharp.Api.Services.Des;

public class DesEncryptionService
{
    private readonly ILogger<DesEncryptionService> _logger;
    private readonly DecryptionMemoryCache<string, string> _cache;

    public DesEncryptionService(
        ILogger<DesEncryptionService> logger,
        DecryptionMemoryCache<string, string> cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public string Encrypt(string text, string? salt = null)
    {
        var isSaltEmpty = string.IsNullOrWhiteSpace(salt);
        var saltDescription = isSaltEmpty ? $"empty {nameof(salt)}" : $"{nameof(salt)} '{salt}'";

        _logger.LogInformation($"Encryption of {nameof(text)} '{text}' with {saltDescription} requested.");

        Span<char> hashBuffer = stackalloc char[13];
        if (isSaltEmpty)
            DesEncryptor.Encrypt(text, hashBuffer);
        else
            DesEncryptor.Encrypt(text, salt, hashBuffer);

        var hash = hashBuffer.ToString();
        _cache.GetOrCreate(hash, cacheEntry =>
        {
            var trimmedText = text.Length <= 8 ? text : text.Substring(0, 8);
            cacheEntry.Size = trimmedText.Length;
            return trimmedText;
        });

        _logger.LogInformation($"Encryption of {nameof(text)} '{text}' with {saltDescription} succeeded. " +
            $"Encrypted value is '{hash}'.");

        return hash;
    }
}
