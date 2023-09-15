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
using VAPMAdapater.Log;
using VAPMAdapter.OESIS;
using VAPMAdapter.POCO;

namespace VAPMAdapter.Tasks
{
    public class TaskScanOrchestration
    {
        private static List<OnlinePatchDetail> GetMissingPatches()
        {
            string detectString = OESISPipe.GetMissingPatches("1103");
            List<OnlinePatchDetail> result = OESISUtil.GetOnlinePatchDetail(detectString, false);
            return result;
        }

        private static List<OnlinePatchDetail> GetInstalledPatches()
        {
            string detectString = OESISPipe.GetInstalledPatches("1103");
            List<OnlinePatchDetail> result = OESISUtil.GetOnlinePatchDetail(detectString, true);
            return result;
        }

        private static void AddListToDictionary(List<OnlinePatchDetail> patchList, Dictionary<string, OnlinePatchDetail> resultList)
        {
            foreach(OnlinePatchDetail current in patchList)
            {
                if (!resultList.ContainsKey(current.kb))
                {
                    resultList.Add(current.kb, current);
                }
            }
        }


        public static Dictionary<string,OnlinePatchDetail> Scan()
        {
            //
            // First initialize the OESIS Framework
            //
            OESISPipe.InitializeFramework(false);

            Dictionary<string, OnlinePatchDetail> result = new Dictionary<string, OnlinePatchDetail>();

            List<OnlinePatchDetail> missingPatchesList = GetMissingPatches();
            List<OnlinePatchDetail> installedPatchesList = GetInstalledPatches();

            AddListToDictionary(missingPatchesList, result);
            AddListToDictionary(installedPatchesList, result);

            OESISPipe.Teardown();

            return result;
        }
    }
}