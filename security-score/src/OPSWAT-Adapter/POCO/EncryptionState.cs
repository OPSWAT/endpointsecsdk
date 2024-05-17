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
    public class EncryptionState : MethodResult
    {
        public bool fully_encrypted;
        public bool encryption_active;
        public bool support_wde;
        public string locationsJson;
    }
}
