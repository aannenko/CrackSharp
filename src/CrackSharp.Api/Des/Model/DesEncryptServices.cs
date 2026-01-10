using CrackSharp.Api.Common.Logging;
using CrackSharp.Api.Des.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrackSharp.Api.Des.Model;

internal readonly struct DesEncryptServices
{
    [FromServices]
    public required DesEncryptionService EncryptionService { get; init; }

    [FromServices]
    public required Log<DesEncryptServices> Logger { get; init; }
}
