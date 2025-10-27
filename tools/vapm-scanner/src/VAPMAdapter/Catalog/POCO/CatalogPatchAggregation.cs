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
    public class CatalogPatchAggregation
    {
        [Key(0)]
        public List<CatalogDownloadDetails> DownloadDetailsList { get; set; }
        [Key(1)]
        public string ReleaseNoteLink { get; set; }
        [Key(2)]
        public string Release_Date { get; set; }
        [Key(3)]
        public List<string> Architectures { get; set; }
        [Key(4)]
        public string PatchId { get; set; }
        [Key(5)]
        public string Fresh_Installable { get; set; }
        [Key(6)]
        public string Requires_Reboot { get; set; }
        [Key(7)]
        public string LatestVersion { get; set; }
    }

}
