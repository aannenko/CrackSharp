## CrackSharp
This application is a .NET Core based solution to [CS50's Crack problem](https://docs.cs50.net/2017/ap/problems/crack/crack.html).

### Disclaimer
I'm not responsible for any harm you may do to yourself or the others with this app. You've been warned! ;)

### Details
CrackSharp uses bruteforce to guess words behind DES hashes. It keeps object allocation to minimum and uses [`Span<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.span-1) and [`ReadOnlySpan<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.readonlyspan-1) heavily across its codebase.

The first two fields in the `Program` class have the following meaning:
```csharp
MaxWordLength = 4; // the app will only guess words up to 4 characters long
AllowedChars = "abcXYZ"; // the app uses these characters to guess words
```
Uncomment a commented line in the `TryDecryptRecursive` method to see tested words in real time (it will decrease performance though).

### Usage
Naturally you will need [.NET Core](https://dotnet.microsoft.com/download/dotnet-core) installed.

Example in PowerShell:
```
dotnet run -c Release 50E5eT91y/Q.g
```
or
```
dotnet build -c Release
cd .\bin\Release\netcoreapp3.0
.\crack.exe 50E5eT91y/Q.g
```
Output:
```
tOaD
Decrypting 50E5eT91y/Q.g took 15.390648599999999 seconds.
```

### A challenge for the meticulous who study CS50
Make your application decrypt hashes faster than this one. ;)
