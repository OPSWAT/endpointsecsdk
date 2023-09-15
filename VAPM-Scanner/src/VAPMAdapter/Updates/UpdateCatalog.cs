using System.IO;
using VAPMAdapater;

namespace VAPMAdapter.Updates
{
    public class UpdateCatalog
    {

        public static void Update()
        {
            string catalogDir = VAPMSettings.getLocalCatalogDir();

            if (Directory.Exists(catalogDir))
            {
                Directory.Delete(catalogDir, true);
            }
            Directory.CreateDirectory(catalogDir);


            string analogFile = Path.Combine(catalogDir, "analog.zip");
            DownloadCatalog.Download(analogFile);
            ExtractUtils.ExtractZipFiles(catalogDir);
        }

    }
}
