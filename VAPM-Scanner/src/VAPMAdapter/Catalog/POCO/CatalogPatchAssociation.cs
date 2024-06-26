///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using MessagePack;
using System.Collections.Generic;

namespace VAPMAdapter.Catalog.POCO
{
    [MessagePackObject]
    public class CatalogPatchAssociation
    {
        [Key(0)]
        public List<string> SigIdList { get; set; }
        [Key(1)]
        public string PatchId { get; set; }
        [Key(2)]
        public bool IsLatest { get; set; }
        [Key(3)]
        public string VersionComparer { get; set; }
        [Key(4)]
        public CatalogPatchAggregation PatchAggregation { get; set; }
    }

}
