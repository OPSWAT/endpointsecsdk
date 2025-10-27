///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace SDKDownloader
{
    public class DownloadSDK
    {
        private static string SDK_INDEX_URL = "https://vcr.opswat.com/gw/file/download/OesisPackageLinks.xml?type=1&token=%token%";

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


        private static void DownloadReleaseFiles(XElement releaseELement, string destPath)
        {
            if (releaseELement != null)
            {
                foreach (XElement packageElement in releaseELement.Elements())
                {
                    if (packageElement.Name == "Package")
                    {
                        string fileUrl = getAttribute(packageElement, "Link");
                        string sha256Hash = getAttribute(packageElement, "sha256");

                        string fileName = Path.GetFileName(fileUrl);
                        string localFilePath = Path.Combine(destPath, fileName);

                        if (!fileName.Contains("Adapter") && fileName.EndsWith(".zip"))
                        {
                            HttpClientUtils.DownloadValidFile(fileUrl, localFilePath, sha256Hash);
                        }
                    }
                }
            }
        }



        private static void DownloadPlatform(XElement platformElement, string destPath)
        {
            foreach (XElement releaseElement in platformElement.Elements())
            {
                if (releaseElement.Name == "Releases" && getAttribute(releaseElement, "Name") == "OESIS Local V4")
                {
                    XElement latestReleaseElement = releaseElement.Element("LatestRelease");
                    DownloadReleaseFiles(latestReleaseElement, destPath);
                }
            }
        }


        private static void DownloadReleases(string sdkDir, string xmlDescription)
        {
            XDocument doc = XDocument.Parse(xmlDescription);
            IEnumerable<XElement> firstElements = doc.Elements();

            foreach (XElement element in firstElements)
            {
                if (element.Name == "OesisPackageLinks")
                {
                    foreach (XElement platformElement in element.Elements())
                    {
                        string platformName = getAttribute(platformElement, "Name");

                        //
                        // Right now only download Windows Platform
                        //
                        if (platformName == "Windows")
                        {
                            Console.WriteLine("Downloading MetaDefender Endpoint Security SDK for Windows");
                            DownloadPlatform(platformElement, sdkDir);
                        }
                    }
                }
            }
        }

        private static string GetDownloadToken()
        {
            string sdk_token_file = "download_token.txt";
            if (!File.Exists(sdk_token_file))
            {
                throw new Exception("Make sure there is a download token file available in the running directory: " + Directory.GetCurrentDirectory());
            }

            string downloadToken = File.ReadAllText(sdk_token_file);
            return downloadToken;
        }

        private static string GetSDKUrl()
        {
            string token = GetDownloadToken();
            string result = SDK_INDEX_URL.Replace("%token%", token);

            return result;
        }

        public static void Download(string destDir)
        {
            Util.MakeDirs(destDir);

            string oesisFilePath = Path.Combine(destDir, "OESIS-Descriptior.xml");

            Console.WriteLine("Downloading MetaDefender Endpoint Security SDK Index");
            HttpClientUtils.DownloadFileSynchronous(GetSDKUrl(), oesisFilePath);

            if (File.Exists(oesisFilePath))
            {
                string xmlString = File.ReadAllText(oesisFilePath);
                DownloadReleases(destDir, xmlString);
            }
        }



    }
}
