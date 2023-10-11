///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////
using System.IO;
using VAPMAdapater;

namespace VAPMAdapter.Updates
{
    public class UpdateCatalog
    {

        public static void Update()
        {
            string catalogDir = VAPMSettings.getLocalCatalogDir();

            if (Directory.Exists(catalogDir))
            {
                Directory.Delete(catalogDir, true);
            }
            Directory.CreateDirectory(catalogDir);


            string analogFile = Path.Combine(catalogDir, "analog.zip");
            DownloadCatalog.Download(analogFile);
            ExtractUtils.ExtractZipFiles(catalogDir);
        }

    }
}
