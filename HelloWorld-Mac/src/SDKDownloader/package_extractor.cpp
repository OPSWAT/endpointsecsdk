#include "package_extractor.h"
#include <iostream>
#include <fstream>
#include <vector>
#include <cstring>
#include <filesystem>
#include <algorithm>
#include <sys/stat.h>
#include <unistd.h>

namespace fs = std::filesystem;

PackageExtractor::PackageExtractor() {
}

PackageExtractor::~PackageExtractor() {
    // Cleanup if needed
}

bool PackageExtractor::extractPackage(const std::string& packageFile, const std::string& extractDir) {
    std::cout << "Extracting package: " << packageFile << " to: " << extractDir << std::endl;
    
    // Create extraction directory if it doesn't exist
    fs::create_directories(extractDir);
    
    // Extract based on file extension
    std::string extension = fs::path(packageFile).extension().string();
    
    if (extension == ".zip" || extension == ".ZIP") {
        return extractZipPackage(packageFile, extractDir);
    } else {
        std::cerr << "Unsupported package format: " << extension << std::endl;
        return false;
    }
}

bool PackageExtractor::extractZipPackage(const std::string& packageFile, const std::string& extractDir) {
    // macOS: Use unzip
    std::string command = "unzip -o '" + packageFile + "' -d '" + extractDir + "'";
    
    std::cout << "Running extraction command: " << command << std::endl;
    int result = system(command.c_str());
    
    if (result != 0) {
        std::cerr << "Failed to extract zip package. Error code: " << result << std::endl;
        // Try using Python's zipfile module as a fallback
        std::string pyCommand = "python3 -c \"import zipfile; zipfile.ZipFile('" + 
                                packageFile + "').extractall('" + extractDir + "')\"";
        std::cout << "Trying alternative extraction method: " << pyCommand << std::endl;
        result = system(pyCommand.c_str());
        
        if (result != 0) {
            std::cerr << "Alternative extraction also failed. Error code: " << result << std::endl;
            return false;
        }
    }
    
    std::cout << "Package extraction completed successfully" << std::endl;
    return true;
}

std::string PackageExtractor::findBinDirectory(const std::string& extractDir) {
    // Try to locate the bin directory in the extracted files
    std::vector<std::string> possibleBinDirs = {
        "/bin",
        "/Bin",
        "/lib",
        "/Lib"
    };

    // First, try to find directories directly
    for (const auto& binDir : possibleBinDirs) {
        std::string fullPath = extractDir + binDir;
        if (fs::exists(fullPath) && fs::is_directory(fullPath)) {
            return fullPath;
        }
    }

    // Then, try to find in subdirectories (one level)
    for (const auto& entry : fs::directory_iterator(extractDir)) {
        if (entry.is_directory()) {
            for (const auto& binDir : possibleBinDirs) {
                std::string fullPath = entry.path().string() + binDir;
                if (fs::exists(fullPath) && fs::is_directory(fullPath)) {
                    return fullPath;
                }
            }
        }
    }

    // If we can't find it, look for the deepest directory with library files
    for (const auto& entry : fs::recursive_directory_iterator(extractDir)) {
        if (entry.is_regular_file()) {
            std::string filename = entry.path().filename().string();
            if (filename.find("waapi") != std::string::npos &&
                (filename.find(".dylib") != std::string::npos)) {
                return entry.path().parent_path().string();
            }
        }
    }

    // Not found
    std::cerr << "Warning: Could not find bin directory in extracted files" << std::endl;
    return "";
}

bool PackageExtractor::copyExtractedFiles(const std::string& extractDir, const std::string& sdkDir, const std::string& vapmDir) {
    try {
        // Create necessary directories
        fs::create_directories(fs::path(sdkDir) / "lib");
        fs::create_directories(fs::path(sdkDir) / "inc");
        fs::create_directories(vapmDir);
        
        // Get current architecture
        std::string arch = getCurrentArchitecture();
        
        std::cout << "Detected architecture: " << arch << std::endl;
        
        // Copy include files
        copyIncludeFiles(extractDir, sdkDir);
        
        // Copy macOS-specific libraries
        copyMacOsLibs(extractDir, sdkDir, arch);
        
        // Copy VAPM files
        copyVapmFiles(extractDir, vapmDir);
        
        return true;
    } catch (const std::exception& e) {
        std::cerr << "Error copying files: " << e.what() << std::endl;
        return false;
    }
}

