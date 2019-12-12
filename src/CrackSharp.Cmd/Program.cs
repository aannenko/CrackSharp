using System;
using System.Threading.Tasks;
using CrackSharp.Core;

namespace CrackSharp.Cmd
{
    class Program
    {
        private const int MaxWordLength = 5;
        private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        static async Task<int> Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: ./crack hash");
                return 1;
            }

            try
            {
                Console.WriteLine($"{await new DesDecryptionService().DecryptAsync(args[0], MaxWordLength, Chars)}\n");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return 1;
        }
    }
}
