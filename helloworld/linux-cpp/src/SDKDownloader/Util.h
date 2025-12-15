///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OESIS Framework
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////


#ifndef DOWNLOADER_H
#define DOWNLOADER_H

#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <filesystem>
#include <curl/curl.h>
#include <tinyxml2.h>
#include <cstdlib>

namespace fs = std::filesystem;
using namespace tinyxml2;
using namespace std;

// Curl write callback
size_t WriteCallback(void* contents, size_t size, size_t nmemb, std::string* userData);

// File write callback
size_t FileWriteCallback(void* contents, size_t size, size_t nmemb, FILE* file);

// Read the entire content from a file into a string
std::string ReadAllTextFromFile(const std::string& filename);

// Download a file from a URL and save it to disk
bool DownloadFile(const std::string& url, const std::string& outputPath);

// Unzip a package to the "stage/" directory
void UnzipPackage(const std::string& filePath, const std::string& filename);

// Extract a tar package to the "stage/extract" directory
void UntarPackage(const std::string& filePath, const std::string& filename);

// Delete a directory and all its contents
void DeleteDirectory(const std::string& dirPath);

bool CreateDirectory(const std::string& dirPath);

void CopyExtractedBinaries(const std::string& sourcePath, const std::string& destPath);

#endif // DOWNLOADER_H
