using CrackSharp.Api.Des.Model;
using CrackSharp.Api.Des.Endpoints.Filters;
using CrackSharp.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace CrackSharp.Api.Des.Endpoints;

internal static class DesApi
{
    public static IEndpointRouteBuilder MapDesApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/des");

        group.MapGet("/decrypt/{*hash}", Decrypt)
            .AddEndpointFilter(DesValidationFilters.ValidateDecryptInput)
            .WithName("DecryptDesHash");

        group.MapGet("/encrypt/{*text}", Encrypt)
            .AddEndpointFilter(DesValidationFilters.ValidateEncryptInput)
            .WithName("GetDesHash");

        return app;
    }

    private static async Task<Results<Ok<string>, NotFound, StatusCodeHttpResult>> Decrypt(
        [AsParameters] DesDecryptServices services,
        [RegularExpression("^[./0-9A-Za-z]{13}$")] string hash,
        [Range(1, 8)] int maxTextLength = 8,
        [RegularExpression("^[./0-9A-Za-z]+$")] string chars = DesConstants.DecryptDefaultChars,
        CancellationToken cancellationToken = default)
    {
        var decryptionService = services.DecryptionService;
        var logger = services.Logger;

        try
        {
            logger.DecryptionRequested(hash, maxTextLength, chars);
            var decrypted = await decryptionService.DecryptAsync(new(hash, maxTextLength, chars) , cancellationToken).ConfigureAwait(false);
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

    private static Ok<string> Encrypt(
        [AsParameters] DesEncryptServices services,
        [RegularExpression("^[./0-9A-Za-z]+$")] string text,
        [RegularExpression("^[./0-9A-Za-z]{2}$")] string? salt = null)
    {
        var encryptionService = services.EncryptionService;
        var logger = services.Logger;

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
