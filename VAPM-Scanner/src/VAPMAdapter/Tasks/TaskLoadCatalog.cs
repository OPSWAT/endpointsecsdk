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
    public class TaskLoadCatalog
    {

        public static List<CatalogProduct> Load()
        {
            List<CatalogProduct> result = new List<CatalogProduct>();
            Catalog.Catalog catalog = new Catalog.Catalog();
            UpdateCatalog.Update();

            string catalogRoot = VAPMSettings.getLocalCatalogDir();
            catalogRoot = Path.Combine(catalogRoot, "analog/server");
            catalog.Load(catalogRoot);

            catalog.PopulateSignatureVulnerability();
            result = catalog.GetProductList();

            return result;
        }
    }
}
