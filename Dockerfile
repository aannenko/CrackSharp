FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/CrackSharp.Core/*.csproj ./CrackSharp.Core
COPY src/CrackSharp.Api/*.csproj ./CrackSharp.Api
RUN dotnet restore CrackSharp.Api

# Copy everything else and build the app
COPY src/CrackSharp.Core/. ./CrackSharp.Core
COPY src/CrackSharp.Api/. ./CrackSharp.Api
RUN dotnet publish CrackSharp.Api -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "CrackSharp.Api.dll"]