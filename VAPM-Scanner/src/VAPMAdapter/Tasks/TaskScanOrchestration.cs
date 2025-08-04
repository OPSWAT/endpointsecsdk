///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VAPMAdapater.Log;
using VAPMAdapter.OESIS;
using VAPMAdapter.OESIS.POCO;

namespace VAPMAdapter.Tasks
{
    /// <summary>
    /// Represents a class to orchestrate the scanning for missing and installed patches using the OESIS Framework.
    /// </summary>
    public class TaskScanOrchestration
    {

        /// <summary>
        /// Retrieves a list of missing patches using the OESIS Framework.
        /// </summary>
        /// <returns>A list of OnlinePatchDetail objects representing the missing patches.</returns>
        private static List<OnlinePatchDetail> GetMissingPatches()
        {
            // Retrieve the detection string for missing patches
            string detectString = OESISPipe.GetMissingPatches("1103");

            // Parse the detection string into a list of OnlinePatchDetail objects.
            List<OnlinePatchDetail> result = OESISUtil.GetOnlinePatchDetail(detectString, false);
            return result;
        }

        /// <summary>
        /// Retrieves a list of installed patches using the OESIS Framework.
        /// </summary>
        /// <returns>A list of OnlinePatchDetail objects representing the installed patches.</returns>
        private static List<OnlinePatchDetail> GetInstalledPatches()
        {
            // Retrieve the detection string for installed patches.
            string detectString = OESISPipe.GetInstalledPatches("1103");

            // Parse the detection string into a list of OnlinePatchDetail objects.
            List<OnlinePatchDetail> result = OESISUtil.GetOnlinePatchDetail(detectString, true);
            return result;
        }

        /// <summary>
        /// Adds a list of patch details to a dictionary, avoiding duplicates.
        /// </summary>
        /// <param name="patchList">The list of patches to add.</param>
        /// <param name="resultList">The dictionary to add the patches to.</param>
        private static void AddListToDictionary(List<OnlinePatchDetail> patchList, Dictionary<string, OnlinePatchDetail> resultList)
        {

            // Iterate through each patch in the list
            foreach (OnlinePatchDetail current in patchList)
            {
                // Add patch to the dictionary if it does not already contain the patch key.
                if (current.kb != null && !resultList.ContainsKey(current.kb))
                {
                    resultList.Add(current.kb, current);
                }
            }
        }

        /// <summary>
        /// Orchestrates the scanning process to detect both missing and installed patches.
        /// </summary>
        /// <returns>A dictionary where the key is the patch KB identifier and the value is an OnlinePatchDetail object representing the patch.</returns>
        public static Dictionary<string,OnlinePatchDetail> Scan()
        {
            //
            // First initialize the OESIS Framework
            //
            OESISPipe.InitializeFramework(false);

            // Initialize the result dictionary to store patch details.
            Dictionary<string, OnlinePatchDetail> result = new Dictionary<string, OnlinePatchDetail>();

            // Retrieve and store lists of missing and installed patches.
            List<OnlinePatchDetail> missingPatchesList = GetMissingPatches();
            List<OnlinePatchDetail> installedPatchesList = GetInstalledPatches();

            // Add the lists of patches to the result dictionary, avoiding duplicates
            AddListToDictionary(missingPatchesList, result);
            AddListToDictionary(installedPatchesList, result);

            // Tear down the OESIS Framework after operations are completed.
            OESISPipe.Teardown();

            // Return the dictionary containing all patch details.
            return result;
        }
    }
}