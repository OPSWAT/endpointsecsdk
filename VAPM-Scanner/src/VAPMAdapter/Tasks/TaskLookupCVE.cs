///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using VAPMAdapater;


namespace VAPMAdapter.Tasks
{
    public class TaskLookupCVE
    {

        public static string LookupCVE(string CVE)
        {
            string result;
            Catalog.Catalog catalog = new Catalog.Catalog();

            string catalogRoot = VAPMSettings.getLocalCatalogDir();
            catalogRoot = Path.Combine(catalogRoot, "analog/server");
            catalog.Load(catalogRoot);

            result = catalog.GetCVEDetail(CVE);

            return result;
        }
    }
}
