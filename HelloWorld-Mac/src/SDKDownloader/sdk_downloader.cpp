#include "sdk_downloader.h"
#include <iostream>
#include <fstream>


SdkDownloader::SdkDownloader(const std::string& sdkDir, 
                           const std::string& vapmDir,
                           const std::string& workDir,
                           const std::string& licenseDir)
    : sdkDirectory_(sdkDir),
      vapmDirectory_(vapmDir),
      workingDirectory_(workDir),
      licenseDirectory_(licenseDir) {
    
    // Set up stage and extract directories
    stageDirectory_ = workingDirectory_ + "/stage";
    extractDirectory_ = stageDirectory_ + "/extract";
}

bool SdkDownloader::initialize() {
    try {
        // Create working directories
        fs::create_directories(workingDirectory_);
        fs::create_directories(stageDirectory_);
        fs::create_directories(extractDirectory_);
        
        // Read download token
        if (!readDownloadToken()) {
            std::cerr << "Failed to read download token" << std::endl;
            return false;
        }
        
        return true;
    } catch (const std::exception& e) {
        std::cerr << "Initialization failed: " << e.what() << std::endl;
        return false;
    }
}

bool SdkDownloader::readDownloadToken() {
    try {
        std::string tokenFile = fs::path(licenseDirectory_) / "download_token.txt";
        std::ifstream file(tokenFile);
        if (!file.is_open()) {
            std::cerr << "Failed to open token file: " << tokenFile << std::endl;
            return false;
        }
        
        std::getline(file, downloadToken_);
        file.close();
        
        if (downloadToken_.empty()) {
            std::cerr << "Download token is empty" << std::endl;
            return false;
        }
        
        return true;
    } catch (const std::exception& e) {
        std::cerr << "Failed to read download token: " << e.what() << std::endl;
        return false;
    }
}

std::string SdkDownloader::getPackageListXml() {
    std::string url = "https://vcr.opswat.com/gw/file/download/OesisPackageLinks.xml?type=1&token=" + downloadToken_;
    return downloadManager_.downloadToString(url);
}

bool SdkDownloader::downloadSdkPackages() {
    try {
        // Get package list XML
        std::string xml = getPackageListXml();
        if (xml.empty()) {
            std::cerr << "Failed to get package list XML" << std::endl;
            return false;
        }
        
        // Parse SDK packages
        auto sdkPackages = xmlParser_.parseSdkPackages(xml);
        if (sdkPackages.empty()) {
            std::cerr << "No SDK packages found for macOS" << std::endl;
            return false;
        }
        
        // Download SDK packages
        for (const auto& package : sdkPackages) {
            std::string fileName = package.url.substr(package.url.find_last_of("/") + 1);
            std::string outputPath = stageDirectory_ + "/" + fileName;
            
            std::cout << "Downloading SDK package: " << package.name << " (" << package.version << ")" << std::endl;
            
            if (!downloadManager_.downloadToFile(package.url, outputPath)) {
                std::cerr << "Failed to download SDK package: " << package.url << std::endl;
                return false;
            }
            
            // Extract the package
            if (!packageExtractor_.extractPackage(outputPath, extractDirectory_)) {
                std::cerr << "Failed to extract SDK package: " << outputPath << std::endl;
                return false;
            }
        }
        
        // Download catalog files (analog.zip)
        std::string catalogUrl = "https://vcr.opswat.com/gw/file/download/analog.zip?type=1&token=" + downloadToken_;
        std::string catalogOutputPath = stageDirectory_ + "/analog.zip";
        
        std::cout << "Downloading catalog package (analog.zip)" << std::endl;
        
        if (!downloadManager_.downloadToFile(catalogUrl, catalogOutputPath)) {
            std::cerr << "Failed to download catalog package: " << catalogUrl << std::endl;
            return false;
        }
        
        // Extract catalog package
        if (!packageExtractor_.extractPackage(catalogOutputPath, extractDirectory_)) {
            std::cerr << "Failed to extract catalog package: " << catalogOutputPath << std::endl;
            return false;
        }
        
        return true;
    } catch (const std::exception& e) {
        std::cerr << "Error downloading SDK packages: " << e.what() << std::endl;
        return false;
    }
}

bool SdkDownloader::extractAndSetupSdk() {
    try {
        // Clean existing SDK and VAPM directories
        if (fs::exists(sdkDirectory_)) {
            fs::remove_all(sdkDirectory_);
        }
        
        if (fs::exists(vapmDirectory_)) {
            fs::remove_all(vapmDirectory_);
        }
        
        // Create fresh directories
        fs::create_directories(sdkDirectory_);
        fs::create_directories(vapmDirectory_);
        
        // Copy SDK files from extracted packages
        if (!packageExtractor_.copyExtractedFiles(extractDirectory_, sdkDirectory_, vapmDirectory_)) {
            std::cerr << "Failed to copy SDK files" << std::endl;
            return false;
        }
        
        return true;
    } catch (const std::exception& e) {
        std::cerr << "Error setting up SDK: " << e.what() << std::endl;
        return false;
    }
}

bool SdkDownloader::cleanup() {
    try {
        // Remove the staging directory
        if (fs::exists(stageDirectory_)) {
            std::cout << "Cleaning up temporary files..." << std::endl;
            fs::remove_all(stageDirectory_);
        }
        
        return true;
    } catch (const std::exception& e) {
        std::cerr << "Error during cleanup: " << e.what() << std::endl;
        return false;
    }
}

bool SdkDownloader::downloadAndSetupSdk() {
    if (!initialize()) {
        return false;
    }
    
    if (!downloadSdkPackages()) {
        return false;
    }
    
    if (!extractAndSetupSdk()) {
        return false;
    }
    
    if (!cleanup()) {
        std::cerr << "Warning: Failed to clean up temporary files" << std::endl;
        // Continue despite cleanup failure
    }
    
    std::cout << "SDK setup completed successfully!" << std::endl;
    std::cout << "SDK files installed to: " << fs::absolute(sdkDirectory_) << std::endl;
    std::cout << "VAPM files installed to: " << fs::absolute(vapmDirectory_) << std::endl;
    
    return true;
} 
