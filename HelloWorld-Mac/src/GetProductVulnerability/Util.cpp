///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///  Mac version adapted by Lucas
///////////////////////////////////////////////////////////////////////////////////////////////
#include "wa_api.h"
#include <iostream>
#include <sstream>
#include <fstream>
#include <vector>
#include <string>
#include <locale>
#include <codecvt>  // for UTF-8 conversion
#include <unistd.h>   // for getcwd
#include <limits.h>   // for PATH_MAX
#include <cstring>    // for strlen
#include <nlohmann/json.hpp>  // Include the JSON library

using json = nlohmann::json;
using namespace std;

// Convert UTF-8 string to wstring
wstring utf8_to_wstring(const string& str) {
    wstring_convert<codecvt_utf8<wchar_t>> converter;
    return converter.from_bytes(str);
}

// Convert wstring to UTF-8 string
string wstring_to_utf8(const wstring& wstr) {
    wstring_convert<codecvt_utf8<wchar_t>> converter;
    return converter.to_bytes(wstr);
}

wstring GetCurrentFolderPath() {
    char currentFolder[PATH_MAX];
    if (getcwd(currentFolder, sizeof(currentFolder)) == NULL) {
        wcerr << L"[ERROR] Unable to get current directory.\n";
        return L"";
    }
    return utf8_to_wstring(currentFolder);
}

// Helper function to remove trailing carriage return
wstring TrimCarriageReturn(const wstring& line) {
    if (!line.empty() && line.back() == L'\r') {
        return line.substr(0, line.size() - 1);
    }
    return line;
}

wstring ReadFileContentIntoWString(const wstring& filename) {
    // Convert wstring filename to UTF-8 for macOS file handling
    string path = wstring_to_utf8(filename);

    wifstream inputFile(path);
    // macOS uses UTF-8 by default
    inputFile.imbue(locale("en_US.UTF-8"));  // Set locale for UTF-8

    if (!inputFile.is_open()) {
        wcerr << L"[ERROR] Unable to open file: " << filename << L"\n";
        return L"";  // Return empty wstring on failure
    }

    wstring content;
    wstring line;
    while (getline(inputFile, line)) {
        // Remove trailing carriage return if present
        content += line;  // Add newline to preserve file structure
    }
    inputFile.close();
    return content;
}

bool LoadJsonFromFile(const string& filename, json& jsonData) {
    ifstream file(filename);
    if (!file.is_open()) {
        cerr << "[ERROR] Unable to open file: " << filename << endl;
        return false;
    }

    try {
        file >> jsonData;
    }
    catch (const json::parse_error& e) {
        cerr << "[ERROR] JSON parsing error: " << e.what() << endl;
        return false;
    }

    file.close();
    return true;
}

void PrintCurrentDirectory() {
    char currentDir[PATH_MAX];
    if (getcwd(currentDir, sizeof(currentDir)) != NULL) {
        std::cout << "Current Working Directory: " << currentDir << std::endl;
    }
    else {
        std::cerr << "[ERROR] Unable to get current working directory." << std::endl;
    }
} 