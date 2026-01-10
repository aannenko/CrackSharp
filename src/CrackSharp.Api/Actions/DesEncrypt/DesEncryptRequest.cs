namespace CrackSharp.Api.Actions.DesEncrypt;

internal readonly record struct DesEncryptRequest(
    string Text,
    string? Salt = null);
