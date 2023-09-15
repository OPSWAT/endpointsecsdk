///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace VAPMAdapter.Catalog.POCO
{
    public class CatalogProduct
    {
        public string Name;
        public CatalogVendor Vendor;
        public string Id;
        public bool SupportsInstall;
        public List<CatalogSignature> SigList;
        public CatalogOSSupport OsType;
        public int CveCount;
    }
}
