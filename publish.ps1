<#
.SYNOPSIS
Publish script for PdfNorm.

.DESCRIPTION
Publishes PdfNorm as a framework-dependent deployment to the local AppData folder.
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$TargetPath = "${Env:LOCALAPPDATA}\PdfNorm"
$ProjectPath = Join-Path $PSScriptRoot 'PdfNorm\PdfNorm.csproj'

Write-Host "Publishing PdfNorm..." -ForegroundColor Cyan

# Clean target directory
if (Test-Path $TargetPath) {
    Write-Host "Cleaning target directory..." -ForegroundColor Gray
    Remove-Item -LiteralPath $TargetPath -Force -Recurse
}

# Publish the application
Write-Host "Building and publishing..." -ForegroundColor Gray
dotnet publish $ProjectPath `
    -c 'Release' `
    -r 'win-x64' `
    -o $TargetPath `
    --no-self-contained `
    /p:DebugType=None `
    /p:DebugSymbols=false `
    /p:PublishSingleFile=false `
    /p:PublishTrimmed=false

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "✓ Published successfully to: $TargetPath" -ForegroundColor Green
Write-Host ""
Write-Host "To use PdfNorm from anywhere, add this to your PATH:" -ForegroundColor Yellow
Write-Host "$TargetPath"
