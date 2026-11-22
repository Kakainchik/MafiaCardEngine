# Stage 1: Base runtime for Linux containers
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: SDK Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project definition files for layer caching
COPY ["WebServer/WebServer.csproj", "WebServer/"]
COPY ["WebServer.Shared/WebServer.Shared.csproj", "WebServer.Shared/"]
COPY ["GameLogic/GameLogic.csproj", "GameLogic/"]

RUN dotnet restore "WebServer/WebServer.csproj"

# Copy full source tree and build
COPY . .
WORKDIR "/src/WebServer"
RUN dotnet build "WebServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish binaries
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final production container image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Environment settings for ASP.NET in Docker
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "WebServer.dll"]
