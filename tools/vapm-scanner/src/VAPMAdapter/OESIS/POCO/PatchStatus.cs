///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace VAPMAdapter.OESIS.POCO
{
    public class PatchStatus
    {
        public int      productId;
        public string   productName;
        public int      signatureId;
        public string   signatureName;
        public string   status;
        public string   lastTested;
        public string   platform;
        public string   lastKnownGood;
    }
}
