# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["AntaraSoft.sln", "./"]
COPY ["src/AntaraSoft.Domain/AntaraSoft.Domain.csproj", "src/AntaraSoft.Domain/"]
COPY ["src/AntaraSoft.Application/AntaraSoft.Application.csproj", "src/AntaraSoft.Application/"]
COPY ["src/AntaraSoft.Infrastructure/AntaraSoft.Infrastructure.csproj", "src/AntaraSoft.Infrastructure/"]
COPY ["src/AntaraSoft.Api/AntaraSoft.Api.csproj", "src/AntaraSoft.Api/"]
COPY ["tests/AntaraSoft.UnitTests/AntaraSoft.UnitTests.csproj", "tests/AntaraSoft.UnitTests/"]

# Restore dependencies
RUN dotnet restore "src/AntaraSoft.Api/AntaraSoft.Api.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/AntaraSoft.Api"
RUN dotnet build "AntaraSoft.Api.csproj" -c Release -o /app/build --no-restore

# Publish stage
FROM build AS publish
RUN dotnet publish "AntaraSoft.Api.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Descarga de audio de YouTube: yt-dlp (binario autónomo) y ffmpeg para
# convertir la pista a MP3 antes de guardarla en la ruta elegida.
RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg ca-certificates curl \
    && curl -fsSL -o /usr/local/bin/yt-dlp \
       https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp_linux \
    && chmod +x /usr/local/bin/yt-dlp \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /app/wwwroot/uploads /music /music/flac

EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AntaraSoft.Api.dll"]
