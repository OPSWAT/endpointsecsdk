///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///  Mac version adapted by Lucas
///////////////////////////////////////////////////////////////////////////////////////////////

#include "Util.h"

bool IsSDKFile(const std::string& filename) {
    // Check if it's zip file first (seems like Mac files are zips, not tars)
    if (filename.size() < 4 || filename.substr(filename.size() - 4) != ".zip") {
        std::cout << "[DEBUG] Skipping file (not .zip): " << filename << "\n";
        return false;
    }

    // Must start with OESIS_V4_mac
    if (filename.rfind("OESIS_V4_mac", 0) == 0) {
        std::cout << "[+] Recognized SDK package: " << filename << "\n";
        return true;
    }

    std::cout << "[DEBUG] Skipping zip file (doesn't start with 'OESIS_V4_mac'): " << filename << "\n";
    return false;
}


// Parse XML and download + extract .tar files
void ParseAndHandleMacZips(const std::string& xmlContent) {
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

        if (!platformName || std::string(platformName) != "Mac") {
            std::cout << "[DEBUG] Skipping non-macOS platform.\n";
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
                    const char* packageName = package->Attribute("Name");

                    if (!url) continue;

                    std::string linkStr = url;
                    std::cout << "[DEBUG] Package URL: " << linkStr << std::endl;
                    
                    if (packageName) {
                        std::cout << "[DEBUG] Package Name: " << packageName << std::endl;
                    }
                    
                    std::string filename = linkStr.substr(linkStr.find_last_of("/") + 1);

                    if (IsSDKFile(filename))
                    {
                        std::string localPath = "stage/" + filename;

                        std::cout << "[*] Downloading: " << filename << "\n";
                        if (DownloadFile(linkStr, localPath)) {
                            UnzipPackage(localPath, filename);
                        }
                        else {
                            std::cerr << "[!] Download failed: " << filename << "\n";
                        }
                    }
                }
            }
        }
    }

    std::cout << "[DEBUG] Finished processing macOS packages.\n";
}


void CopySDKFiles()
{
    // Create SDK directory structure if it doesn't exist
    CreateDirectory("sdk");
    CreateDirectory("sdk/lib");
    CreateDirectory("sdk/lib/x64");
    CreateDirectory("sdk/lib/arm64");
    CreateDirectory("sdk/inc");
    CreateDirectory("sdk/docs");

    // First try the standard directory structure (from extraction)
    //
    // Copy Detection files
    //
    CopyExtractedBinaries("stage/extract/bin/detection/x64/release", "sdk/lib/x64");
    CopyExtractedBinaries("stage/extract/bin/detection/arm64/release", "sdk/lib/arm64");

    //
    // Copy DeviceInfo files
    //
    CopyExtractedBinaries("stage/extract/bin/deviceinfo/x64/release", "sdk/lib/x64");
    CopyExtractedBinaries("stage/extract/bin/deviceinfo/arm64/release", "sdk/lib/arm64");

    //
    // Copy Manageability files
    //
    CopyExtractedBinaries("stage/extract/bin/manageability/x64/release", "sdk/lib/x64");
    CopyExtractedBinaries("stage/extract/bin/manageability/arm64/release", "sdk/lib/arm64");

    //
    // Copy Diagnostics files
    //
    CopyExtractedBinaries("stage/extract/bin/diagnostics/x64/release", "sdk/lib/x64");
    CopyExtractedBinaries("stage/extract/bin/diagnostics/arm64/release", "sdk/lib/arm64");

    //
    // Copy Vulnerability files
    //
    CopyExtractedBinaries("stage/extract/bin/vulnerability/x64/release", "sdk/lib/x64");
    CopyExtractedBinaries("stage/extract/bin/vulnerability/arm64/release", "sdk/lib/arm64");

    // Now check for alternative nested structure from Engine Package
    //
    // Search for lib files in stage directory (handle nested folders)
    //
    std::string cmd = "find stage -name \"*.dylib\" -o -name \"*.so\" -o -name \"*.a\" | sort";
    FILE* pipe = popen(cmd.c_str(), "r");
    if (pipe) {
        char buffer[1024];
        while (fgets(buffer, sizeof(buffer), pipe) != NULL) {
            std::string path(buffer);
            // Remove trailing newline
            if (!path.empty() && path[path.length() - 1] == '\n') {
                path.erase(path.length() - 1);
            }
            
            std::cout << "[DEBUG] Found library: " << path << std::endl;
            
            // Determine architecture (x64 or arm64) from path
            if (path.find("arm64") != std::string::npos) {
                // Copy to arm64 directory
                std::string filename = path.substr(path.find_last_of("/") + 1);
                std::string destPath = "sdk/lib/arm64/" + filename;
                std::string cpCmd = "cp \"" + path + "\" \"" + destPath + "\"";
                std::system(cpCmd.c_str());
                std::cout << "[+] Copied to arm64: " << filename << std::endl;
            } else {
                // Default to x64
                std::string filename = path.substr(path.find_last_of("/") + 1);
                std::string destPath = "sdk/lib/x64/" + filename;
                std::string cpCmd = "cp \"" + path + "\" \"" + destPath + "\"";
                std::system(cpCmd.c_str());
                std::cout << "[+] Copied to x64: " << filename << std::endl;
            }
        }
        pclose(pipe);
    }

    // Search for header files
    cmd = "find stage -name \"*.h\" | sort";
    pipe = popen(cmd.c_str(), "r");
    if (pipe) {
        char buffer[1024];
        while (fgets(buffer, sizeof(buffer), pipe) != NULL) {
            std::string path(buffer);
            // Remove trailing newline
            if (!path.empty() && path[path.length() - 1] == '\n') {
                path.erase(path.length() - 1);
            }
            
            std::cout << "[DEBUG] Found header: " << path << std::endl;
            
            // Copy to include directory
            std::string filename = path.substr(path.find_last_of("/") + 1);
            std::string destPath = "sdk/inc/" + filename;
            std::string cpCmd = "cp \"" + path + "\" \"" + destPath + "\"";
            std::system(cpCmd.c_str());
            std::cout << "[+] Copied header: " << filename << std::endl;
        }
        pclose(pipe);
    }
}

void CopyIncFiles()
{
    //
    // Copy Inc files
    //
    CopyExtractedBinaries("stage/extract/inc", "sdk/inc");
    
    // Also look for headers in stage/include if it exists
    CopyExtractedBinaries("stage/include", "sdk/inc");
}

void CopyDocsFiles()
{
    //
    // Copy doc files
    //
    CopyExtractedBinaries("stage/extract/docs", "sdk/docs");
    CopyExtractedBinaries("stage/docs", "sdk/docs");
}


void CopyResourceFiles()
{
    //
    // Copy resource files
    //
    CopyExtractedBinaries("stage/extract/bin/x64", "sdk/lib/x64");
    CopyExtractedBinaries("stage/extract/bin/arm64", "sdk/lib/arm64");
    
    // Also look for lib files directly in bin folder
    CopyExtractedBinaries("stage/bin/x64", "sdk/lib/x64");
    CopyExtractedBinaries("stage/bin/arm64", "sdk/lib/arm64");
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
    ParseAndHandleMacZips(xml);

    DeleteDirectory("sdk"); // Delete the sdk directory and its contents
 
    CopySDKFiles();
    CopyIncFiles();
    CopyResourceFiles();
    CopyDocsFiles();
} 