using CrackSharp.Api.Endpoints.Filters;
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
        const string partialMessage = $"Decryption of the {nameof(hash)} '{{{nameof(hash)}}}' " +
            $"with {nameof(maxTextLength)} = {{{nameof(maxTextLength)}}} " +
            $"and {nameof(chars)} = '{{{nameof(chars)}}}'";

        try
        {
            return TypedResults.Ok(
                await decryptor.DecryptAsync(hash, maxTextLength, chars ?? DefaultChars, cancellationToken));
        }
        catch (DecryptionFailedException e)
        {
            logger.LogInformation(e, $"{partialMessage} failed. Value not found.", hash, maxTextLength, chars);
            return TypedResults.NotFound();
        }
        catch (OperationCanceledException e)
        {
            logger.LogInformation(e, $"{partialMessage} canceled.", hash, maxTextLength, chars);
            return TypedResults.StatusCode(StatusCodes.Status408RequestTimeout);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"{partialMessage} failed.", hash, maxTextLength, chars);
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
            const string errorMessage = $"Encryption of the {nameof(text)} '{{{nameof(text)}}}' " +
                $"with {nameof(salt)} '{{{nameof(salt)}}}' failed.";

            logger.LogError(e, errorMessage, text, salt);
            throw;
        }
    }
}
