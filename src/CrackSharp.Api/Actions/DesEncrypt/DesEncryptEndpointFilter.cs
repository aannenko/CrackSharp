using System.Text.RegularExpressions;

namespace CrackSharp.Api.Actions.DesEncrypt;

internal static partial class DesEncryptEndpointFilter
{
    public static bool HasErrors(string? text, string? salt, out Dictionary<string, string[]> errors)
    {
        errors = [];

        if (string.IsNullOrEmpty(text) || !TextPattern().IsMatch(text))
            errors[nameof(text)] = ["The text field must match the regular expression '^[./0-9A-Za-z]+$'."];

        if (salt is not null && !SaltPattern().IsMatch(salt))
            errors[nameof(salt)] = ["The salt field must match the regular expression '^[./0-9A-Za-z]{2}$'."];

        return errors.Count > 0;
    }

    [GeneratedRegex("^[./0-9A-Za-z]+$")]
    private static partial Regex TextPattern();

    [GeneratedRegex("^[./0-9A-Za-z]{2}$")]
    private static partial Regex SaltPattern();
}
