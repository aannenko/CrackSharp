using CrackSharp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrackSharp.Api.Actions.DesEncrypt;

internal readonly struct DesEncryptServices
{
    [FromServices]
    public required DesEncryptionService EncryptionService { get; init; }

    [FromServices]
    public required Log<DesEncryptServices> Logger { get; init; }
}
