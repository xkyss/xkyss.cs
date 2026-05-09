#!/usr/bin/env pwsh

param(
    [string]$Version = "0.1.0-preview",
    [switch]$SkipAot
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$outputDir = Join-Path $root ".build\release"
$frameworkOutput = Join-Path $outputDir "framework"
$aotOutput = Join-Path $outputDir "aot-win-x64"
$zipPath = Join-Path $outputDir "MewPad-$Version-win-x64-aot.zip"

Write-Host "=== MewPad Release Build ===" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Green
Write-Host "Output:  $outputDir" -ForegroundColor Green

if (Test-Path $outputDir) {
    Remove-Item -Recurse -Force $outputDir
}
New-Item -ItemType Directory -Path $frameworkOutput -Force | Out-Null
New-Item -ItemType Directory -Path $aotOutput -Force | Out-Null

Write-Host "Building Release binaries..." -ForegroundColor Yellow
dotnet build "$root\MewPad.slnx" -c Release
if ($LASTEXITCODE -ne 0) {
    throw "Release build failed."
}

Write-Host "Publishing framework-dependent package..." -ForegroundColor Yellow
dotnet publish "$root\src\MewPad.Hosting\MewPad.Hosting.csproj" -c Release --self-contained false -o $frameworkOutput
if ($LASTEXITCODE -ne 0) {
    throw "Framework publish failed."
}

if (-not $SkipAot) {
    $vsDevCmd = "D:\Scoop\apps\vsbuildtools2022\current\vs\Common7\Tools\VsDevCmd.bat"
    if (-not (Test-Path $vsDevCmd)) {
        throw "VsDevCmd not found at $vsDevCmd"
    }

    Write-Host "Publishing NativeAOT win-x64..." -ForegroundColor Yellow
    cmd /c "`"$vsDevCmd`" -arch=amd64 -host_arch=amd64 && dotnet publish `"$root\src\MewPad.Hosting\MewPad.Hosting.csproj`" /p:PublishProfile=win-x64-aot /p:IlcUseEnvironmentalTools=true -o `"$aotOutput`""
    if ($LASTEXITCODE -ne 0) {
        throw "NativeAOT publish failed."
    }

    Compress-Archive -Path "$aotOutput\*" -DestinationPath $zipPath -Force
    $checksum = (Get-FileHash -Path $zipPath -Algorithm SHA256).Hash
    @"
# MewPad v$Version Checksums

MewPad-$Version-win-x64-aot.zip: $checksum

Algorithm: SHA256
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss K")
"@ | Set-Content -Path (Join-Path $outputDir "CHECKSUMS.txt") -Encoding utf8
}

Write-Host ""
Write-Host "=== Release Summary ===" -ForegroundColor Green
Write-Host "Framework output: $frameworkOutput"
if (-not $SkipAot) {
    Write-Host "AOT output:       $aotOutput"
    Write-Host "AOT archive:      $zipPath"
}
Write-Host "Done." -ForegroundColor Green