/**
 * @file sdk_downloader.h
 * @brief Main SDK downloader class
 */

#pragma once

#include "download_manager.h"
#include "xml_parser.h"
#include "package_extractor.h"
#include <string>
#include <vector>
#include <filesystem>

namespace fs = std::filesystem;

/**
 * @class SdkDownloader
 * @brief Main class for downloading and setting up the OESIS SDK
 * 
 * This class coordinates the downloading, extraction, and setup
 * of the OESIS SDK files.
 */
class SdkDownloader {
public:
    /**
     * @brief Constructor
     * 
     * @param sdkDir Directory where SDK files will be stored
     * @param vapmDir Directory where VAPM files will be stored
     * @param workDir Temporary working directory for downloads and extraction
     * @param licenseDir Directory containing license files
     */
    SdkDownloader(const std::string& sdkDir, 
                 const std::string& vapmDir,
                 const std::string& workDir,
                 const std::string& licenseDir = "./license");
    
    /**
     * @brief Initialize the downloader
     * 
     * @return true if initialization was successful, false otherwise
     */
    bool initialize();
    
    /**
     * @brief Download and set up the SDK
     * 
     * @return true if the operation was successful, false otherwise
     */
    bool downloadAndSetupSdk();

private:
    DownloadManager downloadManager_;
    XmlParser xmlParser_;
    PackageExtractor packageExtractor_;
    
    std::string sdkDirectory_;
    std::string vapmDirectory_;
    std::string workingDirectory_;
    std::string stageDirectory_;
    std::string extractDirectory_;
    std::string licenseDirectory_;
    
    std::string downloadToken_;
    
    /**
     * @brief Read the download token from file
     * 
     * @return true if the token was read successfully, false otherwise
     */
    bool readDownloadToken();
    
    /**
     * @brief Download the SDK packages
     * 
     * @return true if downloads were successful, false otherwise
     */
    bool downloadSdkPackages();
    
    /**
     * @brief Extract and set up the SDK files
     * 
     * @return true if extraction and setup were successful, false otherwise
     */
    bool extractAndSetupSdk();
    
    /**
     * @brief Clean up temporary files
     * 
     * @return true if cleanup was successful, false otherwise
     */
    bool cleanup();
    
    /**
     * @brief Get the XML package list from the server
     * 
     * @return std::string The XML data or empty string on failure
     */
    std::string getPackageListXml();
}; 