///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using MessagePack;
using System;
using System.Collections.Generic;

namespace VAPMAdapter.Catalog.POCO
{
    [MessagePackObject]
    public class CatalogProduct
    {
        [Key(0)]
        public string Name;
        [Key(1)]
        public CatalogVendor Vendor;
        [Key(2)]
        public string Id;
        [Key(3)]
        public bool SupportsInstall;
        [Key(4)]
        public List<CatalogSignature> SigList;
        [Key(5)]
        public CatalogOSSupport OsType;
        [Key(6)]
        public int CveCount;
    }
}
