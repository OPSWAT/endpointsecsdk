///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

#include "Util.h"

bool IsSDKFile(const std::string& filename) {
    // Must end with .tar
    if (filename.size() < 4 || filename.substr(filename.size() - 4) != ".tar") {
        std::cout << "[DEBUG] Skipping file (not .tar): " << filename << "\n";
        return false;
    }

    // Must start with OESIS_V4_nix
    if (filename.rfind("OESIS_V4_nix", 0) == 0) {
        std::cout << "[+] Recognized SDK package: " << filename << "\n";
        return true;
    }

    std::cout << "[DEBUG] Skipping tar file (doesn't start with 'OESIS_V4_nix'): " << filename << "\n";
    return false;
}


// Parse XML and download + extract .zip files
void ParseAndHandleLinuxZips(const std::string& xmlContent) {
    std::cout << "[DEBUG] Parsing XML...\n";

    cout << xmlContent;
    XMLDocument doc;
    if (doc.Parse(xmlContent.c_str()) != XML_SUCCESS) {
        std::cerr << "[ERROR] Failed to parse XML.\n";
        return;
    }

    XMLElement* root = doc.RootElement();
    if (!root) {
        std::cerr << "[ERROR] XML has no root element.\n";
        return;
    }

    std::cout << "[DEBUG] Root element name: " << root->Name() << "\n";
    std::filesystem::create_directories("stage");
    std::filesystem::create_directories("stage/extract");

    // Loop through all <Platform> nodes regardless of their parent
    for (XMLElement* platformNode = root->FirstChildElement("Platform"); platformNode; platformNode = platformNode->NextSiblingElement("Platform")) {
        const char* platformName = platformNode->Attribute("Name");
        std::cout << "[DEBUG] Found <Platform> node with Name: " << (platformName ? platformName : "(null)") << "\n";

        if (!platformName || std::string(platformName) != "Linux") {
            std::cout << "[DEBUG] Skipping non-Linux platform.\n";
            continue;
        }

        for (XMLElement* releases = platformNode->FirstChildElement("Releases"); releases; releases = releases->NextSiblingElement("Releases")) {
            const char* releaseName = releases->Attribute("Name");
            if (!releaseName || std::string(releaseName) != "OESIS Local V4") {
                std::cout << "[DEBUG] Skipping release (not 'OESIS Local V4').\n";
                continue;
            }

            for (XMLElement* latestReleaseElement = releases->FirstChildElement("LatestRelease"); latestReleaseElement; latestReleaseElement = latestReleaseElement->NextSiblingElement("LatestRelease")) {
                for (XMLElement* package = latestReleaseElement->FirstChildElement("Package"); package; package = package->NextSiblingElement("Package")) {
                    const char* url = package->Attribute("Link");

                    std::cout << url;
                    if (!url) continue;

                    std::string linkStr = url;
                    std::string filename = linkStr.substr(linkStr.find_last_of("/") + 1);

                    if (IsSDKFile(filename))
                    {
                        std::string localPath = "stage/" + filename;

                        std::cout << "[*] Downloading: " << filename << "\n";
                        if (DownloadFile(linkStr, localPath)) {
                            UntarPackage(localPath, filename);
                        }
                        else {
                            std::cerr << "[!] Download failed: " << filename << "\n";
                        }
                    }
                }
            }
        }
    }

    std::cout << "[DEBUG] Finished processing Linux packages.\n";
}


void CopySDKFiles()
{
    //
    // Copy Detection files
    //
    CopyExtractedBinaries("stage/extract/bin/detection/x64/release", "sdk/lib/x64");
    CopyExtractedBinaries("stage/extract/bin/detection/x86/release", "sdk/lib/x86");
    CopyExtractedBinaries("stage/extract/bin/detection/arm64/release", "sdk/lib/arm64");

    //
    // Copy DeviceInfo files
    //
    CopyExtractedBinaries("stage/extract/bin/deviceinfo/x64/release", "sdk/lib/x64");
    CopyExtractedBinaries("stage/extract/bin/deviceinfo/x86/release", "sdk/lib/x86");
    CopyExtractedBinaries("stage/extract/bin/deviceinfo/arm64/release", "sdk/lib/arm64");

    //
    // Copy Manageability files
    //
    CopyExtractedBinaries("stage/extract/bin/manageability/x64/release", "sdk/lib/x64");
    CopyExtractedBinaries("stage/extract/bin/manageability/x86/release", "sdk/lib/x86");
    CopyExtractedBinaries("stage/extract/bin/manageability/arm64/release", "sdk/lib/arm64");

    //
    // Copy Manageability files
    //
    CopyExtractedBinaries("stage/extract/bin/diagnostics/x64/release", "sdk/lib/x64");
    CopyExtractedBinaries("stage/extract/bin/diagnostics/x86/release", "sdk/lib/x86");
    CopyExtractedBinaries("stage/extract/bin/diagnostics/arm64/release", "sdk/lib/arm64");


    //
    // Copy Vulnerability files
    //
    CopyExtractedBinaries("stage/extract/bin/vulnerability/x64/release", "sdk/lib/x64");
    CopyExtractedBinaries("stage/extract/bin/vulnerability/x86/release", "sdk/lib/x86");
    //CopyExtractedBinaries("stage/extract/bin/vulnerability/arm64/release", "sdk/lib/arm64");

}

void CopyIncFiles()
{
    //
    // Copy Inc files
    //
    CopyExtractedBinaries("stage/extract/inc", "sdk/inc");
}

void CopyDocsFiles()
{
    //
    // Copy Inc files
    //
    CopyExtractedBinaries("stage/extract/docs", "sdk/docs");
}


void CopyResourceFiles()
{
    //
    // Copy resource files
    //
    CopyExtractedBinaries("stage/extract/bin/x64", "sdk/lib/x64");
    CopyExtractedBinaries("stage/extract/bin/x86", "sdk/lib/x64");
    CopyExtractedBinaries("stage/extract/bin/arm64", "sdk/lib/x64");
}


// Download XML as string
std::string GetDownloadXML() {

    std::string token = ReadAllTextFromFile("../license/download_token.txt");

    std::string url = "https://vcr.opswat.com/gw/file/download/OesisPackageLinks.xml?type=1&token=" + token;
    DownloadFile(url, "stage/SDKIndex.xml");

	string result = ReadAllTextFromFile("stage/SDKIndex.xml");

    return result;
}






void DownloadAndExtractSDKFiles()
{

    string xml = GetDownloadXML();

    if (xml.empty()) {
        std::cerr << "ERROR: Empty XML response.\n";
        return;
    }

    std::cout << "\n=== Raw XML ===\n" << xml << "\n==============\n\n";
    ParseAndHandleLinuxZips(xml);


    DeleteDirectory("sdk"); // Delete the stage directory and its contents
 
    CopySDKFiles();
    CopyIncFiles();
    CopyResourceFiles();
    CopyDocsFiles();

}