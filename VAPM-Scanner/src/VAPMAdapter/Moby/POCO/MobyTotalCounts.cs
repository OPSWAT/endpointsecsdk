using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPMAdapter.Moby.POCO
{
    public class MobyTotalCounts
    {
        public MobyPlatformCounts TotalProductsCount { get; set; }
        public MobyPlatformCounts TotalSignaturesCount { get; set; }
        public MobyPlatformCounts CveDetection { get; set; }
        public MobyPlatformCounts SupportAutoPatching { get; set; }
        public MobyPlatformCounts BackgroundPatching { get; set; }
        public MobyPlatformCounts FreshInstallable { get; set; }
        public MobyPlatformCounts ValidationSupported { get; set; }
        public MobyPlatformCounts AppRemover { get; set; }
    }
}
