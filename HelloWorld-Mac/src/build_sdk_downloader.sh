#!/bin/bash

# Ensure the script exits if any command fails
set -e

# Check if the license file exists
if [ ! -e "license/license.cfg" ]; then
    echo "Error: License file 'license/license.cfg' not found."
    echo "Please ensure the license files have been extracted to the 'license' directory before running this script."
    exit 1
fi

# Check for download_token.txt in the license directory
if [ ! -e "license/download_token.txt" ]; then
    echo "Error: Download token file 'license/download_token.txt' not found."
    echo "Please ensure the license files have been extracted to the 'license' directory before running this script."
    exit 1
fi

echo "Cleaning up old SDK files..."
rm -rf sdk
rm -rf stage # Also remove stage if SDKDownloader uses it temporarily
rm -rf work  # Remove work directory
mkdir -p sdk/lib/{x64,arm64} sdk/inc sdk/docs

echo "Building SDKDownloader..."
if [ ! -d "SDKDownloader" ]; then
    echo "Error: SDKDownloader directory not found."
    exit 1
fi
cd SDKDownloader

# Check if brew is installed
if ! command -v brew &> /dev/null; then
    echo "Error: Homebrew is not installed. Please install Homebrew first."
    echo "You can install Homebrew by running:"
    echo "/bin/bash -c \"\$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)\""
    exit 1
fi

# Check and install required packages
echo "Checking and installing required packages..."
brew list curl &>/dev/null || brew install curl
brew list tinyxml2 &>/dev/null || brew install tinyxml2

# First try to build using the Makefile
echo "Building using Makefile..."
if ! make clean; then
    echo "Warning: 'make clean' failed. This might be expected for a first-time build."
fi

if ! make; then
    echo "Error: 'make' failed. Trying alternative build with CMake..."
    
    # Try CMake as a fallback
    if ! command -v cmake &> /dev/null; then
        echo "Installing CMake..."
        brew install cmake
    fi
    
    rm -rf build
    mkdir -p build
    cd build
    
    if ! cmake ..; then
        echo "Error: CMake configuration failed."
        cd ../..
        exit 1
    fi
    
    if ! make; then
        echo "Error: CMake build failed."
        cd ../..
        exit 1
    fi
    
    # Copy the executable to the parent directory
    cp sdk_downloader ..
    cd ..
else
    echo "Makefile build succeeded."
fi

# Check if the executable exists
if [ ! -f "./SDKDownloader" ] && [ ! -f "./sdk_downloader" ]; then
    echo "Error: Failed to build SDKDownloader executable."
    cd ..
    exit 1
fi

# Determine which executable to run
if [ -f "./SDKDownloader" ]; then
    EXECUTABLE="./SDKDownloader"
else
    EXECUTABLE="./sdk_downloader"
fi

echo "Running SDKDownloader..."
if ! $EXECUTABLE --sdk-dir "../sdk" --vapm-dir "../vapm" --work-dir "../work" --license-dir "../license"; then
    echo "Error: Failed to execute SDKDownloader."
    cd ..
    exit 1
fi

cd ..

# Copy required data files for the GetProductVulnerability module
echo "Copying vulnerability data files to GetProductVulnerability directory..."
if [ -f "vapm/mav.dat" ] && [ -f "vapm/v2mod.dat" ]; then
    mkdir -p "GetProductVulnerability"
    cp "vapm/mav.dat" "GetProductVulnerability/"
    cp "vapm/v2mod.dat" "GetProductVulnerability/"
    echo "Data files copied successfully to GetProductVulnerability directory."
else
    echo "Warning: Vulnerability data files not found in vapm directory. GetProductVulnerability example may not work correctly."
fi

# Clean up SDKDownloader temporary files if any (e.g., stage)
echo "Cleaning up temporary files..."
rm -rf stage
rm -rf work

echo "SDK download and setup complete!"
echo "SDK files are located in: $(pwd)/sdk"
echo "  - Library files: sdk/lib/{x64,arm64}"
echo "  - Include files: sdk/inc"
echo "  - Documentation: sdk/docs"

if [ -d "vapm" ]; then
    echo "  - VAPM files: $(pwd)/vapm"
fi

exit 0 