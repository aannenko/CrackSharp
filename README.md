[![Build](https://github.com/aannenko/CrackSharp/actions/workflows/dotnetcore.yml/badge.svg)](https://github.com/aannenko/CrackSharp/actions/workflows/dotnetcore.yml) [![Docker](https://github.com/aannenko/CrackSharp/actions/workflows/docker-publish.yml/badge.svg)](https://github.com/aannenko/CrackSharp/actions/workflows/docker-publish.yml)

# CrackSharp
Use code in this repository to build and run .NET Core applications that are able to efficiently decrypt DES hashes produced by [crypt(3)](https://www.man7.org/linux/man-pages/man3/crypt.3.html). Decryption process is based on brute-force method.

High decryption speed is achieved by heavy usage of stack-allocated [`Span<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.span-1) and [`ReadOnlySpan<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.readonlyspan-1), heap allocation during decryption is kept to a minimum.

### Contents
The repository contains source code for three projects:
- [CrackSharp.Cmd](https://github.com/aannenko/CrackSharp/tree/master/src/CrackSharp.Cmd) is a console app written as a solution to [CS50's Crack problem](https://docs.cs50.net/2019/ap/problems/crack/crack.html)
- [CrackSharp.Api](https://github.com/aannenko/CrackSharp/tree/master/src/CrackSharp.Api) is a WebAPI service, ready for Docker
- [CrackSharp.Core](https://github.com/aannenko/CrackSharp/tree/master/src/CrackSharp.Core) contains encryption/decryption logic and is a common base for the two aforementioned projects.

### Usage
Deployment and usage examples are located in the corresponding project directories.

### Decryption speed
1. The longer the word behind a hash, the longer it will take to decrypt this hash.
2. Same applies to a charset you feed to the app which it then uses to build words: decryption of `50E5eT91y/Q.g` with a charset `abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789` (letters and digits) takes approximately 58% longer than with the same charset but without digits.
