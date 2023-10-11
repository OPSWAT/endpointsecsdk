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
    internal class DownloadCatalog
    {

        public static void Download(string catalogFile)
        {
            HttpClientUtils.DownloadFileSynchronous(VAPMSettings.getCatalogURL(), catalogFile);
        }

    }
}
