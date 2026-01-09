# Use SDK image
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG TARGETARCH
ARG BINARY_VERSION
WORKDIR /source

# Copy everything and publish the app
COPY CrackSharp.Core/. CrackSharp.Core/
COPY CrackSharp.Api/. CrackSharp.Api/
RUN (echo "$BINARY_VERSION" | grep -Eq "^[0-9]+\.[0-9]+\.[0-9]+(-[a-z0-9.]+)?$") && \
    dotnet publish CrackSharp.Api -a $TARGETARCH -o /app -p:Version=$BINARY_VERSION || \
    dotnet publish CrackSharp.Api -a $TARGETARCH -o /app

# Use runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
ENTRYPOINT ["./CrackSharp.Api"]
