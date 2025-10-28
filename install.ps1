$ErrorActionPreference = "Stop"

$AppName   = "CliDashboard.UI.CLI"
$ExeName   = "CliDashboard.UI.CLI.exe"
$AppHome   = Join-Path $env:APPDATA "cli-dashboard"
$Source    = Join-Path $PSScriptRoot "publish"   # adjust if needed

New-Item -ItemType Directory -Force -Path $AppHome | Out-Null

# Copy host + Shared + any content
Copy-Item -Recurse -Force -Path (Join-Path $Source "*") -Destination $AppHome

# Put app home on PATH (user)
$pathUser = [Environment]::GetEnvironmentVariable("Path", "User")
if (-not ($pathUser -split ";" | Where-Object { $_ -eq $AppHome })) {
  [Environment]::SetEnvironmentVariable("Path", $pathUser + ";" + $AppHome, "User")
  Write-Host "Added $AppHome to PATH (user). Open a new terminal."
}

Write-Host "Installed $AppName to $AppHome"
Write-Host "Run: $ExeName"
