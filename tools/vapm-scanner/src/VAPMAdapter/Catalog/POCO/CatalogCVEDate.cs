///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using MessagePack;
using System;

namespace VAPMAdapter.Catalog.POCO
{
    [MessagePackObject]
    public class CatalogCVEDate
    {
        [Key(0)]
        public string cveID;

        [Key(1)]
        public DateTime modifiedDate { get; set; }

        [Key(2)]
        public DateTime releasedDate { get; set; }
    }

}
