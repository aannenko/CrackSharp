using CrackSharp.Api.Constants;

namespace CrackSharp.Api.Actions.DesDecrypt;

internal readonly record struct DesDecryptRequest(
    string Hash,
    int MaxTextLength = 8,
    string Chars = DesConstants.DecryptDefaultChars);
