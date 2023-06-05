using System.Text.RegularExpressions;

namespace CrackSharp.Core.Des;

public static partial class DesValidationUtils
{
    [GeneratedRegex($"^{DesConstants.AllowedCharsPattern}{{13}}$")]
    public static partial Regex GetHashValidator();

    [GeneratedRegex($"^{DesConstants.AllowedCharsPattern}{{2}}$")]
    public static partial Regex GetSaltValidator();

    [GeneratedRegex($"^{DesConstants.AllowedCharsPattern}+$")]
    public static partial Regex GetCharsValidator();

    public static bool IsMaxTextLengthValid(int maxTextLength) =>
        maxTextLength is > 0 and < 9;
}
