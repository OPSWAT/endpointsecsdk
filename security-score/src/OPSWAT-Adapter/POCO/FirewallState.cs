///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

namespace OPSWAT_Adapter.POCO
{
    public class FirewallState : MethodResult
    {
        public bool enabled;
        public string profileDetailsJson;
        public string managedBy3rdPartyProductsJson;
    }
}
