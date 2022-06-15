namespace CrackSharp.Core.Common
{
    public interface ISpanEnumerable<T>
    {
        ISpanEnumerator<T> GetEnumerator();
    }

    public interface ISpanEnumerator<T>
    {
        ReadOnlySpan<T> Current { get; }

        bool MoveNext();
    }
}