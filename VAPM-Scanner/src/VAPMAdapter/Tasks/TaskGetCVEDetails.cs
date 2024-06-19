///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using VAPMAdapater;
using VAPMAdapter.Catalog.POCO;
using VAPMAdapter.OESIS.POCO;
using VAPMAdapter.Updates;

namespace VAPMAdapter.Tasks
{
    /// <summary>
    /// Represents a class to retrieve CVE details based on a list of vulnerability associations.
    /// </summary>
    public class TaskGetCVEDetails
    {

        /// <summary>
        /// Retrieves a list of CVE details for the given list of vulnerability associations.
        /// </summary>
        /// <param name="vulAssociationList">The list of vulnerability associations to retrieve CVE details for.</param>
        /// <returns>A list of CVE details corresponding to the provided vulnerability associations.</returns>
        public static List<CVEDetail> GetCveDetailList(List<CatalogVulnerabilityAssociation> vulAssociationList)
        {
            //empty list to store CVE details and an instanceof the catalog class
            List<CVEDetail> result = new List<CVEDetail>();
            Catalog.Catalog catalog = new Catalog.Catalog();

            //retrieve the local catalog directory from settings
            string catalogRoot = VAPMSettings.getLocalCatalogDir();
            catalogRoot = Path.Combine(catalogRoot, "analog/server");

            //load catalog root from the specified directory
            catalog.Load(catalogRoot);

            //return CVE details list using the loaded catalog and the provided vulnerability associations
            result = catalog.GetCVEDetailsList(vulAssociationList);

            return result;
        }
    }
}
