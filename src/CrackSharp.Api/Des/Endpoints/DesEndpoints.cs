using CrackSharp.Api.Des.DTO;
using CrackSharp.Api.Des.Endpoints.Filters;
using CrackSharp.Api.Des.Services;
using CrackSharp.Core;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CrackSharp.Api.Des.Endpoints;

public sealed class DesEndpoints
{
    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/des").WithOpenApi();

        group.MapGet("/decrypt/{*hash}", Decrypt)
            .AddEndpointFilter(DesValidationFilters.ValidateDecryptInput)
            .WithName("DecryptDesHash");

        group.MapGet("/encrypt/{*text}", Encrypt)
            .AddEndpointFilter(DesValidationFilters.ValidateEncryptInput)
            .WithName("GetDesHash");
    }

    private static async Task<Results<Ok<string>, NotFound, StatusCodeHttpResult>> Decrypt(
        ILogger<DesEndpoints> logger,
        DesBruteForceDecryptionService decryptionService,
        [AsParameters] DesDecryptRequest request,
        CancellationToken cancellationToken = default)
    {
        var (hash, maxTextLength, chars) = request;
        const string partialMessage = $"Decryption of the {nameof(hash)} '{{{nameof(hash)}}}' " +
            $"with {nameof(maxTextLength)} {{{nameof(maxTextLength)}}} and {nameof(chars)} '{{{nameof(chars)}}}'";

        try
        {
            logger.LogInformation($"{partialMessage} requested.", hash, maxTextLength, chars);
            var decrypted = await decryptionService.DecryptAsync(request, cancellationToken);
            logger.LogInformation($"{partialMessage} succeeded.", hash, maxTextLength, chars);
            
            return TypedResults.Ok(decrypted);
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
        DesEncryptionService encryptionService,
        [AsParameters] DesEncryptRequest request)
    {
        var (text, salt) = request;
        const string partialMessage = $"Encryption of the {nameof(text)} '{{{nameof(text)}}}' " +
            $"with {nameof(salt)} '{{{nameof(salt)}}}'";

        try
        {
            logger.LogInformation($"{partialMessage} requested.", text, salt);
            var encrypted = encryptionService.Encrypt(text, salt);
            logger.LogInformation($"{partialMessage} succeeded.", text, salt);

            return TypedResults.Ok(encrypted);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"{partialMessage} failed.", text, salt);
            throw;
        }
    }
}
