# Use SDK image
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
ARG TARGETARCH
WORKDIR /source

# Copy csproj and restore as distinct layers
COPY CrackSharp.Core/*.csproj CrackSharp.Core/
COPY CrackSharp.Api/*.csproj CrackSharp.Api/
RUN dotnet restore CrackSharp.Api -a $TARGETARCH

# Copy everything and build the app
COPY CrackSharp.Core/. CrackSharp.Core/
COPY CrackSharp.Api/. CrackSharp.Api/
ARG BINARY_VERSION
RUN (echo "$BINARY_VERSION" | grep -Eq "^[0-9]+\.[0-9]+\.[0-9]+(-[a-z0-9.]+)?$") && \
    dotnet publish CrackSharp.Api --no-restore -a $TARGETARCH -o /app -p:Version=$BINARY_VERSION || \
    dotnet publish CrackSharp.Api --no-restore -a $TARGETARCH -o /app

# Use runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled
WORKDIR /app

# Copy build results and run the app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["./CrackSharp.Api"]
