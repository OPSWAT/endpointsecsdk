///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using VAPMAdapater.Updates;
using VAPMAdapater;
using System.IO;
using System;

namespace VAPMAdapter.Updates
{
    /// <summary>
    /// Provides methods to download and update database files.
    /// </summary>
    public class UpdateDBFiles
    {
        /// <summary>
        /// Downloads a database file from a specified URL and saves it to the destination path.
        /// </summary>
        /// <param name="destPath">The directory where the file will be saved.</param>
        /// <param name="fileName">The name of the file to download.</param>
        private static void DownloadDBFile(string destPath, string fileName)
        {
            // Get the download URL for the specified file
            string downloadURL = VAPMSettings.getTokenDownloadURL(fileName);
            string newFilePath = Path.Combine(destPath, fileName);

            if(File.Exists(destPath))
            {
                File.Delete(destPath);
            }

            // Download the file synchronously from the specified URL
            HttpClientUtils.DownloadFileSynchronous(downloadURL, newFilePath);
        }

        /// <summary>
        /// Downloads all necessary database files to the current directory.
        /// </summary>
        public static void DownloadFiles()
        {
            string destPath = Directory.GetCurrentDirectory();

            // Download the specified database files to the destination path
            DownloadDBFile(destPath, VAPMSettings.THIRD_PARTY_VULNERABILITY_DB);
            DownloadDBFile(destPath, VAPMSettings.THIRD_PARTY_PATCH_DB);
            DownloadDBFile(destPath, VAPMSettings.WINDOWS_PATCH_DB);
            DownloadDBFile(destPath, VAPMSettings.WINDOWS_VULNERABILITY_DB);
            DownloadDBFile(destPath, VAPMSettings.PATCH_CHECKSUMS_DB);
        }

        /// <summary>
        /// Checks if the DB is downloaded
        /// </summary>
        /// <returns>True if the DB is downloaded, otherwise false.</returns>
        public static bool doesDBExist()
        {
            bool result = false;
            if (File.Exists("patch.dat"))
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Checks if the DB has been updated in the last 7 days.
        /// </summary>
        /// <returns>True if the DB has been updated in the last 7 days, otherwise false.</returns>
        public static bool isDBUpdated()
        {
            bool result = false;

            if (File.Exists("patch.dat"))
            {
                FileInfo dbFileInfo = new FileInfo("patch.dat");
                // Update the SDK every 7 days
                if (dbFileInfo.LastWriteTime > DateTime.Now.AddDays(-7))
                {
                    result = true;
                }
            }

            return result;
        }
    }
}
