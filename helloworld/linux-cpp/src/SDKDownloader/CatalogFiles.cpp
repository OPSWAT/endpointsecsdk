///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

#include "Util.h"





// Download XML as string
void DownloadAnalog() {

    cout << "[*] Downloading: analog.zip\n";

    string token = ReadAllTextFromFile("../license/download_token.txt");
    string url = "https://vcr.opswat.com/gw/file/download/analog.zip?type=1&token=" + token;
    DownloadFile(url, "stage/analog.zip");

}


void ExtractAnalog()
{
	string localPath = "stage/analog";
	string filename = "analog.zip";

    UnzipPackage(localPath, filename);
}


void CopyClientFiles()
{
    //
    // Copy Inc files
    //
    CopyExtractedBinaries("stage/analog/client", "vapm/client");
}


void DownloadAndExtractCatalogFiles()
{
    DownloadAnalog();
	ExtractAnalog();
    
    DeleteDirectory("vapm"); // Delete the stage directory and its contents

    CopyClientFiles();

}