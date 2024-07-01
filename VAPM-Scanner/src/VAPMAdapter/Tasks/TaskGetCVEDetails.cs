///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        private static Catalog.Catalog _catalog;
        private static readonly object _lock = new object();

        /// <summary>
        /// Retrieves a list of CVE details for the given list of vulnerability associations.
        /// </summary>
        /// <param name="vulAssociationList">The list of vulnerability associations to retrieve CVE details for.</param>
        /// <returns>A list of CVE details corresponding to the provided vulnerability associations.</returns>
        public static List<CVEDetail> GetCveDetailList(List<CatalogVulnerabilityAssociation> vulAssociationList)
        {
            // Lazy initialization of the catalog
            if (_catalog == null)
            {
                lock (_lock)
                {
                    if (_catalog == null)
                    {
                        _catalog = new Catalog.Catalog();
                        string catalogRoot = VAPMSettings.getLocalCatalogDir();
                        catalogRoot = Path.Combine(catalogRoot, "analog/server");
                        _catalog.Load(catalogRoot);
                    }
                }
            }

            // Retrieve CVE details list using the loaded catalog and the provided vulnerability associations
            return _catalog.GetCVEDetailsList(vulAssociationList);
        }
    }

}
