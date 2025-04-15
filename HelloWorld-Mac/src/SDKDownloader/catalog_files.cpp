///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

#include <iostream>

using namespace std;



// Download XML as string
void DownloadAnalog() {

    cout << L"[*] Downloading: analog.zip\n";

    string token = ReadAllTextFromFile("../license/download_token.txt");
    string url = "https://vcr.opswat.com/gw/file/download/analog.zip?type=1&token=" + token;
    DownloadFile(url, "stage/analog.zip");

}


void extractAnalog()
{
    string localPath = "stage/analog";
    string filename = "analog.zip";

    UnzipPackage(localPath, filename);
}


void copyClientFiles()
{
    //
    // Copy Inc files
    //
    copyExtractedBinaries("stage/analog/client", "vapm/client");
}


void downloadAndExtractCatalogFiles()
{
    DownloadAnalog();
    ExtractAnalog();
    
    deleteDirectory("vapm"); // Delete the stage directory and its contents

    CopyClientFiles();

}
