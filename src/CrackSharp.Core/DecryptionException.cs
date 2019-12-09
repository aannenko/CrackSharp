using System;

namespace CrackSharp.Core
{
    public class DecryptionFailedException : Exception
    {
        public DecryptionFailedException()
        {
        }

        public DecryptionFailedException(string message) : base(message)
        {
        }
    }
}