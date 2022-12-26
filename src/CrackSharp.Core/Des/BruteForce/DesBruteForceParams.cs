using System.Text.RegularExpressions;
using CrackSharp.Core.Common.BruteForce;

namespace CrackSharp.Core.Des.BruteForce;

public record DesBruteForceParams : IBruteForceParams
{
    private const string DefaultChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./";
    private const string AllowedChars = "[a-zA-Z0-9./]";

    private static readonly Regex _charsValidator = new Regex($"^{AllowedChars}+$", RegexOptions.Compiled);

    public DesBruteForceParams(int maxTextLength, string characters = DefaultChars)
    {
        MaxTextLength = maxTextLength is > 0 and < 9
            ? maxTextLength
            : throw new ArgumentOutOfRangeException(nameof(maxTextLength), maxTextLength,
                "Value cannot be less than 1 or greater than 8.");

        Characters = characters is DefaultChars
            ? characters
            : _charsValidator.IsMatch(characters)
                ? new string(characters.Distinct().ToArray())
                : throw new ArgumentException(
                    $"Value must consist of one or more chars from the set {AllowedChars}.", nameof(characters));
    }

    public int MaxTextLength { get; }

    public string Characters { get; }
}
