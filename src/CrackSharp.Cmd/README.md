## CrackSharp.Cmd
Use code in this directory to build a console application, capable of bruteforcing DES hashes. This console application can be considered as a .NET Core based solution to [CS50's Crack problem](https://docs.cs50.net/2019/ap/problems/crack/crack.html).

### Details
The first two fields in the `Program` class have the following meaning:
```csharp
MaxWordLength = 5; // the app will only guess words up to 5 characters long
Chars = "abcXYZ"; // the app will only build words from these characters
```
### Usage
Naturally, you will need [.NET Core](https://dotnet.microsoft.com/download/dotnet-core/3.1) installed.

Example in PowerShell:
```
dotnet run -c Release 50E5eT91y/Q.g
```
or
```
dotnet build -c Release
cd .\bin\Release\netcoreapp3.1
.\crack.exe 50E5eT91y/Q.g
```
Output: `tOaD`

### Remarks
The longer the word behind a hash, the longer it will take to decrypt this hash. Decryption of `50E5eT91y/Q.g` (corresponds to `tOaD`) takes around 15 seconds on a test machine, the hash `50.jPgLzVirkc` (`hi`) gets decrypted almost instantly.
