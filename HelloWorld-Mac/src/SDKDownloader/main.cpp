#include "sdk_downloader.h"
#include <iostream>
#include <filesystem>
#include <string>

namespace fs = std::filesystem;

void printUsage(const char* programName) {
    std::cout << "Usage: " << programName << " [options]" << std::endl;
    std::cout << "Options:" << std::endl;
    std::cout << "  --sdk-dir <path>     : Set the SDK installation directory (default: ../sdk)" << std::endl;
    std::cout << "  --vapm-dir <path>    : Set the VAPM installation directory (default: ../vapm)" << std::endl;
    std::cout << "  --work-dir <path>    : Set the working directory (default: ./work)" << std::endl;
    std::cout << "  --license-dir <path> : Set the license directory (default: ./license)" << std::endl;
    std::cout << "  --help               : Display this help message" << std::endl;
}

int main(int argc, char** argv) {
    // Default to relative paths from the executable location
    std::string sdkDir = "../sdk";
    std::string vapmDir = "../vapm";
    std::string workDir = "../work";
    std::string licenseDir = "../license";
    
    // Parse command line arguments
    for (int i = 1; i < argc; i++) {
        std::string arg = argv[i];
        
        if (arg == "--help") {
            printUsage(argv[0]);
            return 0;
        } else if (arg == "--sdk-dir" && i + 1 < argc) {
            sdkDir = argv[++i];
        } else if (arg == "--vapm-dir" && i + 1 < argc) {
            vapmDir = argv[++i];
        } else if (arg == "--work-dir" && i + 1 < argc) {
            workDir = argv[++i];
        } else if (arg == "--license-dir" && i + 1 < argc) {
            licenseDir = argv[++i];
        } else {
            std::cerr << "Unknown option: " << arg << std::endl;
            printUsage(argv[0]);
            return 1;
        }
    }
    
    std::cout << "OESIS SDK Downloader for macOS" << std::endl;
    std::cout << "=============================" << std::endl;
    std::cout << "SDK will be installed to: " << fs::absolute(sdkDir) << std::endl;
    std::cout << "VAPM will be installed to: " << fs::absolute(vapmDir) << std::endl;
    std::cout << "Using license files from: " << fs::absolute(licenseDir) << std::endl;
    
    // Check if license directory exists
    fs::path rawLicensePath = licenseDir;
    fs::path normalizedLicensePath = rawLicensePath.lexically_normal();
    
    if (!fs::exists(normalizedLicensePath) ||
        !fs::exists(normalizedLicensePath / "download_token.txt")) {
        std::cerr << "Error: license directory or download_token.txt not found at " << licenseDir << std::endl;
        std::cerr << "Please make sure you have extracted the license files to the license directory" << std::endl;
        std::cerr << "The license directory should contain at least:" << std::endl;
        std::cerr << "  - download_token.txt" << std::endl;
        std::cerr << "  - license.cfg" << std::endl;
        return 1;
    }
    
    // Initialize the SDK downloader
    SdkDownloader downloader(sdkDir, vapmDir, workDir, licenseDir);
    
    // Start the download and setup process
    std::cout << "Starting SDK download and setup..." << std::endl;
    if (!downloader.downloadAndSetupSdk()) {
        std::cerr << "SDK setup failed" << std::endl;
        return 1;
    }
    
    return 0;
} 
