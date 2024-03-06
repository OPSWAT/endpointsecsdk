///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////
namespace VAPMAdapter.OESIS.POCO
{
    public class ProductInstallResult
    {
        public bool success;
        public bool returnCode;
        public string message;
        public ErrorResult  errorResult;
        public InstallResult installResult;
    }
}
