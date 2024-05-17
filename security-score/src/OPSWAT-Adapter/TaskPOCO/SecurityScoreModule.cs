///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace OPSWAT_Adapter.TaskPOCO
{
    public class SecurityScoreModule
    {
        public string name;
        public int score = 0;
        public List<SecurityScoreEntry> entries = new List<SecurityScoreEntry>();
    }
}
