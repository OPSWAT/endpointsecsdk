using SDKDownloader;
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
            CopyExtractedFile(tempCatalogDir, clientDBDir, "analog/client", "v2mod.dat");
            CopyExtractedFile(tempCatalogDir, clientDBDir, "analog/client", "ap_checksum.dat");
            CopyExtractedFile(tempCatalogDir, clientDBDir, "analog/client", "patch.dat");
            CopyExtractedFile(tempCatalogDir, clientDBDir, "analog/client", "wiv-lite.dat");
            CopyExtractedFile(tempCatalogDir, clientDBDir, "analog/client", "wuo.dat");
        }



        public static void DownloadAndCopy(string catalogDir)
        {
            string tempArchiveDir = Util.GetCleanTempDir("OESIS-CATALOG-ARCHIVE");
            DownloadCatalog.Download(tempArchiveDir);

            string tempCatalogDir = Util.GetCleanTempDir("OESIS-CATALOG");
            string archivePath = Path.Combine(tempArchiveDir, "analog.zip");
            System.IO.Compression.ZipFile.ExtractToDirectory(archivePath, tempCatalogDir);

            CopyWindowsClientFiles(tempCatalogDir, catalogDir);

            //
            // Cleanup the temp paths
            //
            Directory.Delete(tempArchiveDir, true);
            Directory.Delete(tempCatalogDir, true);
        }
    }
}
