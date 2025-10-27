///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

#include "Util.h"

// Curl write callback
size_t WriteCallback(void* contents, size_t size, size_t nmemb, std::string* userData) {
    size_t totalSize = size * nmemb;
    userData->append((char*)contents, totalSize);
    return totalSize;
}

// For downloading files to disk
size_t FileWriteCallback(void* contents, size_t size, size_t nmemb, FILE* file) {
    return fwrite(contents, size, nmemb, file);
}

std::string ReadAllTextFromFile(const std::string& filename) {
    std::ifstream file(filename);
    if (!file.is_open()) {
        throw std::runtime_error("Failed to open file: " + filename);
    }

    std::ostringstream buffer;
    buffer << file.rdbuf();  // Read the entire file into the buffer
    return buffer.str();     // Convert the buffer to a string and return
}

// Download a file and save it to disk with debug statements
bool DownloadFile(const std::string& url, const std::string& outputPath) {
    std::cout << "[DEBUG] Initializing CURL for download..." << std::endl;
    CURL* curl = curl_easy_init();
    if (!curl) {
        std::cerr << "[ERROR] CURL initialization failed." << std::endl;
        return false;
    }

    std::cout << "[DEBUG] Opening file for writing: " << outputPath << std::endl;
    FILE* file = fopen(outputPath.c_str(), "wb");
    if (!file) {
        std::cerr << "[ERROR] Failed to open file for writing: " << outputPath << std::endl;
        curl_easy_cleanup(curl);
        return false;
    }

    std::cout << "[DEBUG] Setting CURL options..." << std::endl;
    curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
    curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, FileWriteCallback);
    curl_easy_setopt(curl, CURLOPT_WRITEDATA, file);
    curl_easy_setopt(curl, CURLOPT_FOLLOWLOCATION, 1L);
    curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, 0L);
    curl_easy_setopt(curl, CURLOPT_SSL_VERIFYHOST, 0L);
    curl_easy_setopt(curl, CURLOPT_USERAGENT, "Mozilla/5.0");

    std::cout << "[DEBUG] Starting file download from URL: " << url << std::endl;
    CURLcode res = curl_easy_perform(curl);
    fclose(file);
    curl_easy_cleanup(curl);

    if (res != CURLE_OK) {
        std::cerr << "[ERROR] Download failed: " << curl_easy_strerror(res) << std::endl;
        return false;
    }

    std::cout << "[DEBUG] Download completed successfully. File saved to: " << outputPath << std::endl;
    return true;
}

void UnzipPackage(const std::string& filePath, const std::string& filename) {
    std::cout << "[+] Extracting ZIP: " << filename << "\n";
    std::string cmd = "unzip -o \"" + filePath + "\" -d stage/";
    int result = std::system(cmd.c_str());
    if (result != 0) {
        std::cerr << "[!] Failed to unzip: " << filename << "\n";
    }
}

void UntarPackage(const std::string& filePath, const std::string& filename) {
    std::cout << "[+] Extracting TAR: " << filename << "\n";
    std::string cmd = "tar -xf \"" + filePath + "\" -C stage/extract";
    int result = std::system(cmd.c_str());
    if (result != 0) {
        std::cerr << "[!] Failed to untar: " << filename << "\n";
    }
}

void DeleteDirectory(const std::string& dirPath) {
    try {
        fs::path pathToDelete(dirPath);

        if (fs::exists(pathToDelete) && fs::is_directory(pathToDelete)) {
            std::cout << "[*] Deleting directory: " << pathToDelete << "\n";

            // Removes the directory and all contents
            fs::remove_all(pathToDelete);

            std::cout << "[✓] Directory and all contents removed: " << pathToDelete << "\n";
        }
        else {
            std::cerr << "[ERROR] Directory does not exist or is not a directory: " << pathToDelete << "\n";
        }

    }
    catch (const fs::filesystem_error& ex) {
        std::cerr << "[ERROR] Filesystem error: " << ex.what() << "\n";
    }
}


bool CreateDirectory(const std::string& dirPath) {
    try {
        std::cout << "[DEBUG] Attempting to create directory: " << dirPath << std::endl;

        if (fs::exists(dirPath)) {
            std::cout << "[DEBUG] Directory already exists: " << dirPath << std::endl;
            return true;
        }

        if (fs::create_directories(dirPath)) {
            std::cout << "[INFO] Directory created successfully: " << dirPath << std::endl;
            return true;
        }
        else {
            std::cerr << "[ERROR] Failed to create directory: " << dirPath << std::endl;
            return false;
        }
    }
    catch (const fs::filesystem_error& ex) {
        std::cerr << "[ERROR] Filesystem error: " << ex.what() << std::endl;
        return false;
    }
}

void CopyExtractedBinaries(const std::string& sourcePath, const std::string& destPath) {
    try {
        // Ensure destination directory exists
        fs::create_directories(destPath);

        // Append trailing slashes to ensure correct syntax
        std::string src = sourcePath;
        if (src.back() != '/')
            src += '/';

        std::string dst = destPath;
        if (dst.back() != '/')
            dst += '/';

        // Form the command: copy everything inside the source dir to dest dir
        std::string cmd = "cp -a \"" + src + ".\" \"" + dst + "\"";

        std::cout << "[*] Copying using command: " << cmd << "\n";
        int result = std::system(cmd.c_str());

        if (result != 0) {
            std::cerr << "[ERROR] Copy command failed with code: " << result << "\n";
        }
        else {
            std::cout << "[✓] Copy completed successfully.\n";
        }

    }
    catch (const fs::filesystem_error& ex) {
        std::cerr << "[ERROR] Filesystem error: " << ex.what() << "\n";
    }
}


