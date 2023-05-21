using System.Text.RegularExpressions;

namespace CrackSharp.Core.Des;

public static partial class DesUtils
{
    [GeneratedRegex($"^{DesConstants.AllowedCharsPattern}{{13}}$")]
    public static partial Regex GetHashValidator();

    [GeneratedRegex($"^{DesConstants.AllowedCharsPattern}+$")]
    public static partial Regex GetCharsValidator();
}
