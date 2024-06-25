using SDKDownloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDKDownloadLib
{
    internal class CatalogExtractor
    {
        private static void CopyExtractedFile(string libDir, string sdkDir, string folder, string filename)
        {
            string rootFile = Path.Combine(sdkDir, "sdk");
            rootFile = Path.Combine(rootFile, folder);
            rootFile = Path.Combine(rootFile, "x64/release");
            rootFile = Path.Combine(rootFile, filename);

            string destFile = Path.Combine(libDir, filename);


            File.Copy(rootFile, destFile, true);
        }

        private static void CopyWIndowsClientFiles(string sdkDir, string libDir)
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




        public static void DownloadAndCopy(string catalogDir)
        {
            string tempArchiveDir = Util.GetCleanTempDir("OESIS-CATALOG-ARCHIVE");
            DownloadCatalog.Download(tempArchiveDir);


            string tempSDKDir = Util.GetCleanTempDir("OESIS-CATALOG");
            Util.ExtractArchives(tempArchiveDir, tempSDKDir);


            SDKExtractor.CopyAllLibFiles(tempSDKDir, libDir);

            //
            // Cleanup the temp paths
            //
            Directory.Delete(tempArchiveDir, true);
            Directory.Delete(tempSDKDir, true);
        }
    }
}
