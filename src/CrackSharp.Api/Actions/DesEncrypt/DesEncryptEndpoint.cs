using CrackSharp.Api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Functions.Worker;
using System.Diagnostics.CodeAnalysis;

namespace CrackSharp.Api.Actions.DesEncrypt;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI")]
internal sealed class DesEncryptEndpoint(
    DesEncryptionService encryptionService,
    Log<DesEncryptEndpoint> logger)
{
    [Function("GetDesHash")]
    public Results<Ok<string>, ValidationProblem> Encrypt(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/des/encrypt/{*text}")] HttpRequest req,
        string text,
        string? salt = null)
    {
        if (DesEncryptEndpointFilter.HasErrors(text, salt, out var errors))
            return TypedResults.ValidationProblem(errors);

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
