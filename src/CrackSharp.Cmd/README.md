## CrackSharp.Cmd
Use code in this directory to build a console application, capable of bruteforcing DES hashes. This console application can be considered as a .NET Core based solution to [CS50's Crack problem](https://docs.cs50.net/2019/ap/problems/crack/crack.html).

### Details
The first two fields in the `Program` class have the following meaning:
```csharp
MaxWordLength = 5; // the app will only guess words up to 5 characters long
Chars = "abcXYZ"; // the app will only build words from these characters
```
### Usage
Naturally, you will need [.NET Core SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1) installed.

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