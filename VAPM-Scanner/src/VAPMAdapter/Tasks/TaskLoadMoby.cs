using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPMAdapater;
using VAPMAdapter.Catalog.POCO;
using VAPMAdapter.Moby.POCO;

namespace VAPMAdapter.Tasks
{
    public class TaskLoadMoby
    {
        public static List<MobyProduct> Load()
        {
            List<MobyProduct> result = new List<MobyProduct>();
            Moby.Moby moby = new Moby.Moby();
            string catalogRoot = VAPMSettings.getLocalCatalogDir();
            catalogRoot = Path.Combine(catalogRoot, "analog/server");
            if (moby.Load(catalogRoot))
            {
                result = moby.GetList();
                moby.LoadCounts(); // Load the counts after loading the product list
            }

            return result;
        }
    }
}
