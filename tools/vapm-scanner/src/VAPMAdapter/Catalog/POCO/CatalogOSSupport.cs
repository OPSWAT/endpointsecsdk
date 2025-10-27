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
    public class CatalogOSSupport
    {
        [Key(0)]
        public bool Windows { get; set; }

        [Key(1)]
        public bool Mac { get; set; }

        [Key(2)]
        public bool Linux { get; set; }
    }

}
