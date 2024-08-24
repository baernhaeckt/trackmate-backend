# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/TrackMate.Backend.RestApi/TrackMate.Backend.RestApi.csproj", "TrackMate.Backend.RestApi/"]
COPY ["src/Trackmate.Backend/Trackmate.Backend.csproj", "Trackmate.Backend/"]
COPY ["src/TrackMate.Backend.Neo4J/TrackMate.Backend.Neo4J.csproj", "TrackMate.Backend.Neo4J/"]
WORKDIR /src/TrackMate.Backend.RestApi
RUN dotnet restore
WORKDIR /src
COPY src/ .
WORKDIR "/src/TrackMate.Backend.RestApi"
RUN dotnet build -c $BUILD_CONFIGURATION --no-restore -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/TrackMate.Backend.RestApi"
RUN dotnet publish -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrackMate.Backend.RestApi.dll"]
