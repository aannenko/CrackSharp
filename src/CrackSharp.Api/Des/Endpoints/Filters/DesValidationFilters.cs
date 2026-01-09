using CrackSharp.Core.Des;
using System.Net;
using System.Text.RegularExpressions;

namespace CrackSharp.Api.Des.Endpoints.Filters;

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
        $"Value cannot be null or empty and must follow pattern {_charsValidator}";

    public static async ValueTask<object?> ValidateDecryptInput(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var hash = context.GetArgument<string>(1);
        var maxTextLength = context.GetArgument<int>(2);
        var chars = context.GetArgument<string>(3);

        hash = WebUtility.UrlDecode(hash);
        context.Arguments[1] = hash;

        Dictionary<string, string[]>? errors = null;

        if (hash is null || !_hashValidator.IsMatch(hash))
            (errors = new(3)).Add(nameof(hash), [_hashValidationMessage]);

        if (!DesValidationUtils.IsMaxTextLengthValid(maxTextLength))
            (errors ??= new(2)).Add(nameof(maxTextLength), [_maxTextLengthValidationMessage]);

        if (chars is null || !_charsValidator.IsMatch(chars))
            (errors ??= new(1)).Add(nameof(chars), [_charsValidationMessage]);

        return errors is null ? await next(context).ConfigureAwait(false) : TypedResults.ValidationProblem(errors);
    }

    public static async ValueTask<object?> ValidateEncryptInput(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var text = context.GetArgument<string>(1);
        var salt = context.GetArgument<string>(2);

        text = WebUtility.UrlDecode(text);
        context.Arguments[1] = text;

        Dictionary<string, string[]>? errors = null;

        if (text is null || !_charsValidator.IsMatch(text))
            (errors = new(2)).Add(nameof(text), [_charsValidationMessage]);

        if (salt is not null && !_saltValidator.IsMatch(salt))
            (errors ??= new(1)).Add(nameof(salt), [_saltValidationMessage]);

        return errors is null ? await next(context).ConfigureAwait(false) : TypedResults.ValidationProblem(errors);
    }
}
