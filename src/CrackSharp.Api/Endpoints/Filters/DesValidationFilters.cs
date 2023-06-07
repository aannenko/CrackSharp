using CrackSharp.Core.Des;
using System.Text.RegularExpressions;

namespace CrackSharp.Api.Endpoints.Validation;

internal static class DesValidationFilters
{
    private const string _maxTextLengthValidationMessage = "Value must be greater than 0 and less than 9.";

    private static readonly Regex _hashValidator = DesValidationUtils.GetHashValidator();

    private static readonly Regex _saltValidator = DesValidationUtils.GetSaltValidator();

    private static readonly Regex _charsValidator = DesValidationUtils.GetCharsValidator();

    private static readonly string _hashValidationMessage =
        $"Value cannot be null or empty and must follow pattern {_hashValidator}";

    private static readonly string _saltValidationMessage =
        $"Value must follow pattern {_saltValidator}";

    private static readonly string _charsValidationMessage =
        $"Value must follow pattern {_charsValidator}";

    public static async ValueTask<object?> ValidateDecryptInput(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var (hash, maxTextLength, chars) =
            (context.GetArgument<string?>(2),
            context.GetArgument<int>(3),
            context.GetArgument<string?>(4));

        Dictionary<string, string[]>? errors = null;
        if (hash is null || !_hashValidator.IsMatch(hash))
            (errors ??= new(3)).Add(nameof(hash), new[] { _hashValidationMessage });

        if (!DesValidationUtils.IsMaxTextLengthValid(maxTextLength))
            (errors ??= new(2)).Add(nameof(maxTextLength), new[] { _maxTextLengthValidationMessage });

        if (chars is not null && !_charsValidator.IsMatch(chars))
            (errors ??= new(1)).Add(nameof(chars), new[] { _charsValidationMessage });

        if (errors is not null)
            return TypedResults.ValidationProblem(errors);

        return await next(context);
    }

    public static async ValueTask<object?> ValidateEncryptInput(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var (text, salt) =
            (context.GetArgument<string?>(2),
            context.GetArgument<string?>(3));

        Dictionary<string, string[]>? errors = null;
        if (text is null || !_charsValidator.IsMatch(text))
            (errors ??= new(2)).Add(nameof(text), new[] { _charsValidationMessage });

        if (salt is not null && !_saltValidator.IsMatch(salt))
            (errors ??= new(1)).Add(nameof(salt), new[] { _saltValidationMessage });

        if (errors is not null)
            return TypedResults.ValidationProblem(errors);

        return await next(context);
    }
}
