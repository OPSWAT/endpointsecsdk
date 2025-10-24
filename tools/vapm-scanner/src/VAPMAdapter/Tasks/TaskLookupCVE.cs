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
    /// <summary>
    /// Represents a class to lookup CVE details based on a CVE identifier.
    /// </summary>
    public class TaskLookupCVE
    {

        /// <summary>
        /// Looks up CVE details based on the provided CVE identifier.
        /// </summary>
        /// <param name="CVE">The CVE identifier to look up.</param>
        /// <returns>A string containing the CVE details.</returns>
        public static string LookupCVE(string CVE)
        {
            string result;
            Catalog.Catalog catalog = new Catalog.Catalog();

            // Retrieve the local catalog directory from settings.
            string catalogRoot = VAPMSettings.GetLocalCatalogDir();
            catalogRoot = Path.Combine(catalogRoot, "analog/server");

            // Load the catalog from the specified directory
            catalog.Load(catalogRoot);

            // Retrieve CVE details for the specified CVE identifier.
            result = catalog.GetCVEDetail(CVE);

            // Return the string containing the CVE details
            return result;
        }
    }
}
