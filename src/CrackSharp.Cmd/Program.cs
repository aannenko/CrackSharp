using System;
using CrackSharp.Core.Des;

const int MaxWordLength = 5;
const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

if (args.Length != 1)
{
    Console.WriteLine($"Usage: ./crack hash");
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
