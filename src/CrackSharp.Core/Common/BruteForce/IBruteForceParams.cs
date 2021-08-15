namespace CrackSharp.Core.Des.BruteForce
{
    public interface IBruteForceParams
    {
        int MaxTextLength { get; }

        string Characters { get; }
    }
}