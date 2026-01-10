using CrackSharp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrackSharp.Api.Actions.DesDecrypt;

internal readonly struct DesDecryptServices
{
    [FromServices]
    public required DesBruteForceDecryptionService DecryptionService { get; init; }

    [FromServices]
    public required Log<DesDecryptServices> Logger { get; init; }
}
