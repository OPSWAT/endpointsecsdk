///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////
using VAPMAdapater.Updates;
using VAPMAdapater;

namespace VAPMAdapter.Updates
{
    /// <summary>
    /// Represents a class responsible for downloading a catalog.
    /// </summary>
    internal class DownloadCatalog
    {

        // Calls the method to download the file synchronously from the URL obtained
        // from VAPMSettings.getCatalogURL() and saves it to the specified catalogFile path.
        public static void Download(string catalogFile)
        {
            HttpClientUtils.DownloadFileSynchronous(VAPMSettings.getCatalogURL(), catalogFile);
        }

    }
}
