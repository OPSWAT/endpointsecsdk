///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace VAPMAdapter.OESIS.POCO
{
    public class InstallerDetail
    {
        public string url;
        public string fileType;
        public List<string> checksumList;
        public int result_code;
        public string title;
        public string severity;
        public string category;
        public string security_update_id;
        public string patch_id;
        public string path;
        public string language;
    }
}
