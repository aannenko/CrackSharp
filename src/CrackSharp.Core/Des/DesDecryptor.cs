using System.Text.RegularExpressions;
using CrackSharp.Core.Common;

namespace CrackSharp.Core.Des;

public static class DesDecryptor
{
    private static readonly Regex _hashValidator = new Regex("^[a-zA-Z0-9./]{13}$", RegexOptions.Compiled);

    public static Task<string> DecryptAsync<T>(string hash, T enumerable, CancellationToken token = default)
        where T : ISpanEnumerable<char>, IDescribable
    {
        if (!_hashValidator.IsMatch(hash))
            throw new ArgumentException(
                "Value must consist of exactly 13 chars from the set [a-zA-Z0-9./].", nameof(hash));

        return Task.Run(() =>
        {
            var salt = hash.AsSpan(0, 2);
            Span<char> hashBuffer = stackalloc char[13];
            foreach (var text in enumerable)
            {
                token.ThrowIfCancellationRequested();
                DesEncryptor.Encrypt(text, salt, hashBuffer);
                if (MemoryExtensions.Equals(hash, hashBuffer, StringComparison.Ordinal))
                    return text.ToString();
            }

            throw new DecryptionFailedException($"Decryption of the {nameof(hash)} '{hash}' " +
                $"using the following {nameof(enumerable)} has failed. {enumerable.Description}");
        }, token);
    }
}
