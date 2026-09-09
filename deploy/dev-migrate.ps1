# Applies EF Core migrations for all 6 StoryVerse services to local PostgreSQL.
# Prereq: PostgreSQL 16 running on localhost:5432 (user/pass postgres/postgres).
# Usage:  pwsh deploy/dev-migrate.ps1
$ErrorActionPreference = "Stop"
$env:PATH += ";$HOME\.dotnet\tools"
$root = Split-Path $PSScriptRoot -Parent

$services = @(
  @{ Name = "Authentication"; Infra = "Authentications/Authentication.Infrastructure"; Api = "Authentications/Authentication.Api" }
  @{ Name = "Content";        Infra = "Contents/Content.Infrastructure";               Api = "Contents/Content.Api" }
  @{ Name = "Community";      Infra = "Communities/Community.Infrastructure";           Api = "Communities/Community.Api" }
  @{ Name = "Library";       Infra = "Libraries/Library.Infrastructure";               Api = "Libraries/Library.Api" }
  @{ Name = "Moderation";    Infra = "Moderations/Moderation.Infrastructure";          Api = "Moderations/Moderation.Api" }
  @{ Name = "Notification";  Infra = "Notifications/Notification.Infrastructure";      Api = "Notifications/Notification.Api" }
)

foreach ($s in $services) {
  Write-Host "==> $($s.Name)" -ForegroundColor Cyan
  $infra = Join-Path $root "src/Services/$($s.Infra)"
  $api   = Join-Path $root "src/Services/$($s.Api)"
  dotnet ef database update --project $infra --startup-project $api
  if ($LASTEXITCODE -ne 0) { throw "Migration failed for $($s.Name)" }
}
Write-Host "All migrations applied." -ForegroundColor Green
