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

namespace VAPMAdapter.Tasks
{
    /// <summary>
    /// Represents a class to load catalog information including products and vulnerabilities.
    /// </summary>
    public class TaskLoadCatalog
    {
        /// <summary>
        /// Loads catalog information including products and vulnerabilities.
        /// </summary>
        /// <returns>A list of catalog products populated with information from the catalog.</returns>
        public static List<CatalogProduct> Load()
        {
            List<CatalogProduct> result = new List<CatalogProduct>();
            Catalog.Catalog catalog = new Catalog.Catalog();

            // This page only READS the already-downloaded catalog cache - it does not download or
            // refresh it. The catalog is downloaded/refreshed by "Update DB" (UpdateCatalog.Update),
            // so loading it here never triggers a network call.
            string catalogRoot = VAPMSettings.GetLocalCatalogDir();
            catalogRoot = Path.Combine(catalogRoot, "analog/server");

            if (!catalog.Load(catalogRoot))
            {
                throw new System.Exception(
                    "No cached catalog was found at " + catalogRoot +
                    ". Click 'Update DB' to download the catalog, then load it here.");
            }

            //populate the sig vulnerabilities into the catalog object
            catalog.PopulateSignatureVulnerability();
            //retrieve the list of produt from the catalog
            result = catalog.GetProductList();

            //returns a list of the catalog products
            return result;
        }
    }
}
