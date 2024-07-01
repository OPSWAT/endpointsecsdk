using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPMAdapater;
using VAPMAdapater.Updates;

namespace VAPMAdapter.Updates
{
    public class UpdateMobyFile
    {
        private static void DownloadDBFile(string destPath, string fileName)
        {
            // Get the download URL for the specified file
            string downloadURL = "https://oesis-downloads-portal.s3.amazonaws.com/moby.json";
            string newFilePath = Path.Combine(destPath, fileName);

            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }

            // Download the file synchronously from the specified URL
            HttpClientUtils.DownloadFileSynchronous(downloadURL, newFilePath);
        }
        public static void DownloadMoby()
        {
            string destPath = VAPMSettings.getLocalCatalogDir();
            destPath = Path.Combine(destPath, "analog/server");


            // Download the specified database files to the destination path
            DownloadDBFile(destPath, "moby.json");
        }

        public static bool doesMobyExist()
        {
            // Get the current directory of the executable
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            // Define the relative path to the file from the base directory
            string relativePath = @"catalog\analog\server\moby.json";


            // Combine the base path with the relative path
            string fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));

            // Check if the file exists at the specified path
            return File.Exists(fullPath);
        }
    }
}
