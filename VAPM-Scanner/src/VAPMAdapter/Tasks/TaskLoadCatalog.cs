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
using VAPMAdapter.Updates;

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
            UpdateCatalog.Update();

            //retrieve the local catalog directory from settings
            string catalogRoot = VAPMSettings.GetLocalCatalogDir();
            catalogRoot = Path.Combine(catalogRoot, "analog/server");
            catalog.Load(catalogRoot);

            //populate the sig vulnerabilities into the catalog object
            catalog.PopulateSignatureVulnerability();
            //retrieve the list of produt from the catalog
            result = catalog.GetProductList();

            //returns a list of the catalog products
            return result;
        }
    }
}
