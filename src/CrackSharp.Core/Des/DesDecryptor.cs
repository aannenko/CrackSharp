using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CrackSharp.Core.Des
{
    public static class DesDecryptor
    {
        private const string DefaultChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./";

        private static readonly Regex _stringValidator = new Regex("^[a-zA-Z0-9./]+$");

        public static Task<string> DecryptAsync(string hash, int maxTextLength = 8, string chars = DefaultChars,
            CancellationToken token = default)
        {
            if (hash?.Length != 13 || !_stringValidator.IsMatch(hash))
                throw new ArgumentException(
                    "Value must consist of exactly 13 chars from the set [a-zA-Z0-9./].", nameof(hash));

            if (maxTextLength < 1 || maxTextLength > 8)
                throw new ArgumentOutOfRangeException(nameof(maxTextLength), maxTextLength,
                    "Value must be between 1 and 8.");

            if (chars != null && !_stringValidator.IsMatch(chars))
                throw new ArgumentException(
                    "Value must consist of one or more chars from the set [a-zA-Z0-9./].", nameof(chars));

            chars = new string(chars.Distinct().ToArray());

            return Task.Run(() =>
            {
                var lastCharPosition = chars.Length - 1;
                var salt = hash.AsSpan().Slice(0, 2);
                for (int i = 0; i < maxTextLength; i++)
                {
                    Span<char> buffer = stackalloc char[i + 1];
                    if (TryDecryptRecursive(ref hash, salt, ref chars, lastCharPosition, buffer, 0, i, token))
                        return buffer.ToString();
                }

                throw new DecryptionFailedException($"The {nameof(hash)} '{hash}' " +
                    $"does not correspond to any combination of {nameof(chars)} '{chars}' " +
                    $"up to {maxTextLength} chars long.");
            });
        }

        private static bool TryDecryptRecursive(ref string hash, ReadOnlySpan<char> salt,
            ref string chars, int lastCharPosition,
            Span<char> buffer, int position, int maxPosition, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
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
                    if (hash == DesEncryptor.Encrypt(buffer, salt))
                        return true;
                }
            }

            return false;
        }
    }
}