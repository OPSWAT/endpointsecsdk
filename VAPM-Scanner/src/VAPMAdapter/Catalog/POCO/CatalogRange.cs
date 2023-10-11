///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

namespace VAPMAdapter.Catalog.POCO
{
    public class CatalogRange
    {
        string start;
        string limit;

        public string Start { get => start; set => start = value; }
        public string Limit { get => limit; set => limit = value; }
    }
}
