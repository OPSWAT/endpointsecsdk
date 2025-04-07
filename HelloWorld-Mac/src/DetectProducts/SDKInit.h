///////////////////////////////////////////////////////////////////////////////////////////////
///  Header File for OESIS Setup and Configuration (macOS)
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///  Mac version adapted by Lucas
///////////////////////////////////////////////////////////////////////////////////////////////

#ifndef OPSWAT_SETUP_H
#define OPSWAT_SETUP_H

#include "wa_api.h"
#include "Util.h"
#include <iostream>
#include <string>
#include <unistd.h>   // for getcwd
#include <limits.h>   // for PATH_MAX
#include <nlohmann/json.hpp>  // JSON library

using json = nlohmann::json;
using namespace std;

// Function to get the debug configuration as a wstring
wstring GetDebugConfig();

// Function to create the OESIS configuration string
wstring CreateConfig(bool enableDebug);

// Function to set up the OESIS SDK
int SetupOESIS();

#endif // OPSWAT_SETUP_H 