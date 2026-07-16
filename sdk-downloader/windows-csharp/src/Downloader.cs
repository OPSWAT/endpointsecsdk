///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OESIS Framework
///
///  Created by Chris Seiler
///  OPSWAT OEM Field CTO
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

        /// <summary>
        /// HEAD the URL and compare its size to the recorded size. Download only when the file is
        /// new or its size changed; otherwise skip it (so it is neither re-downloaded nor
        /// re-extracted). Records the new size / update date in the status on success.
        /// </summary>
        private static bool DownloadIfChanged(string key, string url, string localPath,
                                              DownloadStatus status, string sha256 = null)
        {
            string lastModified;
            long? remoteSize = HttpClientUtils.RemoteContentLength(url, out lastModified);
            long? prevSize = status.GetSize(key);

            // Fast path: server size confirms unchanged -> skip download AND extract.
            if (remoteSize.HasValue && prevSize.HasValue && remoteSize.Value == prevSize.Value)
            {
                Console.WriteLine("  Unchanged (size " + remoteSize.Value +
                                  " bytes) - skipping download and extract: " + key);
                return false;
            }

            string reason = !prevSize.HasValue ? "new"
                          : !remoteSize.HasValue ? "size unverifiable up front - downloading to verify"
                          : "size changed " + prevSize.Value + " -> " + remoteSize.Value + " bytes";
            Console.WriteLine("  Downloading (" + reason + "): " + key);

            bool ok = HttpClientUtils.DownloadValidFile(url, localPath, sha256);
            if (!(ok && File.Exists(localPath)))
            {
                Console.WriteLine("  Download FAILED: " + key);
                return false;
            }

            long downloadedSize = new FileInfo(localPath).Length;

            // Post-download check: even though we re-downloaded, if the size matches what we
            // already have then the content is unchanged -> remove the archive so the extractor
            // ignores it, and leave the recorded status as-is. This skips a needless re-extract.
            if (prevSize.HasValue && downloadedSize == prevSize.Value)
            {
                Console.WriteLine("  Re-downloaded but size unchanged (" + downloadedSize +
                                  " bytes) - skipping extract: " + key);
                try { File.Delete(localPath); } catch { }
                return false;
            }

            status.Record(key, url, downloadedSize, sha256, lastModified);
            return true;
        }

        private static void DownloadReleaseFiles(XElement releaseELement, string destPath,
                                                 string platform, DownloadStatus status)
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
                            string key = platform + "/" + fileName;
                            string sha = string.IsNullOrEmpty(sha256Hash) ? null : sha256Hash;
                            DownloadIfChanged(key, fileUrl, localFilePath, status, sha);
                        }
                    }
                }
            }
        }



        private static void DownloadPlatform(XElement platformElement, string destPath,
                                             string platform, DownloadStatus status)
        {
            foreach (XElement releaseElement in platformElement.Elements())
            {
                if (releaseElement.Name == "Releases" && getAttribute(releaseElement, "Name") == "OESIS Local V4")
                {
                    XElement latestReleaseElement = releaseElement.Element("LatestRelease");
                    DownloadReleaseFiles(latestReleaseElement, destPath, platform, status);
                }
            }
        }


        private static void DownloadReleases(string sdkDir, string xmlDescription, DownloadStatus status)
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
                            Console.WriteLine("Platform: " + platformName);

                            platformName = platformName.ToLower();
                            string platformDir = Path.Combine(sdkDir, platformName);
                            Directory.CreateDirectory(platformDir);

                            DownloadPlatform(platformElement, platformDir, platformName, status);
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

        private static void DownloadArchives(string archiveRoot, DownloadStatus status)
        {
            // The descriptor is always fetched (small; it provides the current file URLs) and is
            // not extracted, so it is not tracked in the download status.
            string oesisFilePath = Path.Combine(archiveRoot, Constants.DESCRIPTOR_FILE);

            HttpClientUtils.DownloadFileSynchronous(GetSDKUrl(), oesisFilePath);

            if (File.Exists(oesisFilePath))
            {
                string xmlString = File.ReadAllText(oesisFilePath);
                DownloadReleases(archiveRoot, xmlString, status);
            }
        }


        private static void DownloadDynamicFile(string archiveRoot, string fileName, DownloadStatus status)
        {
            Console.WriteLine("Dynamic File: " + fileName);
            string filePath = Path.Combine(archiveRoot, fileName);
            string fileURL = Util.GetTokenDownloadURL(fileName);
            DownloadIfChanged(fileName, fileURL, filePath, status);
        }


        public static void Download(string sdkRoot)
        {
            string archivePath = Util.GetArchivesPath(sdkRoot);
            Console.WriteLine("Downloading SDK to " + archivePath);
            Util.CreateCleanDir(archivePath);

            // Load the persistent status so we can skip files whose size hasn't changed.
            DownloadStatus status = new DownloadStatus(sdkRoot).Load();

            DownloadArchives(archivePath, status);

            //
            // Now Download the dynamic files
            //
            DownloadDynamicFile(archivePath, Constants.COMPLIANCE_FILE, status);
            DownloadDynamicFile(archivePath, Constants.CATALOG_FILE, status);

            status.Save();

            int n = status.DownloadedThisRun.Count;
            if (n > 0)
                Console.WriteLine("Downloaded " + n + " new/changed file(s): " +
                                  string.Join(", ", status.DownloadedThisRun));
            else
                Console.WriteLine("All files are up-to-date - nothing downloaded (nothing to extract).");
        }

    }
}
