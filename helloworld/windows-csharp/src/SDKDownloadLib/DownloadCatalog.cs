///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using SDKDownloader;
using System;
using System.IO;

namespace SDKDownloadLib
{
    public class DownloadCatalog
    {

        //
        // Please email me for the values for %download_token% and %SDK-URL%.   You need these values for the auto download to work
        // Email: christopher.seiler@opswat.com
        //
        private static string DOWNLOAD_TOKEN = "";
        private static string VCR_URL = "https://vcr.opswat.com/gw/file/download/%file%?type=1&token=%token%";
        

        //public static string THIRD_PARTY_VULNERABILITY_DB   = "vmod.dat";
        public static string THIRD_PARTY_VULNERABILITY_DB = "v2mod.dat";
        public static string THIRD_PARTY_PATCH_DB = "patch.dat";
        public static string WINDOWS_VULNERABILITY_DB = "wiv-lite.dat";
        public static string WINDOWS_PATCH_DB = "wuo.dat";
        public static string PATCH_CHECKSUMS_DB = "ap_checksum.dat";


        public static string GetTokenDownloadURL(string fileName)
        {
            string DOWNLOAD_TOKEN = File.ReadAllText("download_token.txt");
            string result = VCR_URL.Replace("%token%", DOWNLOAD_TOKEN);
            result = result.Replace("%file%", fileName);
            return result;
        }

     

        public static string GetCatalogURL()
        {
            return GetTokenDownloadURL("analog.zip");
        }

        public static void Download(string sdkDir)
        {
            string catalogFilePath = Path.Combine(sdkDir, "analog.zip");
            HttpClientUtils.DownloadFileSynchronous(GetCatalogURL(), catalogFilePath);
        }

    }
}
