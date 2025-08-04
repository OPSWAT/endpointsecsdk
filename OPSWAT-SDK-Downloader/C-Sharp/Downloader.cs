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
    public class Downloader
    {
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

                        // Only Download the static release files
                        if (!fileName.Contains("Adapter") && !fileName.Contains("offline") && (fileName.EndsWith(".zip") || fileName.EndsWith(".tar")))
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

                        if (platformName == "Windows" || platformName == "Linux" || platformName == "Mac")
                        {
                            Console.WriteLine("Downloading Platform: " + platformName);

                            platformName = platformName.ToLower();
                            string platformDir = Path.Combine(sdkDir, platformName);
                            Directory.CreateDirectory(platformDir);

                            DownloadPlatform(platformElement, platformDir);
                        }
                    }
                }
            }
        }

        private static string GetSDKUrl()
        {
            string result = Util.GetTokenDownloadURL(Constants.DESCRIPTOR_FILE);
            return result;
        }

        private static void DownloadArchives(string archiveRoot)
        {
            string oesisFilePath = Path.Combine(archiveRoot, Constants.DESCRIPTOR_FILE);

            HttpClientUtils.DownloadFileSynchronous(GetSDKUrl(), oesisFilePath);

            if (File.Exists(oesisFilePath))
            {
                string xmlString = File.ReadAllText(oesisFilePath);
                DownloadReleases(archiveRoot, xmlString);
            }
        }


        private static void DownloadDynamicFile(string archiveRoot, string fileName)
        {
            string filePath = Path.Combine(archiveRoot, fileName);
            string fileURL = Util.GetTokenDownloadURL(fileName);
            HttpClientUtils.DownloadFileSynchronous(fileURL, filePath);
        }


        public static void Download(string sdkRoot)
        {
            string archivePath = Util.GetArchivesPath(sdkRoot);

            string firstFile = Util.FindFirstFile(archivePath);
            if (!Util.IsFileUpdated(firstFile))
            {
                Console.WriteLine("Downloading SDK to " + archivePath);
                Util.CreateCleanDir(archivePath);
                DownloadArchives(archivePath);
                Console.WriteLine("SDKDownloader Complete");
            }


            //
            // Now Download the dynamic files
            //
            DownloadDynamicFile(archivePath, Constants.COMPLIANCE_FILE);
            DownloadDynamicFile(archivePath, Constants.CATALOG_FILE);
        }

    }
}
