///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using SDKDownloader;
using System;
using System.IO;

namespace SDKDownloadLib
{
    public class ExtractorCatalog
    {
        private static void CopyExtractedFile(string tempCatalogDir, string clientDBDir, string folder, string filename)
        {
            string rootFile = Path.Combine(tempCatalogDir, folder);
            rootFile = Path.Combine(rootFile, filename);
            string destFile = Path.Combine(clientDBDir, filename);
            File.Copy(rootFile, destFile, true);
        }

        private static void CopyWindowsClientFiles(string tempCatalogDir, string clientDBDir)
        {
            Util.MakeDirs(clientDBDir);
            CopyExtractedFile(tempCatalogDir, clientDBDir, "analog/client", "v2mod.dat");
            CopyExtractedFile(tempCatalogDir, clientDBDir, "analog/client", "ap_checksum.dat");
            CopyExtractedFile(tempCatalogDir, clientDBDir, "analog/client", "patch.dat");
            CopyExtractedFile(tempCatalogDir, clientDBDir, "analog/client", "wiv-lite.dat");
            CopyExtractedFile(tempCatalogDir, clientDBDir, "analog/client", "wuo.dat");
        }

        //
        // Platforms are as follows
        // 1 - Windows
        // 2 - Mac
        // 3 - Linux
        public static void DownloadAndCopy(string rootDir)
        {
            string tempArchiveDir = Util.GetCleanTempDir("OESIS-CATALOG-ARCHIVE");

            DownloadCatalog.Download(tempArchiveDir);


            Console.WriteLine("Extracting Vulnerability and Patch Catalog");
            string tempCatalogDir = Util.GetCleanTempDir("OESIS-CATALOG");
            string archivePath = Path.Combine(tempArchiveDir, "analog.zip");

            Util.CreateCleanDir(tempCatalogDir);
            System.IO.Compression.ZipFile.ExtractToDirectory(archivePath, tempCatalogDir);
            string catalogDir = Path.Combine(rootDir, "vapm");

            //
            // Either cleanup the directory or create a new one here
            //
            Console.WriteLine("Copying Windows files for to project Vulnerability and Patch Catalog");
            CopyWindowsClientFiles(tempCatalogDir, catalogDir);

            //
            // Cleanup the temp paths
            //
            Directory.Delete(tempArchiveDir, true);
            Directory.Delete(tempCatalogDir, true);
        }
    }
}
