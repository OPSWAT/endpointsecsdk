﻿///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

namespace VAPMAdapter.OESIS.POCO
{
    internal class InstallResults
    {
        public string version;
        public bool success;
        public int result_code;
        public int require_restart;
    }
}
