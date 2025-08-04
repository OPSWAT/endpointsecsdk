#pragma once

#include "wa_api.h"
#include <iostream>
#include <sstream>
#include <fstream>
#include <vector>
#include <string>
#include <stdarg.h>
#include <string.h>
#include <stdio.h>
#include <comdef.h>
#include "windows.h"

#include "json.hpp"

using namespace std;
using json = nlohmann::json;

wstring CreateJsonIn(int methodId, string signature_id = "");
int ParseCommand(int argc, char** argv, wstring& output);
bool CheckJsonKey(json js, string key);
string WStringToString(wstring wstr);
bool ReadFileContentIntoString(const char* sFilePath, string& sContent);
size_t GetSizeOfFile(const wstring& path);
wstring GetCurrentFolderPath();
wstring ReadFileContentIntoWString(const wstring& filename);
vector<wstring> Split(wstring str, wstring special_character);
// Convert a wide Unicode string to an UTF8 string
string utf8_encode(const wstring& wstr);
// Convert an UTF8 string to a wide Unicode String
wstring utf8_decode(const string& str);
int InvokeMethod(map<wstring, wstring> input, wstring& json_out);
int GetJsonValues(json& result, int rc, wstring json_out, vector<string> keys);
int CreateJsonIn(wstring& json_in, map<string, string> input);
void WriteWStringToFile(const wstring& filename, const wstring& content);
