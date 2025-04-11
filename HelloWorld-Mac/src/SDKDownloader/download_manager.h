/**
 * @file download_manager.h
 * @brief Handles HTTP downloads using libcurl
 */

#pragma once

#include <string>
#include <functional>
#include <memory>

/**
 * @class DownloadManager
 * @brief Manages downloads from HTTP sources using libcurl
 * 
 * This class provides methods to download files from URLs and save them
 * to the local filesystem or return their content as a string.
 */
class DownloadManager {
public:
    /**
     * @brief Download a file from a URL to a local file
     * 
     * @param url The source URL
     * @param outputPath The local file path where the downloaded file will be saved
     * @param progressCallback Optional callback function for download progress updates
     * @return true if download was successful, false otherwise
     */
    bool downloadToFile(const std::string& url, const std::string& outputPath, 
                        std::function<void(double)> progressCallback = nullptr);
    
    /**
     * @brief Download content from a URL and return it as a string
     * 
     * @param url The source URL
     * @return std::string The downloaded content or empty string on failure
     */
    std::string downloadToString(const std::string& url);

private:
    /**
     * @brief Initialize curl for a request
     * 
     * @param url The URL to access
     * @return void* The initialized curl handle or nullptr on failure
     */
    void* initCurl(const std::string& url);
}; 