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
            string catalogDir = VAPMSettings.getLocalCatalogDir();


            if (Directory.Exists(catalogDir))
            {
                // Only update the catalog once a day
                if (Directory.GetCreationTime(catalogDir).Add(TimeSpan.FromDays(1)) > DateTime.Now)
                {
                    return;
                }

                Directory.Delete(catalogDir, true);
            }
            Directory.CreateDirectory(catalogDir);

            // Define the path for the downloaded catalog zip file
            string analogFile = Path.Combine(catalogDir, "analog.zip");

            // Download the latest catalog zip file
            DownloadCatalog.Download(analogFile);

            // Extract all zip files in the catalog directory
            ExtractUtils.ExtractZipFiles(catalogDir);
        }

    }
}
