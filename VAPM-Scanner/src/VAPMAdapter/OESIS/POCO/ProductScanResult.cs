///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace VAPMAdapter.OESIS.POCO
{
#nullable disable
    public class ProductScanResult
    {
        public Product product;
        public List<CVEDetail> cveDetailList;
        public PatchLevelDetail patchLevelDetail;
        public List<InstallerDetail> installDetail;
        public string cveJson;
    }
}
