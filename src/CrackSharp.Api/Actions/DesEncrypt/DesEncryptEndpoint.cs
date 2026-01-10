using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace CrackSharp.Api.Actions.DesEncrypt;

internal static class DesEncryptEndpoint
{
    public static IEndpointRouteBuilder MapDesEncryptEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/encrypt/{*text}", EncryptAsync).WithName("GetDesHash");
        return app;
    }

    private static Ok<string> EncryptAsync(
        [AsParameters] DesEncryptServices services,
        [Required][RegularExpression("^[./0-9A-Za-z]+$")] string text,
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
