///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.IO;
using VAPMAdapter.Catalog.POCO;
using VAPMAdapter.OESIS;
using VAPMAdapter.OESIS.POCO;
using VAPMAdapter.Updates;

namespace VAPMAdapter.Tasks
{
    public class TaskLoadStatus
    {

        public static List<PatchStatus> Load()
        {

            UpdateStatus.Update();
            string statusJson = File.ReadAllText("patch_status.json");
            List<PatchStatus> result = OESISUtil.GetPatchStatusList(statusJson);

            return result;
        }

    }
}
