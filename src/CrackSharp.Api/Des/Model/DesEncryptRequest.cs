namespace CrackSharp.Api.Des.Model;

internal readonly record struct DesEncryptRequest(
    string Text,
    string? Salt = null);
