///////////////////////////////////////////////////////////////////////////////////////////////
///  Header File for OPSWAT MetaDefender Endpoint Security SDK (macOS)
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///  Mac version adapted by Lucas
///////////////////////////////////////////////////////////////////////////////////////////////

#ifndef OPSWAT_UTILS_H
#define OPSWAT_UTILS_H

#include <iostream>
#include <sstream>
#include <fstream>
#include <string>
#include <locale>
#include <codecvt>    // for UTF-8 conversion
#include <unistd.h>   // for getcwd
#include <limits.h>   // for PATH_MAX
#include <cstring>    // for strlen
#include <nlohmann/json.hpp>  // JSON library

using json = nlohmann::json;
using namespace std;

// Convert UTF-8 string to wstring
wstring utf8_to_wstring(const string& str);

// Convert wstring to UTF-8 string
string wstring_to_utf8(const wstring& wstr);

// Get the current working directory path
wstring GetCurrentFolderPath();

// Trim trailing carriage return from wstring
wstring TrimCarriageReturn(const wstring& line);

// Read file content into a wstring
wstring ReadFileContentIntoWString(const wstring& filename);

// Load JSON from file into a json object
bool LoadJsonFromFile(const string& filename, json& jsonData);

// Print the current working directory
void PrintCurrentDirectory();

#endif // OPSWAT_UTILS_H 