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
    public class SecurityScoreResult
    {
        public List<SecurityScoreModule> moduleList = new List<SecurityScoreModule>();
        public int score;
    }
}
