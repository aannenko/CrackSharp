using System;
using System.Threading.Tasks;
using CrackSharp.Core.Des;

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
                Console.WriteLine("Usage: ./crack hash\n");
                return 1;
            }

            try
            {
                Console.WriteLine($"{await DesDecryptor.DecryptAsync(args[0], MaxWordLength, Chars)}\n");
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
