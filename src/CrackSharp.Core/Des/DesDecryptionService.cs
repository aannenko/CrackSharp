using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CrackSharp.Core
{
    public class DesDecryptionService
    {
        private static readonly Regex _stringValidator = new Regex("^[a-zA-Z0-9./]+$");

        private readonly ConcurrentDictionary<(int, string, string), Task<string>> _runningItems =
            new ConcurrentDictionary<(int, string, string), Task<string>>();
        private readonly ILogger<DesDecryptionService>? _logger;
        private readonly IMemoryCache? _cache;

        public DesDecryptionService(ILogger<DesDecryptionService>? logger = null, IMemoryCache? cache = null)
        {
            _logger = logger;
            _cache = cache;
        }

        public Task<string> DecryptAsync(string hash, int maxWordLength, string? chars = null, CancellationToken token = default)
        {
            if (hash?.Length != 13 || !_stringValidator.IsMatch(hash))
                throw new ArgumentException(
                    "Value must consist of exactly 13 chars that exist in the set [a-zA-Z0-9./].", nameof(hash));

            if (maxWordLength < 1 || maxWordLength > 8)
                throw new ArgumentException("Value must be between 1 and 8.", nameof(maxWordLength));

            if (TryGetCachedValue(ref hash, out var result))
            {
                _logger?.LogInformation($"The {nameof(hash)} '{hash}' " +
                    $"is already decrypted and corresponds to '{result}'.");

                return Task.FromResult(result);
            }

            if (!AreCharsValid(ref chars, out var newChars))
                _logger?.LogWarning($"Argument {nameof(chars)} has invalid value '{chars}', " +
                    $"it will be replaced with '{newChars}'.");

            _logger?.LogInformation($"Decrypting {nameof(hash)} '{hash}' " +
                $"with {nameof(maxWordLength)} = {maxWordLength} and {nameof(chars)} = '{newChars}'.");

            return _runningItems.GetOrAdd((maxWordLength, hash, newChars), _ =>
            {
                Func<string> decryption = () =>
                {
                    try
                    {
                        return Decrypt(ref hash, maxWordLength, ref newChars, token);
                    }
                    finally
                    {
                        _runningItems.TryRemove((maxWordLength, hash, newChars), out var completedTask);
                    }
                };

                return _cache == null
                    ? Task.Run(decryption, token)
                    : _cache.GetOrCreateAsync(hash, cacheEntry => Task.Run(() =>
                    {
                        var result = decryption();
                        cacheEntry.Size = result.Length;
                        return result;
                    }, token));
            });
        }

        private bool TryGetCachedValue(ref string hash, out string result)
        {
            result = string.Empty;
            return _cache != null && _cache.TryGetValue<string>(hash, out result);
        }

        private string Decrypt(ref string hash, int maxWordLength, ref string chars,
            CancellationToken token)
        {
            var lastCharPosition = chars.Length - 1;
            var salt = hash.AsSpan().Slice(0, 2);
            for (int i = 0; i < maxWordLength; i++)
            {
                Span<char> buffer = stackalloc char[i + 1];
                if (TryDecryptRecursive(ref hash, salt, ref chars, lastCharPosition,
                    buffer, 0, i, token))
                {
                    var result = buffer.ToString();
                    _logger?.LogInformation($"Decryption of the {nameof(hash)} '{hash}' " +
                        $"with {nameof(maxWordLength)} = {maxWordLength} " +
                        $"and {nameof(chars)} = '{chars}' succeeded. " +
                        $"The {nameof(hash)} '{hash}' corresponds to '{result}'.");

                    return result;
                }
            }

            throw new DecryptionFailedException($"The {nameof(hash)} '{hash}' " +
                $"does not correspond to any combination of {nameof(chars)} '{chars}' " +
                $"up to {maxWordLength} chars long.");
        }

        private bool TryDecryptRecursive(ref string hash, ReadOnlySpan<char> salt,
            ref string chars, int lastCharPosition,
            Span<char> buffer, int position, int maxPosition, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (TryGetCachedValue(ref hash, out var value))
            {
                value.AsSpan().CopyTo(buffer);
                return true;
            }

            for (int i = 0; i <= lastCharPosition; i++)
            {
                buffer[position] = chars[i];
                if (position < maxPosition)
                {
                    if (TryDecryptRecursive(ref hash, salt, ref chars, lastCharPosition,
                        buffer, position + 1, maxPosition, token))
                        return true;
                }
                else
                {
                    if (hash == DesEncryptionService.Encrypt(salt, buffer))
                        return true;
                }
            }

            return false;
        }

        private static bool AreCharsValid(ref string? chars, out string newChars)
        {
            newChars = chars == null || !_stringValidator.IsMatch(chars)
                ? "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
                : new string(chars.Distinct().ToArray());

            return newChars == chars;
        }
    }
}
