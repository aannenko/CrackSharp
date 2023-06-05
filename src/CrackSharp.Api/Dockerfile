# Use SDK image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# Copy csproj and restore as distinct layers
COPY src/CrackSharp.Api.sln .
COPY src/CrackSharp.Core/*.csproj ./CrackSharp.Core/
COPY src/CrackSharp.Api/*.csproj ./CrackSharp.Api/
RUN dotnet restore

# Copy everything and build the app
COPY src/CrackSharp.Core/. ./CrackSharp.Core/
COPY src/CrackSharp.Api/. ./CrackSharp.Api/
RUN dotnet publish CrackSharp.Api -c Release -o /app --no-restore

# Use runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Copy build results and run the app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "CrackSharp.Api.dll"]