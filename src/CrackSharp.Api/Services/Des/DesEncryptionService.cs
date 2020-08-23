using CrackSharp.Core.Des;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CrackSharp.Api.Services.Des
{
    public class DesEncryptionService
    {
        private readonly ILogger<DesEncryptionService> _logger;
        private readonly IMemoryCache _cache;

        public DesEncryptionService(ILogger<DesEncryptionService> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        public string Encrypt(string text, string? salt = null)
        {
            var isSaltEmpty = string.IsNullOrWhiteSpace(salt);
            var saltDescription = isSaltEmpty ? $"empty {nameof(salt)}" : $"{nameof(salt)} '{salt}'";

            _logger.LogInformation($"Encryption of {nameof(text)} '{text}' with {saltDescription} requested.");

            string hash = isSaltEmpty
                ? DesEncryptor.Encrypt(text)
                : DesEncryptor.Encrypt(text, salt);

            _cache.GetOrCreate(hash, cacheEntry =>
            {
                var trimmedText = text.Length < 8 ? text : text.Substring(0, 8);
                cacheEntry.Size = trimmedText.Length;
                return trimmedText;
            });

            _logger.LogInformation($"Encryption of {nameof(text)} '{text}' with {saltDescription} succeeded. " +
                $"Encrypted value is '{hash}'.");

            return hash;
        }
    }
}