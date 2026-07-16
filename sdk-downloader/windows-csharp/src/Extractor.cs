///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OESIS Framework
///  
///  Created by Chris Seiler
///  OPSWAT OEM Field CTO
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

                bool found = false;
                foreach (string archive in dirList)
                {
                    if (archive.EndsWith(".zip") || archive.EndsWith(".tar"))
                    {
                        found = true;
                        Console.WriteLine("  Extracting new data: " + Path.GetFileName(archive));
                        Util.ExtractFile(archive, extractDirectory);
                    }
                }
                if (!found)
                {
                    Console.WriteLine("  No new archives to extract in " + archiveDirectory);
                }
            }
            else
            {
                Console.WriteLine("  Nothing downloaded for " + Path.GetFileName(archiveDirectory) +
                                  " this run - skipping (existing extracted data kept).");
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
                Console.WriteLine("Extracting new data: " + dynamicFile);
                Util.ExtractFile(archiveFile, extractDir);
            }
            else
            {
                Console.WriteLine("Skipping " + dynamicFile + " - not downloaded this run " +
                                  "(unchanged; existing extracted data kept).");
            }
        }


        public static void Extract(string sdkRoot)
        {
            // The extract directory is intentionally NOT cleaned: unchanged data from prior runs
            // is preserved, and only the archives downloaded THIS run (the only files present in
            // the archive directory) are (re)extracted. This is the "only extract new data" behavior.
            string extractRoot = Util.GetExtractPath(sdkRoot);
            string archivePath = Util.GetArchivesPath(sdkRoot);

            ExtractDyanmicFile(archivePath, extractRoot, Constants.CATALOG_FILE);
            ExtractDyanmicFile(archivePath, extractRoot, Constants.COMPLIANCE_FILE);

            ExtractPlatform(archivePath, extractRoot, "linux");
            ExtractPlatform(archivePath, extractRoot, "windows");
            ExtractPlatform(archivePath, extractRoot, "mac");
        }
    }
}
