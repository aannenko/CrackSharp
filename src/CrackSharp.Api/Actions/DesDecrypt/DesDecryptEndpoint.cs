using CrackSharp.Api.Constants;
using CrackSharp.Api.Services;
using CrackSharp.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Functions.Worker;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace CrackSharp.Api.Actions.DesDecrypt;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated by DI")]
internal sealed class DesDecryptEndpoint(
    DesBruteForceDecryptionService decryptionService,
    Log<DesDecryptEndpoint> logger)
{
    [Function("DecryptDesHash")]
    public async Task<Results<Ok<string>, NotFound, StatusCodeHttpResult>> DecryptAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/des/decrypt/{*hash}")] HttpRequest req,
        [Required][RegularExpression("^[./0-9A-Za-z]{13}$")] string hash,
        [Required][Range(1, 8)] int maxTextLength = 8,
        [Required][RegularExpression("^[./0-9A-Za-z]+$")] string chars = DesConstants.DecryptDefaultChars,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.DecryptionRequested(hash, maxTextLength, chars);
            var decrypted = await decryptionService
                .DecryptAsync(new(hash, maxTextLength, chars), cancellationToken)
                .ConfigureAwait(false);

            logger.DecryptionSucceeded(hash, maxTextLength, chars);

            return TypedResults.Ok(decrypted);
        }
        catch (DecryptionFailedException e)
        {
            logger.DecryptionFailed(e, hash, maxTextLength, chars);
            return TypedResults.NotFound();
        }
        catch (OperationCanceledException e)
        {
            logger.DecryptionCanceled(e, hash, maxTextLength, chars);
            return TypedResults.StatusCode(StatusCodes.Status408RequestTimeout);
        }
        catch (Exception e)
        {
            logger.DecryptionError(e, hash, maxTextLength, chars);
            throw;
        }
    }
}
