///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OESIS Framework
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace SDKDownloader
{
    public class Extractor
    {
        public static void ExtractArchives(string archiveDirectory, string extractDirectory)
        {
            if (archiveDirectory != null && Directory.Exists(archiveDirectory))
            {
                string[] dirList = Directory.GetFiles(archiveDirectory);

                // Extract just the first 2 files in the directory  
                foreach (string archive in dirList)
                {
                    if (archive.EndsWith(".zip") || archive.EndsWith(".tar"))
                    {
                        Util.ExtractFile(archive, extractDirectory);
                    }
                }
            }
            else
            {
                Logger.Log("Unable to extract because directory is null or non existent");
            }
        }



        private static void ExtractPlatform(string archiveRoot, string extractDir, string platform)
        {
            Console.WriteLine("Extracting Platform: " + platform);
            string platformDir = Path.Combine(archiveRoot, platform);
            string platformExtractDir = Path.Combine(extractDir, platform);
            ExtractArchives(platformDir, platformExtractDir);
        }

        private static void ExtractDyanmicFile(string archiveRoot, string extractDir, string dynamicFile)
        {
            string archiveFile = Path.Combine(archiveRoot, dynamicFile);
            if (File.Exists(archiveFile))
            {
                Console.WriteLine("Extracting Dynamic File: " + dynamicFile);
                Util.ExtractFile(archiveFile, extractDir);
            }
            else
            {
                Console.WriteLine("Dynamic file not found: " + dynamicFile);
            }
        }


        public static void Extract(string sdkRoot)
        {
            string extractRoot = Util.GetExtractPath(sdkRoot);
            string archivePath = Util.GetArchivesPath(sdkRoot);

            Util.CreateCleanDir(extractRoot);

            ExtractDyanmicFile(archivePath, extractRoot, Constants.CATALOG_FILE);
            ExtractDyanmicFile(archivePath, extractRoot, Constants.COMPLIANCE_FILE);

            ExtractPlatform(archivePath, extractRoot, "linux");
            ExtractPlatform(archivePath, extractRoot, "windows");
            ExtractPlatform(archivePath, extractRoot, "mac");
        }
    }
}
