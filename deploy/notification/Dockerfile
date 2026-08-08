FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Be.StoryVerse.sln .
COPY src/Services/Notifications/Notification.Api/Notification.Api.csproj src/Services/Notifications/Notification.Api/
COPY src/Services/Notifications/Notification.Application/Notification.Application.csproj src/Services/Notifications/Notification.Application/
COPY src/Services/Notifications/Notification.Domain/Notification.Domain.csproj src/Services/Notifications/Notification.Domain/
COPY src/Services/Notifications/Notification.Infrastructure/Notification.Infrastructure.csproj src/Services/Notifications/Notification.Infrastructure/
COPY src/Shared/Be.StoryVerse.ApiCommon/Be.StoryVerse.ApiCommon.csproj src/Shared/Be.StoryVerse.ApiCommon/
COPY src/Shared/Be.StoryVerse.Cache/Be.StoryVerse.Cache.csproj src/Shared/Be.StoryVerse.Cache/
COPY src/Shared/Be.StoryVerse.Core/Be.StoryVerse.Core.csproj src/Shared/Be.StoryVerse.Core/
COPY src/Shared/Be.StoryVerse.EventBus/Be.StoryVerse.EventBus.csproj src/Shared/Be.StoryVerse.EventBus/
COPY src/Shared/Be.StoryVerse.Shared/Be.StoryVerse.Shared.csproj src/Shared/Be.StoryVerse.Shared/

RUN dotnet restore src/Services/Notifications/Notification.Api/Notification.Api.csproj

COPY src/ src/

RUN dotnet publish src/Services/Notifications/Notification.Api/Notification.Api.csproj \
    -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# GCP Cloud Run / Cloud Storage / Secret Manager expect the container to
# listen on $PORT; see code-standard.md section 39 (GCP Usage Rules).
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Notification.Api.dll"]
