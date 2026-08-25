using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSWAT_Adapter
{
    /// <summary>
    /// Builds the authenticated OPSWAT VCR download URLs used by the SDK updater. The download
    /// token is read from download_token.txt in the running directory.
    /// Contact oem@opswat.com for a download token / SDK URL.
    /// </summary>
    internal class SDKSettings
    {
        // VCR download URL template; %file% and %token% are substituted at runtime.
        private static string VCR_URL = "https://vcr.opswat.com/gw/file/download/%file%?type=1&token=%token%";

        // Reads the OPSWAT download token from download_token.txt in the running directory.
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

      
        /// <summary>Returns the token-authenticated VCR download URL for the given file name.</summary>
        public static string GetTokenDownloadURL(string fileName)
        {
            string token = GetDownloadToken();
            string result = VCR_URL.Replace("%token%", token);
            result = result.Replace("%file%", fileName);
            return result;
        }

        /// <summary>Returns the download URL for the OESIS package descriptor (OesisPackageLinks.xml).</summary>
        public static string GetSDKURL()
        {
            string downloadURL = GetTokenDownloadURL("OesisPackageLinks.xml");
            return downloadURL;
        }
    }
}
