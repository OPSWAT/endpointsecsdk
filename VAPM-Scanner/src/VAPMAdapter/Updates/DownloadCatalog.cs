using VAPMAdapater.Updates;
using VAPMAdapater;

namespace VAPMAdapter.Updates
{
    internal class DownloadCatalog
    {

        public static void Download(string catalogFile)
        {
            HttpClientUtils.DownloadFileSynchronous(VAPMSettings.getCatalogURL(), catalogFile);
        }

    }
}
