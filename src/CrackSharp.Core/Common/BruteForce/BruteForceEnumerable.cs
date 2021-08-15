using System;
using CrackSharp.Core.Des.BruteForce;

namespace CrackSharp.Core.Common.BruteForce
{
    public class BruteForceEnumerable : ISpanEnumerable<char>, IDescribable
    {
        private readonly IBruteForceParams _params;

        public BruteForceEnumerable(IBruteForceParams parameters) =>
            _params = parameters ?? throw new ArgumentNullException(nameof(parameters));

        public string Description =>
            $"Brute-force enumerable combining characters '{_params.Characters}' " +
            $"into strings of up to {_params.MaxTextLength} characters long.";

        public ISpanEnumerator<char> GetEnumerator() => new Enumerator(_params);

        public struct Enumerator : ISpanEnumerator<char>
        {
            private readonly IBruteForceParams _params;
            private readonly char[] _textBuffer;
            private readonly int[] _indices;
            private int _position;

            internal Enumerator(IBruteForceParams parameters)
            {
                _params = parameters;
                _textBuffer = new char[_params.MaxTextLength];
                _indices = new int[_params.MaxTextLength];
                _position = 0;
            }

            public ReadOnlySpan<char> Current => _textBuffer.AsSpan(0, _position + 1);

            public bool MoveNext() => MoveNext(_position);

            private bool MoveNext(int position)
            {
                if (_indices[position] < _params.Characters.Length)
                {
                    _textBuffer[position] = _params.Characters[_indices[position]];
                    _indices[position]++;
                    return true;
                }

                _textBuffer[position] = _params.Characters[0];
                _indices[position] = 1;

                return position > 0
                    ? MoveNext(position - 1)
                    : ++_position < _params.MaxTextLength
                        ? MoveNext(_position)
                        : false;
            }
        }
    }
}