/**
 * @file package_extractor.h
 * @brief Handles extraction of downloaded packages
 */

#pragma once

#include <string>
#include <unordered_map>
#include <vector>
#include <filesystem>

namespace fs = std::filesystem;

/**
 * @class PackageExtractor
 * @brief Extracts downloaded packages (ZIP and TAR formats)
 * 
 * This class provides functionality to extract downloaded packages
 * to specified directories.
 */
class PackageExtractor {
public:
    PackageExtractor();
    ~PackageExtractor();
    
    /**
     * @brief Extract a package file to the specified directory
     * 
     * @param packagePath Path to the package file
     * @param extractDir Directory to extract the package to
     * @return true if extraction was successful, false otherwise
     */
    bool extractPackage(const std::string& packagePath, const std::string& extractDir);
    
    /**
     * @brief Copy extracted files to the SDK and VAPM directories
     * 
     * @param extractDir Directory containing extracted files
     * @param sdkDir Target SDK directory
     * @param vapmDir Target VAPM directory
     * @return true if copying was successful, false otherwise
     */
    bool copyExtractedFiles(const std::string& extractDir, const std::string& sdkDir, const std::string& vapmDir);

private:
    /**
     * @brief Extract a ZIP package
     * 
     * @param packageFile Path to the ZIP package file
     * @param extractDir Directory to extract to
     * @return true if extraction was successful, false otherwise
     */
    bool extractZipPackage(const std::string& packageFile, const std::string& extractDir);
    
    /**
     * @brief Find the bin directory in the extracted files
     * 
     * @param extractDir Directory containing extracted files
     * @return std::string Path to the bin directory, or empty if not found
     */
    std::string findBinDirectory(const std::string& extractDir);
    
    /**
     * @brief Copy include files to the SDK directory
     * 
     * @param extractDir Directory containing extracted files
     * @param sdkDir Target include directory
     */
    void copyIncludeFiles(const std::string& extractDir, const std::string& sdkDir);
    
    /**
     * @brief Copy macOS library files
     * 
     * @param extractDir Directory containing extracted files
     * @param sdkDir Target SDK directory
     * @param arch Architecture (x64, arm64)
     */
    void copyMacOsLibs(const std::string& extractDir, const std::string& sdkDir, const std::string& arch);
    
    /**
     * @brief Copy VAPM files to the VAPM directory
     * 
     * @param extractDir Directory containing extracted files
     * @param vapmDir Target VAPM directory
     */
    void copyVapmFiles(const std::string& extractDir, const std::string& vapmDir);
    
    /**
     * @brief Get the current architecture
     * 
     * @return std::string Architecture identifier (x64, arm64)
     */
    std::string getCurrentArchitecture();
    
    /**
     * @brief Find files with a specific extension in a directory (recursively)
     * 
     * @param directory Directory to search in
     * @param extension File extension to search for
     * @return std::vector<fs::path> List of matching files
     */
    std::vector<fs::path> findFiles(const fs::path& directory, const std::string& extension);
    
    /**
     * @brief Find directories matching a name pattern (recursively)
     * 
     * @param directory Directory to search in
     * @param namePattern Pattern to match in directory names
     * @return std::vector<fs::path> List of matching directories
     */
    std::vector<fs::path> findDirectories(const fs::path& directory, const std::string& namePattern);
    
    /**
     * @brief List contents of a directory (with optional recursion)
     * 
     * @param directory Directory to list
     * @param depth Current recursion depth (used for indentation)
     */
    void listDirectoryContents(const fs::path& directory, int depth = 0);
}; 