///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using ComplianceAdapater.Log;
using ComplianceAdapater.OESIS;
using Newtonsoft.Json.Linq;
using OPSWAT_Adapter.OESIS;
using OPSWAT_Adapter.POCO;
using System;
using System.Collections.Generic;

namespace OPSWAT_Adapter.Tasks
{
    public class TaskSecurityScore
    {
        private Logger checkLog = new Logger();

        public Logger GetLogger()
        {
            return checkLog;
        }

       


        public int GetSecurityScore()
        {
            int resultCount = 0;

            OESISFramework.InitializeFramework();

            if (OESISUtil.IsFirewallRunning())
            {
                resultCount += 2;
            }

            if (OESISUtil.IsDiskEncrypted())
            {
                resultCount += 2;
            }

            if (OESISUtil.IsAntimalwareProtected())
            {
                resultCount += 2;
            }

            if (OESISUtil.IsUpdateDefinitionRecent(DateTime.Now.AddDays(-1)))
            {
                resultCount += 2;
            }

            if (OESISUtil.IsScanRecent(DateTime.Now.AddDays(-1)))
            {
                resultCount += 2;
            }


            OESISFramework.TearDown();

            return resultCount;
        }

    }
}
