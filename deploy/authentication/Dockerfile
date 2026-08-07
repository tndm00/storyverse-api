FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Be.StoryVerse.sln .
COPY src/Services/Authentications/Authentication.Api/Authentication.Api.csproj src/Services/Authentications/Authentication.Api/
COPY src/Services/Authentications/Authentication.Application/Authentication.Application.csproj src/Services/Authentications/Authentication.Application/
COPY src/Services/Authentications/Authentication.Domain/Authentication.Domain.csproj src/Services/Authentications/Authentication.Domain/
COPY src/Services/Authentications/Authentication.Infrastructure/Authentication.Infrastructure.csproj src/Services/Authentications/Authentication.Infrastructure/
COPY src/Shared/Be.StoryVerse.ApiCommon/Be.StoryVerse.ApiCommon.csproj src/Shared/Be.StoryVerse.ApiCommon/
COPY src/Shared/Be.StoryVerse.Cache/Be.StoryVerse.Cache.csproj src/Shared/Be.StoryVerse.Cache/
COPY src/Shared/Be.StoryVerse.Core/Be.StoryVerse.Core.csproj src/Shared/Be.StoryVerse.Core/
COPY src/Shared/Be.StoryVerse.EventBus/Be.StoryVerse.EventBus.csproj src/Shared/Be.StoryVerse.EventBus/
COPY src/Shared/Be.StoryVerse.Shared/Be.StoryVerse.Shared.csproj src/Shared/Be.StoryVerse.Shared/

RUN dotnet restore src/Services/Authentications/Authentication.Api/Authentication.Api.csproj

COPY src/ src/

RUN dotnet publish src/Services/Authentications/Authentication.Api/Authentication.Api.csproj \
    -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# GCP Cloud Run / Cloud Storage / Secret Manager expect the container to
# listen on $PORT; see code-standard.md section 39 (GCP Usage Rules).
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Authentication.Api.dll"]
