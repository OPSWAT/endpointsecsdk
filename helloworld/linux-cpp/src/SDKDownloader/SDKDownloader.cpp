///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OESIS Framework
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

#include <iostream>    // For std::cerr and std::endl
#include <exception>   // For std::exception

#include "Util.h"
#include "SDKFiles.h"
#include "CatalogFiles.h"


void MoveStagedDeploymentUpOneLevel() {
    fs::path currentDir = fs::current_path();

    fs::path sdkPath = currentDir / "sdk";
    fs::path sdkNewLocation = currentDir.parent_path() / "sdk";

    fs::path vapmPath = currentDir / "vapm";
    fs::path vapmNewLocation = currentDir.parent_path() / "vapm";

    try {
        if (!fs::exists(sdkPath)) {
            std::cerr << "[ERROR] 'sdk' directory not found in current directory.\n";
            return;
        }

        fs::rename(sdkPath, sdkNewLocation);
        std::cout << "[✓] Moved sdk directory to: " << sdkNewLocation << "\n";

        if (!fs::exists(vapmPath)) {
            std::cerr << "[ERROR] 'vapm' directory not found in current directory.\n";
            return;
        }

        fs::rename(vapmPath, vapmNewLocation);
        std::cout << "[✓] Moved sdk directory to: " << vapmNewLocation << "\n";




    }
    catch (const fs::filesystem_error& ex) {
        std::cerr << "[ERROR] Failed to move sdk directory: " << ex.what() << "\n";
    }
}


int main() {
    
    try {

        DeleteDirectory("stage"); // Delete the stage directory and its contents
        CreateDirectory("stage");

        DownloadAndExtractSDKFiles();
        DownloadAndExtractCatalogFiles();

		MoveStagedDeploymentUpOneLevel(); // Move the sdk directory up one level

        DeleteDirectory("stage"); // Delete the stage directory and its contents

    }
    catch (const std::exception& ex) {
        std::cerr << "Exception: " << ex.what() << std::endl;
        return 1;
    }
    return 0;
}
