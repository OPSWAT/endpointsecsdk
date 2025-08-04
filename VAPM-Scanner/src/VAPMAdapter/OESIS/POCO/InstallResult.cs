///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

namespace VAPMAdapter.OESIS.POCO
{
    public class InstallResult
    {
        public int code;
        public int signature;
        public string version;
        public bool success;
        public int require_restart;
        public int require_close_first;
        public int require_uninstall_first;
    }
}
