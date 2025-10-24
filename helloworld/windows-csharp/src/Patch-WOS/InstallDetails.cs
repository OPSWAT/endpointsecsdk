///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace PatchWOS
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