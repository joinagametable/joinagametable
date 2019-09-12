# Build
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY JoinAGameTable/*.csproj ./JoinAGameTable/
RUN dotnet restore

# Copy everything else and build app
COPY JoinAGameTable/. ./JoinAGameTable/
WORKDIR /app/JoinAGameTable
RUN dotnet publish -c Release -o out

# Run
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY --from=build /app/JoinAGameTable/out ./
ENTRYPOINT ["dotnet", "JoinAGameTable.dll"]
