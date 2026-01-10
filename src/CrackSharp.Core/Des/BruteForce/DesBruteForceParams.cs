using CrackSharp.Core.Common.BruteForce;

namespace CrackSharp.Core.Des.BruteForce;

public sealed record DesBruteForceParams : IBruteForceParams
{
    public DesBruteForceParams(int maxTextLength, string characters = DesConstants.AllowedChars)
    {
        MaxTextLength = DesValidationUtils.IsMaxTextLengthValid(maxTextLength)
            ? maxTextLength
            : throw new ArgumentOutOfRangeException(nameof(maxTextLength), maxTextLength,
                "Value must be greater than 0 and less than 9.");

        Characters = characters is not null && DesValidationUtils.GetCharsValidator().IsMatch(characters)
            ? new string(characters.Distinct().ToArray())
            : throw new ArgumentException(
                $"Value must consist of one or more chars from the set {DesConstants.AllowedCharsPattern}",
                nameof(characters));
    }

    public int MaxTextLength { get; }

    public string Characters { get; }
}
