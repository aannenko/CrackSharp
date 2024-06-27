namespace CrackSharp.Api.Des.Model;

public readonly record struct DesEncryptRequest(
    string Text,
    string? Salt = null);
