using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CrackSharp.Core.Des.BruteForce
{
    public record DesBruteForceParams : IBruteForceParams
    {
        private const string DefaultChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./";

        private static readonly Regex _charsValidator = new Regex("^[a-zA-Z0-9./]+$", RegexOptions.Compiled);

        public DesBruteForceParams(int maxTextLength, string characters = DefaultChars)
        {
            MaxTextLength = maxTextLength > 0 && maxTextLength < 9
                ? maxTextLength
                : throw new ArgumentOutOfRangeException(nameof(maxTextLength), maxTextLength,
                    "Value cannot be less than 1 or greater than 8.");

            Characters = _charsValidator.IsMatch(characters)
                ? new string(characters.Distinct().ToArray())
                : throw new ArgumentException(
                    "Value must consist of one or more chars from the set [a-zA-Z0-9./].", nameof(characters));
        }

        public int MaxTextLength { get; }

        public string Characters { get; }
    }
}