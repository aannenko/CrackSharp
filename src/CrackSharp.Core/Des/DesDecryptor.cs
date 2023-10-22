using CrackSharp.Core.Common;

namespace CrackSharp.Core.Des;

public static class DesDecryptor
{
    public static Task<string> DecryptAsync<TEnumerable>(
        string hash,
        TEnumerable enumerable,
        CancellationToken cancellationToken = default)
        where TEnumerable : ISpanEnumerable<char>, IDescribable
    {
        if (!DesValidationUtils.GetHashValidator().IsMatch(hash))
            throw new ArgumentException(
                $"Value must consist of exactly 13 chars from the set {DesConstants.AllowedCharsPattern}.",
                nameof(hash));

        return Task.Run(
            () =>
            {
                var salt = hash.AsSpan(0, 2);
                Span<char> hashBuffer = stackalloc char[13];
                foreach (var text in enumerable)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    DesEncryptor.Encrypt(text, salt, hashBuffer);
                    if (hashBuffer.SequenceEqual(hash))
                        return text.ToString();
                }

                throw new DecryptionFailedException($"Decryption of the {nameof(hash)} '{hash}' " +
                    $"using the following {nameof(enumerable)} has failed. {enumerable.Description}");
            },
            cancellationToken);
    }
}
