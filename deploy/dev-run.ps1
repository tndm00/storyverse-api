# Starts all 6 StoryVerse API services in the background (Development profile).
# Prereq: migrations applied (deploy/dev-migrate.ps1).
# Usage:  pwsh deploy/dev-run.ps1        (Ctrl+C then dev-stop.ps1 to stop)
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
# Each service's launchSettings.json "Development" profile pins its HTTPS port,
# which is what story-fe's Vite proxy (src/services/api/config.ts) forwards to.

$apis = @(
  "Authentications/Authentication.Api"
  "Contents/Content.Api"
  "Communities/Community.Api"
  "Libraries/Library.Api"
  "Moderations/Moderation.Api"
  "Notifications/Notification.Api"
)

$logDir = Join-Path $root "deploy/.logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

foreach ($api in $apis) {
  $name = Split-Path $api -Leaf
  $proj = Join-Path $root "src/Services/$api"
  $log  = Join-Path $logDir "$name.log"
  Write-Host "==> starting $name (log: $log)" -ForegroundColor Cyan
  Start-Process -FilePath "dotnet" -ArgumentList @("run", "--project", $proj, "--launch-profile", $name) `
    -RedirectStandardOutput $log -RedirectStandardError "$log.err" -WindowStyle Hidden
}
Write-Host "All services launching. Give them ~15s, then check deploy/.logs/*.log" -ForegroundColor Green
Write-Host "Stop with: Get-Process dotnet | Stop-Process" -ForegroundColor DarkGray
