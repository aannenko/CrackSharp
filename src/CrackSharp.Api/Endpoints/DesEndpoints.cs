using CrackSharp.Api.Endpoints.Validation;
using CrackSharp.Api.Services.Des;
using CrackSharp.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace CrackSharp.Api.Endpoints;

public sealed class DesEndpoints
{
    private const string DefaultChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/des").WithOpenApi();
        
        group.MapGet("/decrypt", Decrypt)
            .AddEndpointFilter(DesValidationFilters.ValidateDecryptInput)
            .WithName("DecryptDesHash");
        
        group.MapGet("/encrypt", Encrypt)
            .AddEndpointFilter(DesValidationFilters.ValidateEncryptInput)
            .WithName("GetDesHash");
    }

    private static async Task<Results<Ok<string>, NotFound, StatusCodeHttpResult>> Decrypt(
        ILogger<DesEndpoints> logger,
        DesBruteForceDecryptionService decryptor,
        [Required, RegularExpression("^[./0-9A-Za-z]{13}$")] string hash,
        [Range(1, 8)] int maxTextLength = 8,
        [RegularExpression("^[./0-9A-Za-z]+$")] string? chars = DefaultChars,
        CancellationToken cancellationToken = default)
    {
        const string LogDecryption = $"Decryption of the {nameof(hash)} '{{Hash}}' " +
            $"with {nameof(maxTextLength)} = {{MaxTextLength}} and {nameof(chars)} = '{{Chars}}'";

        try
        {
            return TypedResults.Ok(
                await decryptor.DecryptAsync(hash, maxTextLength, chars ?? DefaultChars, cancellationToken));
        }
        catch (DecryptionFailedException e)
        {
            logger.LogInformation(e, $"{LogDecryption} failed. Value not found.", hash, maxTextLength, chars);
            return TypedResults.NotFound();
        }
        catch (OperationCanceledException e)
        {
            logger.LogInformation(e, $"{LogDecryption} canceled.", hash, maxTextLength, chars);
            return TypedResults.StatusCode(StatusCodes.Status408RequestTimeout);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"{LogDecryption} failed.", hash, maxTextLength, chars);
            throw;
        }
    }

    private static Ok<string> Encrypt(
        ILogger<DesEndpoints> logger,
        DesEncryptionService encryptor,
        [Required, RegularExpression("^[./0-9A-Za-z]+$")] string text,
        [RegularExpression("^[./0-9A-Za-z]{2}$")] string? salt = null)
    {
        try
        {
            return TypedResults.Ok(encryptor.Encrypt(text, salt));
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Encryption of the {nameof(text)} '{{Text}}' with {nameof(salt)} '{{Salt}}' failed.",
                text, salt);

            throw;
        }
    }
}
