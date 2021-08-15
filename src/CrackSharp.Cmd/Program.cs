using System;
using CrackSharp.Core.Common.BruteForce;
using CrackSharp.Core.Des;
using CrackSharp.Core.Des.BruteForce;

const int MaxTextLength = 5;
const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

if (args.Length != 1)
{
    Console.WriteLine($"Usage: ./crack hash");
    return 1;
}

try
{
    Console.WriteLine(await DesDecryptor.DecryptAsync(args[0],
        new BruteForceEnumerable(new DesBruteForceParams(MaxTextLength, Chars))));

    return 0;
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

return 1;
