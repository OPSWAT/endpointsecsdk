
using System.Collections.Generic;
using System.IO;
using VAPMAdapater;
using VAPMAdapter.Catalog.POCO;
using VAPMAdapter.OESIS.POCO;
using VAPMAdapter.Updates;

namespace VAPMAdapter.Tasks
{
    public class TaskGetCVEDetails
    {

        public static List<CVEDetail> GetCveDetailList(List<CatalogVulnerabilityAssociation> vulAssociationList)
        {
            List<CVEDetail> result = new List<CVEDetail>();
            Catalog.Catalog catalog = new Catalog.Catalog();

            string catalogRoot = VAPMSettings.getLocalCatalogDir();
            catalogRoot = Path.Combine(catalogRoot, "analog/server");
            catalog.Load(catalogRoot);

            result = catalog.GetCVEDetailsList(vulAssociationList);

            return result;
        }
    }
}
