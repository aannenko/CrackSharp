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

            string hash;
            string textAndSalt;
            if (isSaltEmpty)
            {
                hash = DesEncryptor.Encrypt(text);
                salt = hash.Substring(0, 2);
                textAndSalt = text + salt;
            }
            else
            {
                textAndSalt = text + salt;
                if (_cache.TryGetValue<string>(textAndSalt, out var cachedHash))
                {
                    _logger.LogInformation($"Encrypted value of the {nameof(text)} '{text}' " +
                        $"was found in cache; the value is '{cachedHash}'.");

                    return cachedHash;
                }

                hash = DesEncryptor.Encrypt(text, salt);
            }

            _cache.GetOrCreate(textAndSalt, cacheEntry =>
                {
                    cacheEntry.Size = 13;
                    return hash;
                });

            _logger.LogInformation($"Encryption of {nameof(text)} '{text}' with {saltDescription} succeeded. " +
                $"Encrypted value is '{hash}'.");

            return hash;
        }
    }
}