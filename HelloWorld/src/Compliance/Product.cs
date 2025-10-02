///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

namespace Vulnerability
{
    public class Product
    {
        //public int productId;
        public int signatureId;
        public string name;
        public string vendor;
        public string sig_name;
        public string version;
        public string vulnerability;
        //public boolean isRunning;
        // public List<string> vulnerability_detail;
    }

    public class RunningProducts
    {
        public string name;
        public string version;
        public bool isRunning;
    }
}
