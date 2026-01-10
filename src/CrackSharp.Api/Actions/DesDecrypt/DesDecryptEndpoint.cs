using CrackSharp.Api.Constants;
using CrackSharp.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace CrackSharp.Api.Actions.DesDecrypt;

internal static class DesDecryptEndpoint
{
    public static IEndpointRouteBuilder MapDesDecryptEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/decrypt/{*hash}", DecryptAsync).WithName("DecryptDesHash");
        return app;
    }

    private static async Task<Results<Ok<string>, NotFound, StatusCodeHttpResult>> DecryptAsync(
        [AsParameters] DesDecryptServices services,
        [Required][RegularExpression("^[./0-9A-Za-z]{13}$")] string hash,
        [Required][Range(1, 8)] int maxTextLength = 8,
        [Required][RegularExpression("^[./0-9A-Za-z]+$")] string chars = DesConstants.DecryptDefaultChars,
        CancellationToken cancellationToken = default)
    {
        var decryptionService = services.DecryptionService;
        var logger = services.Logger;

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
