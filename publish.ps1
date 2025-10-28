$ErrorActionPreference = "Stop"

$RID = "win-x64"
$Configuration = "Release"
$AppName = "CliDashboard.UI.CLI"

dotnet publish .\$AppName\$AppName.csproj `
    -c $Configuration `
    -r $RID `
    -p:SelfContained=true `
    -p:PublishSingleFile=true `
    -p:EnableCompressionInSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false `
    --output ".\publish"

# Copy the shared DLL
Copy-Item ".\CliDashboard.Shared\bin\$Configuration\net8.0\CliDashboard.Shared.dll" ".\publish\" -Force

# Remove unnecessary files ending with .pdb or .xml
Get-ChildItem -Path ".\publish\" -Include *.pdb, *.xml -Recurse | Remove-Item -Force

Write-Host ""
Write-Host "✅ Publish complete!"
Write-Host "Output folder: $(Resolve-Path .\publish)"
Write-Host ""
Write-Host "Next step: run .\install.ps1 to install under %AppData%\Programs\$AppName"
