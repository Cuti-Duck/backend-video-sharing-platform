# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend-video-sharing-platform/backend-video-sharing-platform.Api.csproj", "backend-video-sharing-platform/"]
COPY ["backend-video-sharing-platform.Application/backend-video-sharing-platform.Application.csproj", "backend-video-sharing-platform.Application/"]
COPY ["backend-video-sharing-platform.Domain/backend-video-sharing-platform.Domain.csproj", "backend-video-sharing-platform.Domain/"]
COPY ["backend-video-sharing-platform.Infrastructure/backend-video-sharing-platform.Infrastructure.csproj", "backend-video-sharing-platform.Infrastructure/"]
RUN dotnet restore "./backend-video-sharing-platform/backend-video-sharing-platform.Api.csproj"
COPY . .
WORKDIR "/src/backend-video-sharing-platform"
RUN dotnet build "./backend-video-sharing-platform.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./backend-video-sharing-platform.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "backend-video-sharing-platform.Api.dll"]
