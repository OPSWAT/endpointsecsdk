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
        private static string DOWNLOAD_TOKEN = "eyJhbGciOiJIUzUxMiJ9.eyJzdWIiOiJjdW9uZ2xlQWNjb3VudCIsImlhdCI6MTU4MjI3OTMzMn0.SI9tOtEHtytBoABciMynqtKCUUx1ZXIbq7O7UR-BJIPPcKoxEcN2vOMBS0zT4FE9ikTALHhSGCd3tu8yBOs3rQ";
        private static string VCR_URL = "https://vcr.opswat.com/gw/file/download/%file%?type=1&token=%token%";
        private static string SDK_INDEX_URL = "https://software.opswat.com/OESIS_V4/OesisPackageLinks.xml";

        //public static string THIRD_PARTY_VULNERABILITY_DB   = "vmod.dat";
        public static string THIRD_PARTY_VULNERABILITY_DB = "v2mod.dat";
        public static string THIRD_PARTY_PATCH_DB           = "patch.dat";
        public static string WINDOWS_VULNERABILITY_DB       = "wiv-lite.dat";
        public static string WINDOWS_PATCH_DB               = "wuo.dat";
        public static string PATCH_CHECKSUMS_DB             = "ap_checksum.dat";


        public static string GetTokenDownloadURL(string fileName)
        {
            string result = VCR_URL.Replace("%token%", DOWNLOAD_TOKEN);
            result = result.Replace("%file%", fileName);
            return result;
        }

        public static string GetSDKURL()
        {
            return SDK_INDEX_URL;
        }

        public static string GetCatalogURL()
        {
            return GetTokenDownloadURL("analog.zip");    
        }

        public static string GetStatusURL()
        {
            return GetTokenDownloadURL("patch_status.json");
        }


        public static string GetLocalCatalogDir()
        {
            //
            // First delete the SDK directory if it exists
            //
            string sdkDir = Path.Combine(Directory.GetCurrentDirectory(), "catalog");
            return sdkDir;
        }




    }
}
