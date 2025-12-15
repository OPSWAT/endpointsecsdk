<#
.SYNOPSIS
    Copies OPSWAT SDK client binaries for a specified architecture
    to a given Visual Studio output directory.

.PARAMETER Architecture
    The target architecture folder name (e.g., x64, win32, arm64).

.PARAMETER OutputDir
    The full path to the Visual Studio output directory.

.EXAMPLE
    .\Copy-SDK-Binaries.ps1 -Architecture x64 -OutputDir "C:\Projects\App\bin\x64\Debug"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Architecture,

    [Parameter(Mandatory = $true)]
    [string]$OutputDir
)

Write-Host ""
Write-Host "=== DEBUG PARAMETERS ===" -ForegroundColor Cyan
Write-Host ("Architecture : [{0}]" -f $Architecture)
Write-Host ("OutputDir    : [{0}]" -f $OutputDir)
Write-Host "=========================" -ForegroundColor Cyan
Write-Host ""

Write-Host "🔍 Searching for 'sdkroot' file to determine SDK base directory..." -ForegroundColor Cyan


switch ($Architecture.ToLower()) {
    "x86"   { $Architecture = "win32" }
    "amd64" { $Architecture = "x64" }
}

# Start from the current directory and walk up until 'sdkroot' is found
$currentDir = $PSScriptRoot
$sdkRootFile = $null

while ($currentDir -ne $null) {
    $potentialPath = Join-Path $currentDir "sdkroot"
    if (Test-Path $potentialPath) {
        $sdkRootFile = $potentialPath
        break
    }
    $parent = Split-Path $currentDir -Parent
    if ($parent -eq $currentDir) { break }
    $currentDir = $parent
}

if (-not $sdkRootFile) {
    Write-Error "❌ Could not find 'sdkroot' in any parent directory."
    exit 1
}

$sdkBase = Split-Path $sdkRootFile -Parent
$sdkPath = Join-Path $sdkBase "OPSWAT-SDK\client\windows\$Architecture"
$licensePath = Join-Path $sdkBase "eval-license"

if (-not (Test-Path $sdkPath)) {
    Write-Error "❌ Source path not found: $sdkPath"
    exi
}

if (-not (Test-Path $licensePath\license.cfg)) {
    Write-Error "❌ Unable to find license.cfg: $licensePat"
    exit 1
}

if (-not (Test-Path $OutputDir)) {
    Write-Host "📁 Output directory not found. Creating: $OutputDir"
    New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
}

Write-Host "📦 Copying binaries from:`n  $sdkPath`n➡️  to:`n  $OutputDir" -ForegroundColor Green
Copy-Item -Path (Join-Path $sdkPath "*") -Destination $OutputDir -Recurse -Force

Write-Host "📦 Copying license files from:`n  $licensePath`n➡️  to:`n  $OutputDir" -ForegroundColor Green
Copy-Item -Path (Join-Path $licensePath "*") -Destination $OutputDir -Recurse -Force

Write-Host "✅ Copy completed successfully for architecture: $Architecture" -ForegroundColor Yellow
