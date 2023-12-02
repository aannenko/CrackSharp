using CrackSharp.Api.Des.Services;

namespace CrackSharp.Api.Des.Model;

public readonly struct DesEncryptServices
{
    public required DesEncryptionService EncryptionService { get; init; }

    public required ILogger<DesEncryptServices> Logger { get; init; }
}
