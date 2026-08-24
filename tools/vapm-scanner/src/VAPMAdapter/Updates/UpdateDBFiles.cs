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
        /// Copies a single DB file out of the extracted catalog's client folder into the working
        /// directory (where the OESIS engine loads it from), overwriting any existing copy.
        /// </summary>
        /// <param name="clientDir">The extracted catalog client folder (catalog/analog/client).</param>
        /// <param name="destPath">The directory where the file should be placed.</param>
        /// <param name="fileName">The name of the DB file to copy.</param>
        private static void CopyDBFile(string clientDir, string destPath, string fileName)
        {
            string source = Path.Combine(clientDir, fileName);
            if (!File.Exists(source))
            {
                throw new FileNotFoundException(
                    "Expected DB file '" + fileName + "' was not found in the extracted catalog at " +
                    clientDir + ". The configured catalog may not include it.");
            }

            File.Copy(source, Path.Combine(destPath, fileName), true);
        }

        /// <summary>
        /// Makes the DB files current by downloading and extracting the catalog (analog), then
        /// copying the DB files out of it. The catalog is the single source of truth: this keeps
        /// the engine DB files in sync with whichever catalog flavor is configured (production
        /// analog.zip or staging dev-analog.zip) and shares the same download as "Load Catalog".
        /// </summary>
        public static void DownloadFiles()
        {
            string destPath = Directory.GetCurrentDirectory();

            // Download + extract the catalog (no-op if a fresh copy is already cached).
            UpdateCatalog.Update();

            // The DB files ship inside the catalog under analog/client.
            string clientDir = Path.Combine(VAPMSettings.GetLocalCatalogDir(), "analog", "client");

            // Copy the files that used to be downloaded individually out of the extracted catalog.
            CopyDBFile(clientDir, destPath, VAPMSettings.THIRD_PARTY_VULNERABILITY_DB);
            CopyDBFile(clientDir, destPath, VAPMSettings.THIRD_PARTY_PATCH_DB);
            CopyDBFile(clientDir, destPath, VAPMSettings.WINDOWS_PATCH_DB);
            CopyDBFile(clientDir, destPath, VAPMSettings.WINDOWS_VULNERABILITY_DB);
            CopyDBFile(clientDir, destPath, VAPMSettings.PATCH_CHECKSUMS_DB);
            CopyDBFile(clientDir, destPath, VAPMSettings.DRIVER_FIRMWARE_DB); // driver/firmware (incl. BIOS)
        }

        /// <summary>
        /// The driver/firmware (incl. BIOS) DB file name, for display/lookup by the UI.
        /// </summary>
        public static string DriverFirmwareDbFileName
        {
            get { return VAPMSettings.DRIVER_FIRMWARE_DB; }
        }

        /// <summary>
        /// Human-readable catalog channel the DB/catalog is being pulled from, e.g.
        /// "staging (dev-analog.zip)" or "production (analog.zip)".
        /// </summary>
        public static string GetCatalogChannelDescription()
        {
            return VAPMSettings.GetCatalogChannelDescription();
        }

        /// <summary>
        /// Checks if the DB is downloaded
        /// </summary>
        /// <returns>True if the DB is downloaded, otherwise false.</returns>
        public static bool DoesDBExist()
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
        public static bool IsDBUpdated()
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
