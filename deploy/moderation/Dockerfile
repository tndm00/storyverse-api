# syntax=docker/dockerfile:1
# Moderation.Api  ->  Google Cloud Run
#
# Build context is the repository root (story-be-prj/):
#   docker build -f deploy/moderation/Dockerfile -t moderation .
# See deploy/README.md for the full runbook.

# ---- build --------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Restore against project files only, so the restore layer stays cached until a
# .csproj actually changes.
COPY Be.StoryVerse.sln .
COPY src/Services/Moderations/Moderation.Api/Moderation.Api.csproj src/Services/Moderations/Moderation.Api/
COPY src/Services/Moderations/Moderation.Application/Moderation.Application.csproj src/Services/Moderations/Moderation.Application/
COPY src/Services/Moderations/Moderation.Domain/Moderation.Domain.csproj src/Services/Moderations/Moderation.Domain/
COPY src/Services/Moderations/Moderation.Infrastructure/Moderation.Infrastructure.csproj src/Services/Moderations/Moderation.Infrastructure/
COPY src/Shared/Be.StoryVerse.ApiCommon/Be.StoryVerse.ApiCommon.csproj src/Shared/Be.StoryVerse.ApiCommon/
COPY src/Shared/Be.StoryVerse.Cache/Be.StoryVerse.Cache.csproj src/Shared/Be.StoryVerse.Cache/
COPY src/Shared/Be.StoryVerse.Core/Be.StoryVerse.Core.csproj src/Shared/Be.StoryVerse.Core/
COPY src/Shared/Be.StoryVerse.EventBus/Be.StoryVerse.EventBus.csproj src/Shared/Be.StoryVerse.EventBus/
COPY src/Shared/Be.StoryVerse.Shared/Be.StoryVerse.Shared.csproj src/Shared/Be.StoryVerse.Shared/
RUN dotnet restore src/Services/Moderations/Moderation.Api/Moderation.Api.csproj

COPY src/ src/
RUN dotnet publish src/Services/Moderations/Moderation.Api/Moderation.Api.csproj \
    -c "$BUILD_CONFIGURATION" -o /app --no-restore -p:UseAppHost=false

# ---- runtime ----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Non-root user baked into the .NET base images.
USER $APP_UID

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    DOTNET_gcServer=1

EXPOSE 8080

# Cloud Run injects $PORT (8080 by default). `exec` keeps the app as PID 1 so it
# receives SIGTERM for graceful shutdown.
ENTRYPOINT ["/bin/sh", "-c", "exec dotnet Moderation.Api.dll --urls http://0.0.0.0:${PORT:-8080}"]
