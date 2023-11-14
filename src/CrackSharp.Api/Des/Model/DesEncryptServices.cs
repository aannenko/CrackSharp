using CrackSharp.Api.Des.Services;

namespace CrackSharp.Api.Des.Model;

public readonly record struct DesEncryptServices(
    DesEncryptionService EncryptionService,
    ILogger<DesEncryptServices> Logger);
