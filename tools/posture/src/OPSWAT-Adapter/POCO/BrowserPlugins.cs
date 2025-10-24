///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace OPSWAT_Adapter.POCO
{
    public class BrowserPlugins
    {
        public string signatureId;
        public string browserName;
        public string code;
        public List<PluginDetail> pluginList;
    }
}
