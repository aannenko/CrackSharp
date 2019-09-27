using System;
using System.Diagnostics;

namespace CrackSharp
{
    class Program
    {
        private const int MaxWordLength = 4;
        private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly int LastCharPosition = AllowedChars.Length - 1;

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: ./crack hash");
                return;
            }

            var sWatch = new Stopwatch();
            sWatch.Start();

            Console.WriteLine(TryDecrypt(args[0], out string decrypted)
                ? decrypted
                : "Phrase not found.");

            sWatch.Stop();
            Console.WriteLine($"Decrypting {args[0]} took {sWatch.Elapsed.TotalSeconds} seconds.");
        }

        private static bool TryDecrypt(ReadOnlySpan<char> hash, out string decrypted)
        {
            var salt = hash.Slice(0, 2);

            // the algorithm processes shorter words first (a..Z, aa..ZZ)
            for (int i = 1; i <= MaxWordLength; i++)
            {
                Span<char> word = stackalloc char[i];
                if (TryDecryptRecursive(salt, word, 0, i - 1, hash))
                {
                    decrypted = word.ToString();
                    return true;
                }
            }

            decrypted = string.Empty;
            return false;
        }

        private static bool TryDecryptRecursive(ReadOnlySpan<char> salt, Span<char> word,
            int position, int maxPosition, ReadOnlySpan<char> hash)
        {
            for (int i = 0; i <= LastCharPosition; i++)
            {
                word[position] = AllowedChars[i];
                if (position < maxPosition)
                {
                    if (TryDecryptRecursive(salt, word, position + 1, maxPosition, hash))
                        return true;
                }
                else
                {
                    //Console.WriteLine(word.ToString()); // uncomment to see tested words
                    if (hash.SequenceEqual(CryptSharp.Crypt(salt, word)))
                        return true;
                }
            }

            return false;
        }
    }
}