void PackageExtractor::copyMacOsLibs(const std::string& extractDir, const std::string& sdkDir, const std::string& arch) {
    std::cout << "Copying macOS libraries..." << std::endl;
    
    // Target directories
    fs::path targetLibDir = fs::path(sdkDir) / "lib";
    fs::path targetArchLibDir = fs::path(sdkDir) / "lib" / arch;
    
    fs::create_directories(targetLibDir);
    fs::create_directories(targetArchLibDir);
    
    // Use glob patterns to find all dylib files recursively
    std::vector<fs::path> dylibs;
    bool foundLibs = false;
    
    try {
        std::cout << "Searching for dylib files in: " << extractDir << std::endl;
        
        // Search recursively for all dylib files
        for (const auto& entry : fs::recursive_directory_iterator(extractDir)) {
            if (entry.is_regular_file()) {
                std::string ext = entry.path().extension().string();
                if (ext == ".dylib" || ext == ".a" || ext == ".framework") {
                    dylibs.push_back(entry.path());
                    foundLibs = true;
                    
                    // Copy the file to architecture-specific directory
                    fs::copy_file(entry.path(), targetArchLibDir / entry.path().filename(), 
                                 fs::copy_options::overwrite_existing);
                    std::cout << "Copied to " << arch << " dir: " << entry.path().filename().string() << std::endl;
                    
                    // Also copy to the main lib directory for compatibility
                    fs::copy_file(entry.path(), targetLibDir / entry.path().filename(), 
                                 fs::copy_options::overwrite_existing);
                    std::cout << "Copied to main lib dir: " << entry.path().filename().string() << std::endl;
                }
            }
        }
    } catch (const std::exception& e) {
        std::cerr << "Error while searching for macOS libraries: " << e.what() << std::endl;
    }
    
    if (!foundLibs) {
        std::cerr << "Warning: Could not find macOS libraries" << std::endl;
    } else {
        std::cout << "Successfully copied " << dylibs.size() << " macOS libraries" << std::endl;
    }
}

void PackageExtractor::copyIncludeFiles(const std::string& extractDir, const std::string& sdkDir) {
    std::cout << "Copying include files..." << std::endl;
    
    // Target directory
    fs::path targetIncludeDir = fs::path(sdkDir) / "inc";
    fs::create_directories(targetIncludeDir);
    
    // First check common include directories
    std::vector<fs::path> sourceIncludeDirs = {
        fs::path(extractDir) / "include",
        fs::path(extractDir) / "headers",
        fs::path(extractDir) / "inc"
    };
    
    bool foundHeaders = false;
    
    // First try copying from standard include directories
    for (const auto& includeDir : sourceIncludeDirs) {
        if (fs::exists(includeDir)) {
            std::cout << "Found include files in: " << includeDir.string() << std::endl;
            
            // Copy all files and subdirectories from include directory
            for (const auto& entry : fs::recursive_directory_iterator(includeDir)) {
                fs::path relativePath = fs::relative(entry.path(), includeDir);
                fs::path targetPath = targetIncludeDir / relativePath;
                
                if (entry.is_directory()) {
                    fs::create_directories(targetPath);
                } else if (entry.is_regular_file()) {
                    fs::create_directories(targetPath.parent_path());
                    fs::copy_file(entry.path(), targetPath, fs::copy_options::overwrite_existing);
                    std::cout << "Copied include file: " << relativePath.string() << std::endl;
                    foundHeaders = true;
                }
            }
        }
    }
    
    // If no headers found in standard directories, use glob to find .h files
    if (!foundHeaders) {
        std::cout << "Searching for header files using glob pattern..." << std::endl;
        
        try {
            // Search for all .h files recursively
            for (const auto& entry : fs::recursive_directory_iterator(extractDir)) {
                if (entry.is_regular_file()) {
                    std::string ext = entry.path().extension().string();
                    if (ext == ".h" || ext == ".hpp") {
                        // Copy to flat include directory
                        fs::create_directories(targetIncludeDir);
                        fs::copy_file(entry.path(), targetIncludeDir / entry.path().filename(), 
                                     fs::copy_options::overwrite_existing);
                        std::cout << "Copied header: " << entry.path().filename().string() << std::endl;
                        foundHeaders = true;
                    }
                }
            }
        } catch (const std::exception& e) {
            std::cerr << "Error while searching for header files: " << e.what() << std::endl;
        }
    }
    
    if (!foundHeaders) {
        std::cerr << "Warning: Could not find any header files." << std::endl;
    }
}

