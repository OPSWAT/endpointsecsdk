///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////

using MessagePack;

///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

namespace VAPMAdapter.Catalog.POCO
{
    [MessagePackObject]
    public class CatalogDownloadDetails
    {
        [Key(0)]
        public string Link { get; set; }
        [Key(1)]
        public string Architecture { get; set; }
        [Key(2)]
        public string Language { get; set; }
    }

}
