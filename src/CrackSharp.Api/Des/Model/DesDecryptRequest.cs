namespace CrackSharp.Api.Des.Model;

internal readonly record struct DesDecryptRequest(
    string Hash,
    int MaxTextLength = 8,
    string Chars = DesConstants.DecryptDefaultChars);
