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
    }
}
