///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using VAPMAdapater;

namespace VAPMAdapter.Updates
{

    /// <summary>
    /// Provides methods to update the local catalog by downloading and extracting the latest version.
    /// </summary>
    public class UpdateCatalog
    {
        /// <summary>
        /// Updates the local catalog by downloading the latest catalog zip file and extracting its contents.
        /// The catalog is only updated if it is older than one day.
        /// </summary>
        public static void Update()
        {
            Update(false);
        }

        /// <summary>
        /// Updates the local catalog. Normally the catalog is only re-downloaded if it is older than
        /// one day; pass <paramref name="forceRefresh"/> = true to always download a fresh copy
        /// (used by "Update DB" so it never serves a stale or wrong-channel cached catalog).
        /// </summary>
        /// <param name="forceRefresh">When true, ignore the one-day cache and re-download.</param>
        public static void Update(bool forceRefresh)
        {
            string catalogDir = VAPMSettings.GetLocalCatalogDir();

            // Use the cached catalog if it is fresh enough (unless a refresh is forced).
            if (Directory.Exists(catalogDir) && !forceRefresh &&
                Directory.GetCreationTime(catalogDir).Add(TimeSpan.FromDays(1)) > DateTime.Now)
            {
                return;
            }

            // Download and extract into a temporary directory first, then swap it in only after it
            // succeeds. A failed download (offline, bad token, VCR error) then never destroys a
            // previously-working catalog.
            string tempDir = catalogDir + ".new";
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            Directory.CreateDirectory(tempDir);

            try
            {
                string analogFile = Path.Combine(tempDir, "analog.zip");
                DownloadCatalog.Download(analogFile);
                ExtractUtils.ExtractZipFiles(tempDir);
            }
            catch
            {
                // Leave the existing catalog untouched on any failure.
                try { Directory.Delete(tempDir, true); } catch { /* best-effort cleanup */ }
                throw;
            }

            // Swap the freshly downloaded catalog in for the old one.
            if (Directory.Exists(catalogDir))
            {
                Directory.Delete(catalogDir, true);
            }
            Directory.Move(tempDir, catalogDir);
        }

    }
}
