
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


            result = catalog.GetProductList();
            foreach(CatalogProduct product in result)
            {
                foreach(CatalogSignature signature in product.SigList)
                {
                    signature.CVECount = catalog.GetVulnerabilityAssociationFromSignatureId(product.Id).Count;
                }
            }

            return result;
        }
    }
}
