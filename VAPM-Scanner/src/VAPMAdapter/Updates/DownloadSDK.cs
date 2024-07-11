///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VAPMAdapater.Updates
{
    /// <summary>
    /// Represents a class responsible for downloading the SDK files.
    /// </summary>
    internal class DownloadSDK
    {

        /// <summary>
        /// Retrieves the value of a specified attribute from an XElement.
        /// </summary>
        /// <param name="element">The XElement from which to get the attribute value.</param>
        /// <param name="key">The key of the attribute.</param>
        /// <returns>The value of the attribute as a string, or an empty string if the attribute is not found.</returns>
        private static string getAttribute(XElement element, string key)
        {
            string result = "";

            if (element != null)
            {
                XAttribute xmlValue = element.Attribute(key);
                if (xmlValue != null)
                {
                    result = (string)xmlValue;
                }
            }

            return result;
        }

        /// <summary>
        /// Downloads the release files specified in the releaseELement to the destination path.
        /// </summary>
        /// <param name="releaseELement">The XElement containing the release details.</param>
        /// <param name="destPath">The destination path where the files will be saved.</param>
        private static void DownloadReleaseFiles(XElement releaseELement, string destPath)
        {
            if (releaseELement != null)
            {
                foreach (XElement packageElement in releaseELement.Elements())
                {
                    if (packageElement.Name == "Package")
                    {

                        // Get the file URL and SHA256 hash from the package element
                        string fileUrl = getAttribute(packageElement, "Link");
                        string sha256Hash = getAttribute(packageElement, "sha256");

                        // Extract the file name from the URL and determine the local file path
                        string fileName = Path.GetFileName(fileUrl);
                        string localFilePath = Path.Combine(destPath, fileName);

                        // Only download files that are not adapters and end with ".zip"
                        if (!fileName.Contains("Adapter") && fileName.EndsWith(".zip"))
                        {
                            HttpClientUtils.DownloadValidFile(fileUrl, localFilePath, sha256Hash);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Downloads the platform files specified in the platformElement to the destination path.
        /// </summary>
        /// <param name="platformElement">The XElement containing the platform details.</param>
        /// <param name="destPath">The destination path where the files will be saved.</param>
        private static void DownloadPlatform(XElement platformElement, string destPath)
        {
            foreach (XElement releaseElement in platformElement.Elements())
            {

                // Check if the element is a "Releases" element with the name "OESIS Local V4"
                if (releaseElement.Name == "Releases" && getAttribute(releaseElement, "Name") == "OESIS Local V4")
                {
                    // Get the latest release element and download its files
                    XElement latestReleaseElement = releaseElement.Element("LatestRelease");
                    DownloadReleaseFiles(latestReleaseElement, destPath);
                }
            }
        }

        /// <summary>
        /// Downloads the release files for the SDK specified in the XML description to the SDK directory.
        /// </summary>
        /// <param name="sdkDir">The directory where the SDK files will be saved.</param>
        /// <param name="xmlDescription">The XML string describing the SDK releases.</param>
        private static void DownloadReleases(string sdkDir, string xmlDescription)
        {
            // Parse the XML description into an XDocument
            XDocument doc = XDocument.Parse(xmlDescription);
            IEnumerable<XElement> firstElements = doc.Elements();

            foreach (XElement element in firstElements)
            {
                if (element.Name == "OesisPackageLinks")
                {
                    foreach (XElement platformElement in element.Elements())
                    {
                        // Get the platform name from the platform element
                        string platformName = getAttribute(platformElement, "Name");

                        //
                        // Right now only download Windows Platform
                        //
                        if (platformName == "Windows")
                        {
                            DownloadPlatform(platformElement, sdkDir);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Downloads all SDK files to the specified directory.
        /// </summary>
        /// <param name="sdkDir">The directory where the SDK files will be saved.</param>
        public static void DownloadAllSDKFiles(string sdkDir)
        {
            string oesisFilePath = Path.Combine(sdkDir, "OESIS-Descriptior.xml");
            HttpClientUtils.DownloadFileSynchronous(VAPMSettings.getSDKURL(), oesisFilePath);

            if (File.Exists(oesisFilePath))
            {
                // Read the content of the XML file and download the releases described in it
                string xmlString = File.ReadAllText(oesisFilePath);
                DownloadReleases(sdkDir, xmlString);
            }
        }


        public static async Task<string> getLatestReleaseDateAsync(string sdkDir)
        {            
            string oesisFilePath = Path.Combine(sdkDir, "OESIS-Descriptor.xml");           

            try
            {       
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(VAPMSettings.getSDKURL());
                    response.EnsureSuccessStatusCode();

                    await using (var fs = new FileStream(oesisFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during download: {ex.Message}");
                return null;
            }

            string xmlDescription = File.ReadAllText(oesisFilePath);
                        
            XDocument doc = XDocument.Parse(xmlDescription);
            
            XElement rootElement = doc.Root;
            
            foreach (XElement platformElement in rootElement.Elements("Platform"))
            {                
                if (platformElement.Attribute("Name")?.Value == "Windows")
                {
                    foreach (XElement releases in platformElement.Elements("Releases"))
                    {                        
                        if (releases.Attribute("Name")?.Value == "OESIS Local V4")
                        {                            
                            XElement latestRelease = releases.Element("LatestRelease");                            
                            if (latestRelease != null)
                            {                               
                                XAttribute dateAttribute = latestRelease.Attribute("Date");                                
                                if (dateAttribute != null)
                                {
                                    string dateValue = dateAttribute.Value;                                    
                                    return dateValue;
                                }
                                else
                                {
                                    Debug.WriteLine("Date attribute not found.");
                                }
                            }
                            else
                            {
                                Debug.WriteLine("LatestRelease element not found.");
                            }
                        }
                    }
                }
            }

            Debug.WriteLine("No matching elements found.");
            return string.Empty;
    }

}   
}
