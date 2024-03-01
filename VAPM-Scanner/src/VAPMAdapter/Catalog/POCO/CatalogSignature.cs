///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPMAdapter.Catalog.POCO
{
    public class CatalogSignature
    {
        public string   Id;
        public string   Name;
        public string   Platform;
        public string   Architecture;
        public bool     FreshInstall;
        public int      CVECount;
        public bool     BackgroundInstallSupport;
        public bool     ValidateInstallSupport;

        public List<CatalogVulnerabilityAssociation> CVEList;
        public List<CatalogPatchAssociation> PatchAssociations;

    }
}
