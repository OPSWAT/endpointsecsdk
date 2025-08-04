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
        private static string DOWNLOAD_TOKEN = "";
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
    }
}
