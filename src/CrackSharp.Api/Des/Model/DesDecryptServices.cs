using CrackSharp.Api.Des.Services;

namespace CrackSharp.Api.Des.Model;

public readonly struct DesDecryptServices
{
    public required DesBruteForceDecryptionService DecryptionService { get; init; }

    public required ILogger<DesDecryptServices> Logger { get; init; }
}
