using CrackSharp.Core.Des;

namespace CrackSharp.Api.Services.Des;

public sealed class DesEncryptionService
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

        _logger.LogInformation(
            "Encryption of {TextParam} '{Text}' with {SaltDescription} requested.",
            nameof(text), text, saltDescription);

        var trimmedText = text.Length <= 8 ? text : text[..8];
        Span<char> hashBuffer = stackalloc char[13];
        if (isSaltEmpty)
            DesEncryptor.Encrypt(trimmedText, hashBuffer);
        else
            DesEncryptor.Encrypt(trimmedText, salt, hashBuffer);

        var hash = hashBuffer.ToString();
        _cache.GetOrCreate(hash, trimmedText);

        _logger.LogInformation(
            "Encryption of {TextParam} '{Text}' with {SaltDescription} succeeded. Encrypted value is '{Hash}'.",
            nameof(text), text, saltDescription, hash);

        return hash;
    }
}
