# build.ps1 - Windows PowerShell build script for MetaDefender SDK Go project

param(
    [string]$Action = "build",
    [switch]$Help
)

# Enable strict mode for better error handling
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Project configuration
$PROJECT_NAME = "go-sample"
$GO_FILE = ".\src\main.go"

# Console colors
$Colors = @{
    Red    = "Red"
    Green  = "Green"
    Yellow = "Yellow"
    Blue   = "Blue"
    White  = "White"
}

# Function to print colored output
function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor $Colors.Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor $Colors.Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor $Colors.Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor $Colors.Red
}

# Detect Windows architecture
function Get-WindowsArchitecture {
    $arch = $env:PROCESSOR_ARCHITECTURE
    $wow64Arch = $env:PROCESSOR_ARCHITEW6432
    
    # Check if running under WOW64 (32-bit process on 64-bit system)
    if ($wow64Arch) {
        $arch = $wow64Arch
    }
    
    switch ($arch) {
        "AMD64" { return "amd64" }
        "ARM64" { return "arm64" }
        "x86"   { return "386" }
        default { 
            Write-Warning "Unknown architecture: $arch, defaulting to amd64"
            return "amd64" 
        }
    }
}

# Check if Go is installed
function Test-GoInstallation {
    Write-Status "Checking Go installation..."
    
    try {
        $goVersion = go version
        if ($LASTEXITCODE -ne 0) {
            throw "Go command failed"
        }
        Write-Status "Found Go version: $goVersion"
        return $true
    }
    catch {
        Write-Error "Go is not installed or not in PATH"
        Write-Status "Please install Go from https://golang.org/dl/"
        return $false
    }
}

# Check for required files and directories
function Test-RequiredFiles {
    param([string]$Architecture)
    
    Write-Status "Checking for required files..."
    
    # Check Go source file
    if (-not (Test-Path $GO_FILE)) {
        Write-Error "Go source file '$GO_FILE' not found"
        return $false
    }

    # Check for the License Directory
    if (-not (Test-Path "../../eval-license/license.cfg" -PathType Leaf)) {
        Write-Error "License.cfg file not found in %root%/eval-license/"
        Write-Status "Make sure to get the license files from the evaluation directory and unzip it in this location"
        return $false
    }

    
    # Check SDK directory
    if (-not (Test-Path "../../OPSWAT-SDK" -PathType Container)) {
        Write-Error "OPSWAT-SDK directory not found"
        Write-Status "Run the SDK Downloader tool to download the SDK. It should be in %root%/OPSWAT-SDK-Downloader/C-Sharp/bin"
        return $false
    }
    
    return $true
}

# Set up CGO environment variables
function Set-CgoEnvironment {
    param([string]$Architecture)
    
    Write-Status "Setting up CGO environment for Windows/$Architecture..."
    
    # Set Go environment variables
    $env:CGO_ENABLED = "1"
    $env:GOOS = "windows"
    $env:GOARCH = $Architecture
    
    # Set CGO flags for Windows based on architecture
    $libName = switch ($Architecture) {
        "amd64" { "x64" }
        "arm64" { "arm64" }
        "386"   { "win32" }
        default { "waapi" }
    }
    $env:CGO_LDFLAGS = "-L.\libs\win\$libName -llibwaapi"
    $env:CGO_CFLAGS = "-I.\inc"
    
    # For ARM64, we need to specify the cross-compiler
    if ($Architecture -eq "arm64") {
        # Check if MSYS2 CLANG ARM64 toolchain is available
        $clangPath = "C:\msys64\clangarm64\bin"
        if (Test-Path $clangPath) {
            Write-Status "Using MSYS2 CLANG ARM64 toolchain"
            $env:CC = "$clangPath\aarch64-w64-mingw32-clang.exe"
            $env:CXX = "$clangPath\aarch64-w64-mingw32-clang++.exe"
            $env:PATH = "$clangPath;$env:PATH"
        } else {
            Write-Warning "MSYS2 CLANG ARM64 toolchain not found at $clangPath"
            Write-Status "Make sure you have installed: pacman -S mingw-w64-clang-aarch64-toolchain"
        }
    }
    
    Write-Status "CGO environment configured:"
    Write-Status "  CGO_ENABLED: $env:CGO_ENABLED"
    Write-Status "  GOOS: $env:GOOS"
    Write-Status "  GOARCH: $env:GOARCH"
    Write-Status "  CGO_LDFLAGS: $env:CGO_LDFLAGS"
    Write-Status "  CGO_CFLAGS: $env:CGO_CFLAGS"
    if ($env:CC) { Write-Status "  CC: $env:CC" }
    if ($env:CXX) { Write-Status "  CXX: $env:CXX" }
}

