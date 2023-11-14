using CrackSharp.Api.Des.Services;

namespace CrackSharp.Api.Des.Model;

public readonly record struct DesDecryptServices(
    DesBruteForceDecryptionService DecryptionService,
    ILogger<DesDecryptServices> Logger);
