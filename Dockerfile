# Use SDK image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy everything and build the app
COPY src/CrackSharp.Core/. ./CrackSharp.Core/
COPY src/CrackSharp.Api/. ./CrackSharp.Api/
RUN dotnet publish CrackSharp.Api -c Release -o out

# Use runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Copy build results and run the app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "CrackSharp.Api.dll", "--urls", "http://+:5000"]