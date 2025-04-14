/**
 * @file xml_parser.h
 * @brief Parses XML data to extract package information
 */

#pragma once

#include <string>
#include <vector>
#include <tuple>
#include <tinyxml2.h>

/**
 * @class XmlParser
 * @brief Parses OESIS XML package data to extract download information
 * 
 * This class handles the parsing of XML data from the OESIS server
 * to extract package download URLs based on platform.
 */
class XmlParser {
public:
    /**
     * @brief Struct representing a package to download
     */
    struct Package {
        std::string name;    ///< Name of the package
        std::string url;     ///< Download URL
        std::string version; ///< Version of the package
    };

    /**
     * @brief Parses XML data to extract SDK package information for the current platform
     * 
     * @param xmlData The XML content to parse
     * @return std::vector<Package> List of packages to download
     */
    std::vector<Package> parseSdkPackages(const std::string& xmlData);
    
    /**
     * @brief Parses XML data to extract catalog package information
     * 
     * @param xmlData The XML content to parse
     * @return std::vector<Package> List of catalog packages to download
     */
    std::vector<Package> parseCatalogPackages(const std::string& xmlData);

private:
    /**
     * @brief Determines if a filename is an OESIS SDK package for macOS
     * 
     * @param filename The filename to check
     * @return true if the file is an SDK package for macOS
     */
    bool isSdkPackageForMacOS(const std::string& filename);
    
    /**
     * @brief Determines if a filename is a catalog package
     * 
     * @param filename The filename to check
     * @return true if the file is a catalog package
     */
    bool isCatalogPackage(const std::string& filename);
    
    /**
     * @brief Gets the current platform identifier (Mac)
     * 
     * @return std::string The platform identifier
     */
    std::string getCurrentPlatform();
}; 