using CrackSharp.Api.Services.Des;
using CrackSharp.Core;
using CrackSharp.Core.Des;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace CrackSharp.Api.Endpoints;

public sealed class DesEndpoints
{
    private const string DefaultChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string MaxTextLengthValidationMessage = "Value must be greater than 0 and less than 9.";
    
    private static readonly string HashValidationMessage =
        $"Value must follow pattern {DesValidationUtils.GetHashValidator()}";

    private static readonly string SaltValidationMessage =
        $"Value must follow pattern {DesValidationUtils.GetSaltValidator()}";

    private static readonly string CharsValidationMessage =
        $"Value must follow pattern {DesValidationUtils.GetCharsValidator()}";

    public static void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/v1/des").WithOpenApi();
        group.MapGet("/decrypt", Decrypt).WithName("DecryptDesHash");
        group.MapGet("/encrypt", Encrypt).WithName("GetDesHash");
    }

    private static async Task<Results<Ok<string>, NotFound, StatusCodeHttpResult, ValidationProblem>> Decrypt(
        ILogger<DesEndpoints> logger,
        DesBruteForceDecryptionService decryptor,
        [Required, RegularExpression("^[./0-9A-Za-z]{13}$")] string hash,
        [Range(1, 8)] int maxTextLength = 8,
        [RegularExpression("^[./0-9A-Za-z]+$")] string? chars = DefaultChars,
        CancellationToken cancellationToken = default)
    {
        if (TryGetErrors(hash, maxTextLength, chars, out var errorsDict))
            return TypedResults.ValidationProblem(errorsDict);

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

    private static Results<Ok<string>, ValidationProblem> Encrypt(
        ILogger<DesEndpoints> logger,
        DesEncryptionService encryptor,
        [Required, RegularExpression("^[./0-9A-Za-z]+$")] string text,
        [RegularExpression("^[./0-9A-Za-z]{2}$")] string? salt = null)
    {
        if (TryGetErrors(text, salt, out var errorsDict))
            return TypedResults.ValidationProblem(errorsDict);

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

    private static bool TryGetErrors(
        string hash,
        int maxTextLength,
        string? chars,
        [NotNullWhen(true)] out Dictionary<string, string[]>? errors)
    {
        errors = null;
        if (hash is null || !DesValidationUtils.GetHashValidator().IsMatch(hash))
            (errors ??= new(3)).Add(nameof(hash), new[] { HashValidationMessage });

        if (!DesValidationUtils.IsMaxTextLengthValid(maxTextLength))
            (errors ??= new(2)).Add(nameof(maxTextLength), new[] { MaxTextLengthValidationMessage });

        if (chars is not null && !DesValidationUtils.GetCharsValidator().IsMatch(chars))
            (errors ??= new(1)).Add(nameof(chars), new[] { CharsValidationMessage });

        return errors is not null;
    }

    private static bool TryGetErrors(
        string text,
        string? salt,
        [NotNullWhen(true)] out Dictionary<string, string[]>? errors)
    {
        errors = null;
        if (text is null || !DesValidationUtils.GetCharsValidator().IsMatch(text))
            (errors ??= new(2)).Add(nameof(text), new[] { CharsValidationMessage });

        if (salt is null || !DesValidationUtils.GetSaltValidator().IsMatch(salt))
            (errors ??= new(1)).Add(nameof(salt), new[] { SaltValidationMessage });

        return errors is not null;
    }
}
