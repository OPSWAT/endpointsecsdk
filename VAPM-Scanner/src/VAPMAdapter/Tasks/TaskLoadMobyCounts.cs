using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPMAdapater;
using VAPMAdapter.Moby.POCO;

namespace VAPMAdapter.Tasks
{
    public class TaskLoadMobyCounts
    {
        public static MobyTotalCounts LoadCounts()
        {
            Moby.Moby moby = new Moby.Moby();
            string catalogRoot = VAPMSettings.getLocalCatalogDir();
            catalogRoot = Path.Combine(catalogRoot, "analog/server");

            if (moby.Load(catalogRoot))
            {
                return moby.LoadCounts();
            }
            else
            {
                // Handle case where loading fails, return null or default counts
                return null;
            }
        }
    }
}