function Copy-SDK-Files {
    param(
        [string]$LibPath       = "libs",
        [string]$IncPath       = "inc",
        [string]$SDKSourcePath = "..\..\OPSWAT-SDK",
        [int]$AgeThreshold     = 7
    )

    $srcWinX64 = Join-Path $SDKSourcePath "client\windows\x64"
    $doCopy    = $false

    Write-Host "Checking source files in: $srcWinX64" -ForegroundColor Cyan

    if (-not (Test-Path $srcWinX64)) {
        Write-Warning "Source path not found: $srcWinX64"
        return
    }

    $files = Get-ChildItem -Path $LibPath/win/x64 -File -ErrorAction SilentlyContinue

    if ((@($files).Count) -eq 0) {
        # Treat empty as needing refresh
        $doCopy = $true
    }
    else {
        # Trigger copy if **any** file is older than threshold
        $cutoff = (Get-Date).AddDays(-$AgeThreshold)
        $anyOld = $files | Where-Object { $_.LastWriteTime -lt $cutoff } | Select-Object -First 1
        if ($anyOld) { $doCopy = $true }
    }

    if ($doCopy) {

            Write-Host "Copying over the SDK Files" -ForegroundColor Green

            # Clear the directory
            Remove-Item -Path "$LibPath\*" -Recurse -Force -ErrorAction SilentlyContinue
            # Copy fresh files
            Copy-Item -Path "$SDKSourcePath\client\windows" -Destination $LibPath\win -Recurse -Force

            #Copy-Item -Path "$SDKSourcePath\client\mac" -Destination $LibPath\mac -Recurse -Force
            #Copy-Item -Path "$SDKSourcePath\client\linux" -Destination $LibPath\linux -Recurse -Force

            Remove-Item -Path "$IncPath\*" -Recurse -Force -ErrorAction SilentlyContinue
            # Copy fresh files
            Copy-Item -Path "$SDKSourcePath\extract\windows\inc" -Destination . -Recurse -Force

            Write-Host "Stage directory refreshed successfully." -ForegroundColor Green

    }
}



function Copy-License-Files {
    # Set your directories

    Write-Host "Copying License Files" -ForegroundColor Green

    # Copy fresh files
    Copy-Item -Path "..\..\eval-license\*" -Destination build -Recurse -Force
}




# Build the project
function Invoke-ProjectBuild {
    param([string]$Architecture)
    
    Write-Status "Building $PROJECT_NAME for Windows/$Architecture..."
    
    # Create build directory
    $buildDir = "build"
    if (-not (Test-Path $buildDir)) {
        New-Item -ItemType Directory -Path $buildDir | Out-Null
    }
    
    # Set output binary name
    $outputName = "$PROJECT_NAME.exe"
    $outputPath = "$buildDir\$outputName"
    
    # Build command
    $buildCmd = "go build -o `"$outputPath`" `"$GO_FILE`""
    
    Write-Status "Preparing SDK Files..."
    Copy-SDK-Files

    Write-Status "Running: $buildCmd"
    
    try {
        # Execute the build command
        Invoke-Expression $buildCmd
        
        if ($LASTEXITCODE -ne 0) {
            throw "Build command failed with exit code $LASTEXITCODE"
        }
        
        Write-Success "Build completed successfully!"
        Write-Success "Binary created: $outputPath"
        
        # Show file info
        if (Test-Path $outputPath) {
            $fileInfo = Get-Item $outputPath
            $sizeKB = [math]::Round($fileInfo.Length / 1KB, 2)
            Write-Status "Binary size: $sizeKB KB"
            Write-Status "Binary created: $($fileInfo.CreationTime)"
        }
        
        # Copy Windows libraries for the specific architecture to build directory
        Write-Status "Copying Windows libraries for $Architecture to $buildDir..."
        $windowsLibsDir = "libs\win"
        
        # Define architecture-specific library prefixes
        $archPrefix = switch ($Architecture) {
            "amd64" { "x64" }
            "arm64" { "arm64" }
            "386"   { "win32" }
            default { "x64" }
        }
        
        # Copy only the libraries for the current architecture
        $archLibraries = Get-ChildItem "$windowsLibsDir\$archPrefix\*.dll"
        if ($archLibraries.Count -eq 0) {
            Write-Warning "No libraries found for architecture $Architecture with prefix '$archPrefix'"
        } else {

            # Remove existing DLLs from build directory
            Remove-Item -Path (Join-Path $buildDir '*.dll') -Force -ErrorAction SilentlyContinue
            
            foreach ($lib in $archLibraries) {
                $copiedPath = Join-Path $buildDir $lib.Name
                Copy-Item $lib.FullName $copiedPath -Force

                $newName = $lib.Name -replace $archPrefix, ''
    
                if ($newName -ne $lib.Name) {
                    Rename-Item -Path $copiedPath -NewName $newName
                    Write-Status "Copied and renamed: $newName"
                } else {
                    Write-Status "Copied (no rename needed): $($lib.Name)"
                }
            }

            # Copy all .exe files from the source directory
            Get-ChildItem -Path $windowsLibsDir\$archPrefix -Filter '*.exe' -File | ForEach-Object {
            Copy-Item -Path $_.FullName -Destination $buildDir -Force
            Write-Status "Copied executable: $($_.Name)"
}

            Write-Success "Copied $($archLibraries.Count) architecture-specific libraries to $buildDir."


            Copy-License-Files
        }
        
        return $true
    }
    catch {
        Write-Error "Build failed: $($_.Exception.Message)"
        return $false
    }
}