void PackageExtractor::copyVapmFiles(const std::string& extractDir, const std::string& vapmDir) {
    std::cout << "Copying VAPM files..." << std::endl;
    
    // Define possible VAPM directories
    std::vector<fs::path> vapmDirs = {
        fs::path(extractDir) / "analog/client"
    };
    
    bool foundVapmDir = false;
    
    // Try each potential VAPM directory
    for (const auto& sourceVapmDir : vapmDirs) {
        if (fs::exists(sourceVapmDir)) {
            std::cout << "Found VAPM files in: " << sourceVapmDir.string() << std::endl;
            
            // Copy all files and subdirectories from VAPM directory
            for (const auto& entry : fs::recursive_directory_iterator(sourceVapmDir)) {
                fs::path relativePath = fs::relative(entry.path(), sourceVapmDir);
                fs::path targetPath = fs::path(vapmDir) / relativePath;
                
                if (entry.is_directory()) {
                    fs::create_directories(targetPath);
                } else if (entry.is_regular_file()) {
                    fs::create_directories(targetPath.parent_path());
                    fs::copy_file(entry.path(), targetPath, fs::copy_options::overwrite_existing);
                }
            }
            
            foundVapmDir = true;
            break;
        }
    }
    
    if (!foundVapmDir) {
        std::cerr << "Warning: Could not find VAPM directory." << std::endl;
    }
}

std::string PackageExtractor::getCurrentArchitecture() {
    #ifdef __arm64__
        return "arm64";
    #else
        return "x64";
    #endif
}

std::vector<fs::path> PackageExtractor::findFiles(const fs::path& directory, const std::string& extension) {
    std::vector<fs::path> result;
    
    if (!fs::exists(directory) || !fs::is_directory(directory)) {
        std::cout << "Warning: Directory does not exist: " << directory.string() << std::endl;
        return result;
    }
    
    try {
        for (const auto& entry : fs::recursive_directory_iterator(directory)) {
            if (fs::is_regular_file(entry) && entry.path().extension() == extension) {
                result.push_back(entry.path());
            }
        }
    } catch (const std::exception& e) {
        std::cerr << "Error while searching for files: " << e.what() << std::endl;
    }
    
    return result;
}

std::vector<fs::path> PackageExtractor::findDirectories(const fs::path& directory, const std::string& namePattern) {
    std::vector<fs::path> result;
    
    if (!fs::exists(directory) || !fs::is_directory(directory)) {
        std::cout << "Warning: Directory does not exist: " << directory.string() << std::endl;
        return result;
    }
    
    try {
        for (const auto& entry : fs::recursive_directory_iterator(directory)) {
            if (fs::is_directory(entry)) {
                std::string dirName = entry.path().filename().string();
                if (dirName.find(namePattern) != std::string::npos) {
                    result.push_back(entry.path());
                }
            }
        }
    } catch (const std::exception& e) {
        std::cerr << "Error while searching for directories: " << e.what() << std::endl;
    }
    
    return result;
}

void PackageExtractor::listDirectoryContents(const fs::path& directory, int depth) {
    if (!fs::exists(directory) || !fs::is_directory(directory)) {
        std::cout << "Directory does not exist: " << directory.string() << std::endl;
        return;
    }
    
    std::cout << "Contents of " << directory.string() << ":" << std::endl;
    
    try {
        for (const auto& entry : fs::directory_iterator(directory)) {
            std::string indentation(depth * 2, ' ');
            std::cout << indentation << entry.path().filename().string();
            
            if (fs::is_directory(entry)) {
                std::cout << " [DIR]";
                if (depth < 3) { // Limit recursion depth
                    std::cout << std::endl;
                    listDirectoryContents(entry.path(), depth + 1);
                } else {
                    std::cout << " ..." << std::endl;
                }
            } else if (fs::is_regular_file(entry)) {
                std::cout << " [" << fs::file_size(entry) << " bytes]" << std::endl;
            } else {
                std::cout << " [OTHER]" << std::endl;
            }
        }
    } catch (const std::exception& e) {
        std::cerr << "Error while listing directory contents: " << e.what() << std::endl;
    }
} 
