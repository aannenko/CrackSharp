using CrackSharp.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Functions.Worker;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace CrackSharp.Api.Actions.DesEncrypt;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI")]
internal sealed class DesEncryptEndpoint(
    DesEncryptionService encryptionService,
    Log<DesEncryptEndpoint> logger)
{
    [Function("GetDesHash")]
    public Ok<string> Encrypt(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/des/encrypt/{*text}")] HttpRequest req,
        [Required][RegularExpression("^[./0-9A-Za-z]+$")] string text,
        [RegularExpression("^[./0-9A-Za-z]{2}$")] string? salt = null)
    {
        try
        {
            logger.EncryptionRequested(text, salt);
            var encrypted = encryptionService.Encrypt(text, salt);
            logger.EncryptionSucceeded(text, salt);

            return TypedResults.Ok(encrypted);
        }
        catch (Exception e)
        {
            logger.EncryptionError(e, text, salt);
            throw;
        }
    }
}
