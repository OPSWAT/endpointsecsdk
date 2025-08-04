///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using ComplianceAdapater.OESIS;

namespace ComplianceAdapater.Policy
{
    public class EncryptionPolicy
    {
        public bool enabled;
        public bool isEncrypted;
        public ProductInfo expectedProduct;
    }
}
