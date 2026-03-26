using System.Text.RegularExpressions;

namespace CrackSharp.Api.Actions.DesDecrypt;

internal static partial class DesDecryptEndpointFilter
{
    public static bool HasErrors(string? hash, int maxTextLength, string? chars, out Dictionary<string, string[]> errors)
    {
        errors = [];

        if (string.IsNullOrEmpty(hash) || !HashPattern().IsMatch(hash))
            errors[nameof(hash)] = ["The hash field must match the regular expression '^[./0-9A-Za-z]{13}$'."];

        if (maxTextLength is < 1 or > 8)
            errors[nameof(maxTextLength)] = ["The maxTextLength field must be between 1 and 8."];

        if (string.IsNullOrEmpty(chars) || !CharsPattern().IsMatch(chars))
            errors[nameof(chars)] = ["The chars field must match the regular expression '^[./0-9A-Za-z]+$'."];

        return errors.Count > 0;
    }

    [GeneratedRegex("^[./0-9A-Za-z]{13}$")]
    private static partial Regex HashPattern();

    [GeneratedRegex("^[./0-9A-Za-z]+$")]
    private static partial Regex CharsPattern();
}
