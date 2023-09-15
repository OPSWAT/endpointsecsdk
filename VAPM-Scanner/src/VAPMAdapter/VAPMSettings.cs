///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////


using System.IO;
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////


namespace VAPMAdapater
{
    internal class VAPMSettings
    {
        private static string DOWNLOAD_TOKEN = "eyJhbGciOiJIUzUxMiJ9.eyJzdWIiOiJjdW9uZ2xlQWNjb3VudCIsImlhdCI6MTU4MjI3OTMzMn0.SI9tOtEHtytBoABciMynqtKCUUx1ZXIbq7O7UR-BJIPPcKoxEcN2vOMBS0zT4FE9ikTALHhSGCd3tu8yBOs3rQ";
        private static string VCR_URL = "https://vcr.opswat.com/gw/file/download/%file%?type=1&token=%token%";
        private static string SDK_INDEX_URL = "https://software.opswat.com/OESIS_V4/OesisPackageLinks.xml";

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
