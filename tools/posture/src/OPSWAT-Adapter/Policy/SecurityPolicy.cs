///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

namespace ComplianceAdapater.Policy
{
    public class SecurityPolicy
    {
        public AntimalwarePolicy antimalwarePolicy;
        public FirewallPolicy firewallPolicy;
        public EncryptionPolicy encryptionPolicy;
    }
}
