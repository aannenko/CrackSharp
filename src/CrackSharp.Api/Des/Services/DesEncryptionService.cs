using CrackSharp.Api.Common.Services;
using CrackSharp.Core.Des;

namespace CrackSharp.Api.Des.Services;

public sealed class DesEncryptionService(AwaitableMemoryCache<string, string> cache) : IDisposable
{
    public string Encrypt(string text, string? salt = null)
    {
        var trimmedText = text.Length <= 8 ? text : text[..8];
        Span<char> hashBuffer = stackalloc char[13];
        if (string.IsNullOrWhiteSpace(salt))
            DesEncryptor.Encrypt(trimmedText, hashBuffer);
        else
            DesEncryptor.Encrypt(trimmedText, salt, hashBuffer);

        var hash = hashBuffer.ToString();
        cache.GetOrCreate(hash, trimmedText);

        return hash;
    }

    public void Dispose() =>
        cache.Dispose();
}
