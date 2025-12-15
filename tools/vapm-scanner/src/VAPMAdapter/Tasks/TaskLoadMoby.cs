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
            string mobyRoot = VAPMSettings.GetLocalCatalogDir();
            mobyRoot = Path.Combine(mobyRoot, "analog/server");
            moby.Load(mobyRoot);
            result = moby.GetList();
            return result;
        }
    }
}
