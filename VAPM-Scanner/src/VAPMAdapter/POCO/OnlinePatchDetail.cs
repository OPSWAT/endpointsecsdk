///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

namespace VAPMAdapter.POCO
{
#nullable disable
    public class OnlinePatchDetail
    {
        public string title;
        public string description;
        public string severity;
        public string kb;
        public string vendor;
        public string product;
        public bool   installed;
    }
}
