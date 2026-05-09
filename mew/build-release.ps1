#!/usr/bin/env pwsh
# MewPad v0.1.0-preview Release Build Script

$version = "0.1.0-preview"
$outputDir = "./.build/release"

Write-Host "=== MewPad Release Build ===" -ForegroundColor Cyan
Write-Host "Version: $version" -ForegroundColor Green
Write-Host ""

# Clean previous build
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
rm -Recurse $outputDir -Force -ErrorAction SilentlyContinue
mkdir $outputDir | Out-Null

# Build Release
Write-Host "Building Release configuration..." -ForegroundColor Yellow
dotnet build -c Release -o $outputDir/build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Create distribution archive
Write-Host "Creating distribution archive..." -ForegroundColor Yellow
$zipPath = "$outputDir/MewPad-$version.zip"
Compress-Archive -Path "$outputDir/build" -DestinationPath $zipPath -Force

# Generate checksum
Write-Host "Generating checksums..." -ForegroundColor Yellow
$checksum = (Get-FileHash -Path $zipPath -Algorithm SHA256).Hash
@"
# MewPad v$version Checksums

MewPad-$version.zip: $checksum

**Algorithm**: SHA256
**Generated**: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC")
"@ | Out-File -Path "$outputDir/CHECKSUMS.txt"

# Summary
Write-Host ""
Write-Host "=== Release Summary ===" -ForegroundColor Green
Write-Host "Version: $version"
Write-Host "Output: $outputDir"
Write-Host "Archive: $zipPath"
Write-Host "Size: $((Get-Item $zipPath).Length / 1MB | Round-Object -DecimalPlaces 2) MB"
Write-Host ""
Write-Host "✅ Release build complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:"
Write-Host "1. Tag release: git tag v$version"
Write-Host "2. Create GitHub release with: $zipPath"
Write-Host "3. Upload checksums from: $outputDir/CHECKSUMS.txt"