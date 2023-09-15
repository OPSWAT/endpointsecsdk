///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using VAPMAdapater.Updates;
using VAPMAdapater;
using System.IO;

namespace VAPMAdapter.Updates
{
    public class UpdateDBFiles
    {
        private static void DownloadDBFile(string destPath, string fileName)
        {
            string downloadURL = VAPMSettings.getTokenDownloadURL(fileName);
            string newFilePath = Path.Combine(destPath, fileName);

            if(File.Exists(destPath))
            {
                File.Delete(destPath);
            }

            HttpClientUtils.DownloadFileSynchronous(downloadURL, newFilePath);
        }

        public static void DownloadFiles()
        {
            string destPath = Directory.GetCurrentDirectory();

            DownloadDBFile(destPath, "vmod.dat");
            DownloadDBFile(destPath, "patch.dat");
            DownloadDBFile(destPath, "wuo.dat");
            DownloadDBFile(destPath, "wiv-lite.dat");
            DownloadDBFile(destPath, "ap_checksum.dat");
        }

    }
}
