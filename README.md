[![Travis](https://travis-ci.org/aannenko/CrackSharp.svg?branch=master)](https://travis-ci.org/aannenko/CrackSharp)

# CrackSharp
Use code in this repository to build and run .NET Core applications that are able to efficiently decrypt DES hashes. Decryption process is based on brute-force method.

High decryption speed is achieved by heavy use of stack-allocated [`Span<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.span-1) and [`ReadOnlySpan<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.readonlyspan-1), heap allocation during decryption is kept to minimum.

### Contents
The repository contains source code for three projects:
- [CrackSharp.Cmd](https://github.com/aannenko/CrackSharp/tree/master/src/CrackSharp.Cmd) is a console app written as a solution to [CS50's Crack problem](https://docs.cs50.net/2019/ap/problems/crack/crack.html)
- [CrackSharp.Api](https://github.com/aannenko/CrackSharp/tree/master/src/CrackSharp.Api) is a WebAPI service, ready to be used with Docker or OpenShift
- [CrackSharp.Core](https://github.com/aannenko/CrackSharp/tree/master/src/CrackSharp.Core) contains encryption/decryption logic and is a common base for the two projects described above.

### Usage
Deployment and usage examples are located in the corresponding project directories.