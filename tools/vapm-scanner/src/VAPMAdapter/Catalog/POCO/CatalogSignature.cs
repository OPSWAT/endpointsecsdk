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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPMAdapter.Catalog.POCO
{

    [MessagePackObject]
    public class CatalogSignature
    {
        [Key(0)]
        public string Id { get; set; }
        [Key(1)]
        public string Name { get; set; }
        [Key(2)]
        public string Platform { get; set; }
        [Key(3)]
        public string Architecture { get; set; }
        [Key(4)]
        public bool FreshInstall { get; set; }
        [Key(5)]
        public int CVECount { get; set; }
        [Key(6)]
        public bool BackgroundInstallSupport { get; set; }
        [Key(7)]
        public bool ValidateInstallSupport { get; set; }
        [Key(8)]
        public List<CatalogVulnerabilityAssociation> CVEList { get; set; }
        [Key(9)]
        public List<CatalogPatchAssociation> PatchAssociations { get; set; }
    }

}