# Clean previous builds
function Clear-BuildDirectory {
    Write-Status "Cleaning previous builds..."
    
    if (Test-Path "build") {
        Remove-Item "build" -Recurse -Force
        Write-Success "Build directory cleaned"
    } else {
        Write-Status "No build directory to clean"
    }
}

# Show help information
function Show-Help {
    Write-Host ""
    Write-Host "MetaDefender SDK Go Build Script for Windows" -ForegroundColor $Colors.Blue
    Write-Host "=============================================" -ForegroundColor $Colors.Blue
    Write-Host ""
    Write-Host "Usage: .\build.ps1 [Action] [Options]" -ForegroundColor $Colors.White
    Write-Host ""
    Write-Host "Actions:" -ForegroundColor $Colors.Yellow
    Write-Host "  build    Build the project (default)" -ForegroundColor $Colors.White
    Write-Host "  clean    Clean previous builds" -ForegroundColor $Colors.White
    Write-Host ""
    Write-Host "Options:" -ForegroundColor $Colors.Yellow
    Write-Host "  -Help    Show this help message" -ForegroundColor $Colors.White
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor $Colors.Yellow
    Write-Host "  .\build.ps1              # Build the project" -ForegroundColor $Colors.White
    Write-Host "  .\build.ps1 build        # Build the project" -ForegroundColor $Colors.White
    Write-Host "  .\build.ps1 clean        # Clean build directory" -ForegroundColor $Colors.White
    Write-Host "  .\build.ps1 -Help        # Show help" -ForegroundColor $Colors.White
    Write-Host ""
    Write-Host "This script will:" -ForegroundColor $Colors.Green
    Write-Host "  1. Detect your Windows architecture (x64/ARM64)" -ForegroundColor $Colors.White
    Write-Host "  2. Check for required Go installation and MDES SDK files" -ForegroundColor $Colors.White
    Write-Host "  3. Set up CGO environment variables" -ForegroundColor $Colors.White
    Write-Host "  4. Build the MetaDefender SDK Go project" -ForegroundColor $Colors.White
    Write-Host "  5. Copy required DLLs to build directory" -ForegroundColor $Colors.White
    Write-Host ""
}

# Main execution function
function Invoke-Main {
    param([string]$Action)
    
    Write-Status "Starting build process..."
    
    # Detect architecture
    $architecture = Get-WindowsArchitecture
    Write-Status "Detected Windows architecture: $architecture"
    
    # Handle different actions
    switch ($Action.ToLower()) {
        "clean" {
            Clear-BuildDirectory
            Write-Success "Clean completed"
            return
        }
        "build" {
            # Continue with build process
        }
        default {
            Write-Error "Unknown action: $Action"
            Show-Help
            exit 1
        }
    }
    
    # Check Go installation
    if (-not (Test-GoInstallation)) {
        exit 1
    }
    
    # Check required files
    if (-not (Test-RequiredFiles -Architecture $architecture)) {
        exit 1
    }
    
    # Set up CGO environment
    Set-CgoEnvironment -Architecture $architecture
    
    # Build the project
    if (-not (Invoke-ProjectBuild -Architecture $architecture)) {
        exit 1
    }
    
    Write-Success "Build process completed successfully!"
    Write-Status "To run the program:"
    Write-Status "  cd build"
    Write-Status "  .\$PROJECT_NAME.exe"
    Write-Host ""
    Write-Status "Required DLLs have been copied to the build directory."

    
}

# Script entry point
try {
    if ($Help) {
        Show-Help
        exit 0
    }
    
    Invoke-Main -Action $Action
}
catch {
    Write-Error "Script execution failed: $($_.Exception.Message)"
    Write-Status "Use -Help for usage information"
    exit 1
}