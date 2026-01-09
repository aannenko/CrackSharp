using CrackSharp.Api.Des.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrackSharp.Api.Des.Model;

internal readonly struct DesDecryptServices
{
    [FromServices]
    public required DesBruteForceDecryptionService DecryptionService { get; init; }

    [FromServices]
    public required ILogger<DesDecryptServices> Logger { get; init; }
}
