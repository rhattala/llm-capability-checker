# Windows Installer Packaging Script for LLM Capability Checker
# Creates a self-contained Windows installer using NSIS

param(
    [string]$Version = "1.0.0",
    [string]$Configuration = "Release",
    [ValidateSet("framework-dependent", "self-contained")]
    [string]$DeploymentMode = "self-contained"
)

$ErrorActionPreference = "Stop"

# Configuration
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$ProjectFile = "$ProjectRoot\src\LLMCapabilityChecker\LLMCapabilityChecker.csproj"
$PublishDir = "$ProjectRoot\publish\windows-x64"
$InstallerDir = "$ProjectRoot\installers"
$NSIScript = "$ProjectRoot\packaging\metadata\windows-installer.nsi"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "LLM Capability Checker - Windows Build" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Yellow
Write-Host "Mode: $DeploymentMode" -ForegroundColor Yellow
Write-Host ""

# Clean previous builds
Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Green
if (Test-Path $PublishDir) {
    Remove-Item -Recurse -Force $PublishDir
}
New-Item -ItemType Directory -Force -Path $PublishDir | Out-Null
New-Item -ItemType Directory -Force -Path $InstallerDir | Out-Null

# Publish the application
Write-Host "[2/4] Publishing application..." -ForegroundColor Green

$PublishArgs = @(
    "publish",
    $ProjectFile,
    "-c", $Configuration,
    "-r", "win-x64",
    "-o", $PublishDir,
    "/p:PublishSingleFile=true",
    "/p:IncludeNativeLibrariesForSelfExtract=true",
    "/p:Version=$Version"
)

if ($DeploymentMode -eq "self-contained") {
    $PublishArgs += "--self-contained", "true"
    $PublishArgs += "/p:PublishTrimmed=false"  # Avalonia doesn't work well with trimming
} else {
    $PublishArgs += "--self-contained", "false"
}

& dotnet $PublishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# Display published files
Write-Host ""
Write-Host "Published files:" -ForegroundColor Cyan
Get-ChildItem -Path $PublishDir | ForEach-Object {
    $size = if ($_.PSIsContainer) { "-" } else { "{0:N2} MB" -f ($_.Length / 1MB) }
    Write-Host "  $($_.Name) ($size)"
}

# Check for NSIS
Write-Host ""
Write-Host "[3/4] Checking for NSIS installer..." -ForegroundColor Green

$NSISPath = $null
$PossiblePaths = @(
    "${env:ProgramFiles}\NSIS\makensis.exe",
    "${env:ProgramFiles(x86)}\NSIS\makensis.exe",
    "C:\Program Files\NSIS\makensis.exe",
    "C:\Program Files (x86)\NSIS\makensis.exe"
)

foreach ($path in $PossiblePaths) {
    if (Test-Path $path) {
        $NSISPath = $path
        break
    }
}

if (-not $NSISPath) {
    Write-Host ""
    Write-Warning "NSIS not found. Please install NSIS from https://nsis.sourceforge.io/"
    Write-Host ""
    Write-Host "Alternative: Create ZIP archive" -ForegroundColor Yellow

    $ZipFile = "$InstallerDir\LLMCapabilityChecker-v$Version-win-x64.zip"
    if (Test-Path $ZipFile) {
        Remove-Item $ZipFile -Force
    }

    Compress-Archive -Path "$PublishDir\*" -DestinationPath $ZipFile

    Write-Host ""
    Write-Host "SUCCESS: Created portable ZIP package" -ForegroundColor Green
    Write-Host "Location: $ZipFile" -ForegroundColor Cyan
    $zipSize = (Get-Item $ZipFile).Length / 1MB
    Write-Host "Size: $("{0:N2}" -f $zipSize) MB" -ForegroundColor Cyan

    exit 0
}

# Build installer with NSIS
Write-Host "Found NSIS at: $NSISPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "[4/4] Building NSIS installer..." -ForegroundColor Green

# Update NSIS script with current version and paths
$NSIContent = Get-Content $NSIScript -Raw
$NSIContent = $NSIContent -replace '!define VERSION ".*"', "!define VERSION `"$Version`""
$NSIContent = $NSIContent -replace '!define PUBLISH_DIR ".*"', "!define PUBLISH_DIR `"$PublishDir`""
$NSIContent = $NSIContent -replace '!define OUTPUT_DIR ".*"', "!define OUTPUT_DIR `"$InstallerDir`""

$TempNSI = "$env:TEMP\llm-checker-installer.nsi"
$NSIContent | Set-Content -Path $TempNSI -Encoding UTF8

& $NSISPath $TempNSI

Remove-Item $TempNSI -Force

if ($LASTEXITCODE -ne 0) {
    Write-Error "NSIS installer build failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

$InstallerFile = "$InstallerDir\LLMCapabilityChecker-v$Version-win-x64.exe"

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "SUCCESS: Windows installer created!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Location: $InstallerFile" -ForegroundColor Cyan

if (Test-Path $InstallerFile) {
    $installerSize = (Get-Item $InstallerFile).Length / 1MB
    Write-Host "Size: $("{0:N2}" -f $installerSize) MB" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Test the installer on a clean Windows machine"
Write-Host "  2. Consider code signing the installer (signtool.exe)"
Write-Host "  3. Distribute the installer to users"
Write-Host ""
