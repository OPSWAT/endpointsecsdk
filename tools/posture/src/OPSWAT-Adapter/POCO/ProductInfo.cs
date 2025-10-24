///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

namespace ComplianceAdapater.OESIS
{
    public class ProductInfo
    {
        public string name;
        public int sigId;

        public override string ToString()
        {
            return name;
        }

    }
}
