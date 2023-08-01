# Use SDK image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# Copy csproj and restore as distinct layers
COPY CrackSharp.Core/*.csproj CrackSharp.Core/
COPY CrackSharp.Api/*.csproj CrackSharp.Api/
RUN dotnet restore CrackSharp.Api

# Copy everything and build the app
COPY CrackSharp.Core/* CrackSharp.Core/
COPY CrackSharp.Api/* CrackSharp.Api/
ARG BINARY_VERSION
RUN (echo "$BINARY_VERSION" | grep -Eq  ^[0-9]+\.[0-9]+\.[0-9]+[a-zA-Z0-9\.-]*$) && \
    dotnet publish CrackSharp.Api -c Release -o /app --no-restore -p:Version=$BINARY_VERSION || \
    dotnet publish CrackSharp.Api -c Release -o /app --no-restore

# Use runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app

# Copy build results and run the app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "CrackSharp.Api.dll"]