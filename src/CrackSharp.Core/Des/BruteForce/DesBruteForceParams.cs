using CrackSharp.Core.Common.BruteForce;

namespace CrackSharp.Core.Des.BruteForce;

public sealed record DesBruteForceParams : IBruteForceParams
{
    public DesBruteForceParams(int maxTextLength, string characters = DesConstants.AllowedChars)
    {
        MaxTextLength = maxTextLength is > 0 and < 9
            ? maxTextLength
            : throw new ArgumentOutOfRangeException(nameof(maxTextLength), maxTextLength,
                "Value cannot be less than 1 or greater than 8.");

        Characters = characters is not null && DesValidationUtils.GetCharsValidator().IsMatch(characters)
            ? new string(characters.Distinct().ToArray())
            : throw new ArgumentException(
                $"Value must consist of one or more chars from the set {DesConstants.AllowedCharsPattern}",
                nameof(characters));
    }

    public int MaxTextLength { get; }

    public string Characters { get; }
}
