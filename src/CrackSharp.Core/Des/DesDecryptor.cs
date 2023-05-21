using CrackSharp.Core.Common;

namespace CrackSharp.Core.Des;

public static class DesDecryptor
{
    public static Task<string> DecryptAsync<T>(string hash, T enumerable, CancellationToken token = default)
        where T : ISpanEnumerable<char>, IDescribable
    {
        if (!DesUtils.GetHashValidator().IsMatch(hash))
            throw new ArgumentException(
                $"Value must consist of exactly 13 chars from the set {DesConstants.AllowedCharsPattern}.",
                nameof(hash));

        string Decrypt()
        {
            var salt = hash.AsSpan(0, 2);
            Span<char> hashBuffer = stackalloc char[13];
            foreach (var text in enumerable)
            {
                token.ThrowIfCancellationRequested();
                DesEncryptor.Encrypt(text, salt, hashBuffer);
                if (hashBuffer.SequenceEqual(hash))
                    return text.ToString();
            }

            throw new DecryptionFailedException($"Decryption of the {nameof(hash)} '{hash}' " +
                $"using the following {nameof(enumerable)} has failed. {enumerable.Description}");
        }

        return Task.Run(Decrypt, token);
    }
}
