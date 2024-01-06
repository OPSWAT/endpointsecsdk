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
    public class CatalogPatchAggregation
    {
        public List<CatalogDownloadDetails> DownloadDetailsList;
        public string ReleaseNoteLink;
        public string Release_Date;
        public List<string> Architectures;
        public string PatchId;
        public string Fresh_Installable;
        public string Requires_Reboot;
        public string LatestVersion;

    }
}
