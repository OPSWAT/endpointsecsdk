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
    public class CatalogPatchAssociation
    {
        public List<string> SigIdList;
        public string PatchId;
        public bool   IsLatest;
        public string VersionComparer;
        public string PatchAggregation;

    }
}
