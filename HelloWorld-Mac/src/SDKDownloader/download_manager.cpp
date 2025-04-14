#include "download_manager.h"
#include <curl/curl.h>
#include <fstream>
#include <iostream>

// Callback for writing data to a string
static size_t writeToStringCallback(void* content, size_t size, size_t nmemb, std::string* userp) {
    userp->append(static_cast<char*>(content), size * nmemb);
    return size * nmemb;
}

// Callback for writing data to a file
static size_t writeToFileCallback(void* content, size_t size, size_t nmemb, FILE* file) {
    return fwrite(content, size, nmemb, file);
}

// Progress callback
static int progressCallback(void* clientp, double dltotal, double dlnow, double ultotal, double ulnow) {
    if (clientp && dltotal > 0) {
        auto callback = static_cast<std::function<void(double)>*>(clientp);
        (*callback)(dlnow / dltotal * 100.0);
    }
    return 0;
}

void* DownloadManager::initCurl(const std::string& url) {
    CURL* curl = curl_easy_init();
    if (!curl) {
        std::cerr << "Failed to initialize curl" << std::endl;
        return nullptr;
    }

    curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
    curl_easy_setopt(curl, CURLOPT_FOLLOWLOCATION, 1L);
    curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, 0L);
    curl_easy_setopt(curl, CURLOPT_SSL_VERIFYHOST, 0L);
    curl_easy_setopt(curl, CURLOPT_USERAGENT, "OESIS-SDK-Downloader/1.0");

    return curl;
}

bool DownloadManager::downloadToFile(const std::string& url, const std::string& outputPath,
                                     std::function<void(double)> progressCallback) {
    CURL* curl = static_cast<CURL*>(initCurl(url));
    if (!curl) return false;

    // Open the file for writing
    FILE* file = fopen(outputPath.c_str(), "wb");
    if (!file) {
        std::cerr << "Failed to open file for writing: " << outputPath << std::endl;
        curl_easy_cleanup(curl);
        return false;
    }

    // Set write callback
    curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, writeToFileCallback);
    curl_easy_setopt(curl, CURLOPT_WRITEDATA, file);

    // Set progress callback if provided
    std::function<void(double)>* progressCallbackPtr = nullptr;
    if (progressCallback) {
        progressCallbackPtr = new std::function<void(double)>(progressCallback);
        curl_easy_setopt(curl, CURLOPT_NOPROGRESS, 0L);
        curl_easy_setopt(curl, CURLOPT_PROGRESSFUNCTION, ::progressCallback);
        curl_easy_setopt(curl, CURLOPT_PROGRESSDATA, progressCallbackPtr);
    }

    // Perform the request
    std::cout << "Downloading: " << url << std::endl;
    CURLcode res = curl_easy_perform(curl);
    
    // Close the file
    fclose(file);
    
    // Clean up
    curl_easy_cleanup(curl);
    delete progressCallbackPtr;
    
    if (res != CURLE_OK) {
        std::cerr << "Download failed: " << curl_easy_strerror(res) << std::endl;
        return false;
    }
    
    return true;
}

std::string DownloadManager::downloadToString(const std::string& url) {
    CURL* curl = static_cast<CURL*>(initCurl(url));
    if (!curl) return "";
    
    std::string buffer;
    
    // Set write callback
    curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, writeToStringCallback);
    curl_easy_setopt(curl, CURLOPT_WRITEDATA, &buffer);
    
    // Perform the request
    CURLcode res = curl_easy_perform(curl);
    
    // Clean up
    curl_easy_cleanup(curl);
    
    if (res != CURLE_OK) {
        std::cerr << "Download failed: " << curl_easy_strerror(res) << std::endl;
        return "";
    }
    
    return buffer;
} 