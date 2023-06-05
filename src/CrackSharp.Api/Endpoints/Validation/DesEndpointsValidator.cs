using CrackSharp.Core.Des;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace CrackSharp.Api.Endpoints.Validation;

internal static class DesEndpointsValidator
{
    private const string _maxTextLengthValidationMessage = "Value must be greater than 0 and less than 9.";

    private static readonly Regex _hashValidator = DesValidationUtils.GetHashValidator();

    private static readonly Regex _saltValidator = DesValidationUtils.GetSaltValidator();

    private static readonly Regex _charsValidator = DesValidationUtils.GetCharsValidator();

    private static readonly string _hashValidationMessage =
        $"Value must follow pattern {_hashValidator}";

    private static readonly string _saltValidationMessage =
        $"Value must follow pattern {_saltValidator}";

    private static readonly string _charsValidationMessage =
        $"Value must follow pattern {_charsValidator}";

    public static bool TryGetErrors(
        string hash,
        int maxTextLength,
        string? chars,
        [NotNullWhen(true)] out Dictionary<string, string[]>? errors)
    {
        errors = null;
        if (hash is null || !_hashValidator.IsMatch(hash))
            (errors ??= new(3)).Add(nameof(hash), new[] { _hashValidationMessage });

        if (!DesValidationUtils.IsMaxTextLengthValid(maxTextLength))
            (errors ??= new(2)).Add(nameof(maxTextLength), new[] { _maxTextLengthValidationMessage });

        if (chars is not null && !_charsValidator.IsMatch(chars))
            (errors ??= new(1)).Add(nameof(chars), new[] { _charsValidationMessage });

        return errors is not null;
    }

    public static bool TryGetErrors(
        string text,
        string? salt,
        [NotNullWhen(true)] out Dictionary<string, string[]>? errors)
    {
        errors = null;
        if (text is null || !_charsValidator.IsMatch(text))
            (errors ??= new(2)).Add(nameof(text), new[] { _charsValidationMessage });

        if (salt is null || !_saltValidator.IsMatch(salt))
            (errors ??= new(1)).Add(nameof(salt), new[] { _saltValidationMessage });

        return errors is not null;
    }
}
