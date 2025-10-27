///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

namespace VAPMAdapter.OESIS.POCO
{
    public class CVEDetail
    {
        public Product  product;
        public string   cveId;
        public string   description;
        public int      opswatSeverity;
        public string   rawData;
    }
}
