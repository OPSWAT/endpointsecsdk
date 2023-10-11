///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////


using System.IO;

namespace VAPMAdapater
{
    internal class VAPMSettings
    {
        //
        // Please email me for the values for %download_token% and %SDK-URL%.   You need these values for the auto download to work
        // Email: christopher.seiler@opswat.com
        //
        private static string DOWNLOAD_TOKEN = "%download_token%";
        private static string VCR_URL = "https://vcr.opswat.com/gw/file/download/%file%?type=1&token=%token%";
        private static string SDK_INDEX_URL = "https://%SDK-URL%/OesisPackageLinks.xml";

        public static string getTokenDownloadURL(string fileName)
        {
            string result = VCR_URL.Replace("%token%", DOWNLOAD_TOKEN);
            result = result.Replace("%file%", fileName);
            return result;
        }

        public static string getSDKURL()
        {
            return SDK_INDEX_URL;
        }

        public static string getCatalogURL()
        {
            return getTokenDownloadURL("analog.zip");    
        }

        public static string getLocalCatalogDir()
        {
            //
            // First delete the SDK directory if it exists
            //
            string sdkDir = Path.Combine(Directory.GetCurrentDirectory(), "catalog");
            return sdkDir;
        }




    }
}
