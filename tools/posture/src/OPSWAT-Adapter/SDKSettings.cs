using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSWAT_Adapter
{
    internal class SDKSettings
    {
        //
        // Please email me for the values for %download_token% and %SDK-URL%.   You need these values for the auto download to work
        // Email: christopher.seiler@opswat.com
        //
        private static string VCR_URL = "https://vcr.opswat.com/gw/file/download/%file%?type=1&token=%token%";

        private static string GetDownloadToken()
        {
            string sdk_token_file = "download_token.txt";
            if (!File.Exists(sdk_token_file))
            {
                throw new Exception("Make sure there is a download token file available in the running directory: " + Directory.GetCurrentDirectory());
            }

            string downloadToken = File.ReadAllText(sdk_token_file);
            return downloadToken;
        }

      
        public static string GetTokenDownloadURL(string fileName)
        {
            string token = GetDownloadToken();
            string result = VCR_URL.Replace("%token%", token);
            result = result.Replace("%file%", fileName);
            return result;
        }

        public static string GetSDKURL()
        {
            string downloadURL = GetTokenDownloadURL("OesisPackageLinks.xml");
            return downloadURL;
        }
    }
}
