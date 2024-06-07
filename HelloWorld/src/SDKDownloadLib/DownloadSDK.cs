using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SDKDownloader
{
    public class DownloadSDK
    {
        private static string SDK_INDEX_URL = "https://software.opswat.com/OESIS_V4/OesisPackageLinks.xml";

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
                            DownloadPlatform(platformElement, sdkDir);
                        }
                    }
                }
            }
        }

        public static void DownloadAllSDKFiles(string sdkDir)
        {
            string oesisFilePath = Path.Combine(sdkDir, "OESIS-Descriptior.xml");
            HttpClientUtils.DownloadFileSynchronous(SDK_INDEX_URL, oesisFilePath);

            if (File.Exists(oesisFilePath))
            {
                string xmlString = File.ReadAllText(oesisFilePath);
                DownloadReleases(sdkDir, xmlString);
            }
        }

    }
}
