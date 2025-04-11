#include "xml_parser.h"
#include <iostream>

// Define platform-specific identifiers for macOS
#define PLATFORM_NAME "Mac"
#define SDK_PREFIX "OESIS_V4_mac"
#define PACKAGE_EXT ".zip"

std::string XmlParser::getCurrentPlatform() {
    return PLATFORM_NAME;
}

bool XmlParser::isSdkPackageForMacOS(const std::string& filename) {
    // Check file extension
    if (filename.size() < 4 || filename.substr(filename.size() - 4) != PACKAGE_EXT) {
        return false;
    }
    
    // Check prefix for macOS
    if (filename.rfind(SDK_PREFIX, 0) == 0) {
        return true;
    }
    
    return false;
}

bool XmlParser::isCatalogPackage(const std::string& filename) {
    // This is the catalog package, typically named analog.zip
    return (filename == "analog.zip");
}

std::vector<XmlParser::Package> XmlParser::parseSdkPackages(const std::string& xmlData) {
    std::vector<Package> packages;
    tinyxml2::XMLDocument doc;
    
    if (doc.Parse(xmlData.c_str()) != tinyxml2::XML_SUCCESS) {
        std::cerr << "Failed to parse XML data" << std::endl;
        return packages;
    }
    
    tinyxml2::XMLElement* root = doc.RootElement();
    if (!root) {
        std::cerr << "XML has no root element" << std::endl;
        return packages;
    }
    
    // Current platform name
    std::string platformName = getCurrentPlatform();
    
    // Loop through all <Platform> nodes
    for (tinyxml2::XMLElement* platformNode = root->FirstChildElement("Platform"); 
         platformNode; 
         platformNode = platformNode->NextSiblingElement("Platform")) {
        
        const char* platformAttr = platformNode->Attribute("Name");
        if (!platformAttr || std::string(platformAttr) != platformName) {
            continue;
        }
        
        // Look for Releases
        for (tinyxml2::XMLElement* releases = platformNode->FirstChildElement("Releases"); 
             releases; 
             releases = releases->NextSiblingElement("Releases")) {
            
            const char* releaseName = releases->Attribute("Name");
            if (!releaseName || std::string(releaseName) != "OESIS Local V4") {
                continue;
            }
            
            // Find latest release
            for (tinyxml2::XMLElement* latestRelease = releases->FirstChildElement("LatestRelease"); 
                 latestRelease; 
                 latestRelease = latestRelease->NextSiblingElement("LatestRelease")) {
                
                // Get packages
                for (tinyxml2::XMLElement* package = latestRelease->FirstChildElement("Package"); 
                     package; 
                     package = package->NextSiblingElement("Package")) {
                    
                    const char* url = package->Attribute("Link");
                    const char* packageName = package->Attribute("Name");
                    const char* version = package->Attribute("Version");
                    
                    if (!url) continue;
                    
                    std::string filename = url;
                    size_t lastSlash = filename.find_last_of("/");
                    if (lastSlash != std::string::npos) {
                        filename = filename.substr(lastSlash + 1);
                    }
                    
                    if (isSdkPackageForMacOS(filename)) {
                        Package pkg;
                        pkg.name = packageName ? packageName : filename;
                        pkg.url = url;
                        pkg.version = version ? version : "unknown";
                        packages.push_back(pkg);
                    }
                }
            }
        }
    }
    
    return packages;
}

std::vector<XmlParser::Package> XmlParser::parseCatalogPackages(const std::string& xmlData) {
    std::vector<Package> packages;
    tinyxml2::XMLDocument doc;
    
    if (doc.Parse(xmlData.c_str()) != tinyxml2::XML_SUCCESS) {
        std::cerr << "Failed to parse XML data" << std::endl;
        return packages;
    }
    
    tinyxml2::XMLElement* root = doc.RootElement();
    if (!root) {
        std::cerr << "XML has no root element" << std::endl;
        return packages;
    }
    
    // For catalog files, we look for the specific package name
    // (typically "analog.zip") across all platforms
    
    for (tinyxml2::XMLElement* packageNode = root->FirstChildElement("Package"); 
         packageNode; 
         packageNode = packageNode->NextSiblingElement("Package")) {
        
        const char* url = packageNode->Attribute("Link");
        const char* packageName = packageNode->Attribute("Name");
        const char* version = packageNode->Attribute("Version");
        
        if (!url) continue;
        
        std::string filename = url;
        size_t lastSlash = filename.find_last_of("/");
        if (lastSlash != std::string::npos) {
            filename = filename.substr(lastSlash + 1);
        }
        
        if (isCatalogPackage(filename)) {
            Package pkg;
            pkg.name = packageName ? packageName : filename;
            pkg.url = url;
            pkg.version = version ? version : "unknown";
            packages.push_back(pkg);
        }
    }
    
    return packages;
} 