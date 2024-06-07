using System.IO;

namespace SDKDownloader
{
    public class SDKExtractor
    {
        private static string GetCleanTempDir(string dirName)
        {
            string tempPath;

            tempPath = Path.Combine(Path.GetTempPath(), dirName);
            Util.CreateCleanDir(tempPath);

            return tempPath;
        }

        public static void ExtractArchives(string archiveDirectory, string extractDirectory)
        {
            if(archiveDirectory != null && Directory.Exists(archiveDirectory))
            {
                string[] dirList = Directory.GetFiles(archiveDirectory);

                // Extract just the first 2 files in the directory
                foreach (string archive in dirList)
                {
                    if (archive.EndsWith(".zip"))
                    {
                        string extractionPath = extractDirectory;
                        if (archive.Contains("resource"))
                        {
                            extractionPath = Path.Combine(extractionPath, "resource");
                        }
                        else
                        {
                            extractionPath = Path.Combine(extractionPath, "sdk");
                        }

                        Util.CreateCleanDir(extractionPath);
                        System.IO.Compression.ZipFile.ExtractToDirectory(archive, extractionPath);
                    }
                }
            }
            else
            {
                Logger.Log("Unable to extract because directory is null or non existent");
            }
        }

        private static void CopyExtractedFile(string libDir, string sdkDir, string folder, string filename)
        {
            string rootFile = Path.Combine(sdkDir, "sdk");
            rootFile = Path.Combine(rootFile, folder);
            rootFile = Path.Combine(rootFile, "x64/release");
            rootFile = Path.Combine(rootFile, filename);

            string destFile = Path.Combine(libDir, filename);


            File.Copy(rootFile, destFile, true);
        }

        private static void CopySDKFiles(string sdkDir,string libDir)
        {
            CopyExtractedFile(libDir, sdkDir, "bin/detection", "libwaaddon.dll");
            CopyExtractedFile(libDir, sdkDir, "bin/detection", "libwaapi.dll");
            CopyExtractedFile(libDir, sdkDir, "bin/detection", "libwaheap.dll");
            CopyExtractedFile(libDir, sdkDir, "bin/detection", "libwautils.dll");
            CopyExtractedFile(libDir, sdkDir, "bin/manageability", "libwalocal.dll");
            CopyExtractedFile(libDir, sdkDir, "bin/manageability", "wa_3rd_party_host_32.exe");
            CopyExtractedFile(libDir, sdkDir, "bin/manageability", "wa_3rd_party_host_64.exe");
            CopyExtractedFile(libDir, sdkDir, "bin/vulnerability", "libwavmodapi.dll");
        }

        private static void CopyResourceFiles(string sdkDir, string libDir)
        {
            string destFile = Path.Combine(libDir, "libwaresource.dll");
            string sourceFile = Path.Combine(sdkDir, "resource/bin/libwaresource.dll");
            File.Copy(sourceFile, destFile, true);
        }

        public static void CopyAllLibFiles(string extractDir, string libDir)
        {
            CopySDKFiles(extractDir, libDir);
            CopyResourceFiles(extractDir, libDir);
        }


        public static void DownloadAndCopy(string libDir)
        {
            string tempArchiveDir = GetCleanTempDir("OESIS-ARCHIVE");
            DownloadSDK.DownloadAllSDKFiles(tempArchiveDir);


            string tempSDKDir = GetCleanTempDir("OESIS-SDK");
            SDKExtractor.ExtractArchives(tempArchiveDir, tempSDKDir);


            SDKExtractor.CopyAllLibFiles(tempSDKDir, libDir);

            //
            // Cleanup the temp paths
            //
            Directory.Delete(tempArchiveDir,true);
            Directory.Delete(tempSDKDir,true);
        }
    }
}
