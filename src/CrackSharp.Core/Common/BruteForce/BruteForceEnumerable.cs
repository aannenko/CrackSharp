namespace CrackSharp.Core.Common.BruteForce;

public sealed class BruteForceEnumerable(IBruteForceParams parameters) : ISpanEnumerable<char>, IDescribable
{
    private readonly IBruteForceParams _params = parameters ?? throw new ArgumentNullException(nameof(parameters));

    public string Description =>
        $"Brute-force enumerable combining characters '{_params.Characters}' " +
        $"into strings of up to {_params.MaxTextLength} characters long.";

    public ISpanEnumerator<char> GetEnumerator() => new Enumerator(_params.Characters, _params.MaxTextLength);

    private class Enumerator(string characters, int maxTextLength) : ISpanEnumerator<char>
    {
        private readonly char[] _textBuffer = new char[maxTextLength];
        private readonly int[] _indices = new int[maxTextLength];
        private int _position = 0;

        public ReadOnlySpan<char> Current => _textBuffer.AsSpan(0, _position + 1);

        public bool MoveNext() => MoveNext(_position);

        private bool MoveNext(int position)
        {
            if (_indices[position] < characters.Length)
            {
                _textBuffer[position] = characters[_indices[position]];
                _indices[position]++;
                return true;
            }

            _textBuffer[position] = characters[0];
            _indices[position] = 1;

            return position > 0
                ? MoveNext(position - 1)
                : ++_position < maxTextLength && MoveNext(_position);
        }
    }
}
