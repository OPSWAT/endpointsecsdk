///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////
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


#ifdef _WIN32
#include <comdef.h>
#include "windows.h"
#else
#include <unistd.h>
#include <codecvt>
#endif // _WIN32

using namespace std;

/*
#include "json.hpp"

using json = nlohmann::json;
*/

class Utils
{
public:
	static wstring ReadFileContentIntoWString(const wstring& filename);
	static size_t GetSizeOfFile(const wstring& path);
	static wstring GetCurrentFolderPath();
};
#pragma once
