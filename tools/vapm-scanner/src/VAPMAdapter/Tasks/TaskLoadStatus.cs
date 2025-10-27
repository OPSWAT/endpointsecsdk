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
    /// <summary>
    /// Represents a class to load patch status information.
    /// </summary>
    public class TaskLoadStatus
    {
        /// <summary>
        /// Loads patch status information from a JSON file.
        /// </summary>
        /// <returns>A list of PatchStatus objects containing the loaded patch status information.</returns>
        public static List<PatchStatus> Load()
        {

            UpdateStatus.Update();

            // Read the JSON content from the patch_status.json file.
            string statusJson = File.ReadAllText("patch_status.json");

            // Parse the JSON content into a list of PatchStatus objects.
            List<PatchStatus> result = OESISUtil.GetPatchStatusList(statusJson);

            // Return the list of PatchStatus objects containing the loaded patch status information.
            return result;
        }

    }
}
