## CrackSharp.Cmd
Use code in this directory to build a console application, capable of bruteforcing DES hashes produced by [crypt(3)](https://www.man7.org/linux/man-pages/man3/crypt.3.html). This console application can be considered a .NET-based solution to [CS50's Crack problem](https://docs.cs50.net/2019/ap/problems/crack/crack.html).

### Details
The first two constants in `Program.cs` have the following meaning:
```csharp
MaxTextLength = 5; // the app will only guess words up to 5 characters long
Chars = "abcXYZ"; // the app will only build words from these characters
```
### Usage
You will need [.NET 5 SDK](https://dotnet.microsoft.com/download/dotnet/5.0) installed.

Example in PowerShell/bash:
```powershell
dotnet run -c Release -- 50cI2vYkF0YU2
```
or
```powershell
dotnet build -c Release -o out
cd ./out
./crack 50cI2vYkF0YU2
```
Output: `LOL`
