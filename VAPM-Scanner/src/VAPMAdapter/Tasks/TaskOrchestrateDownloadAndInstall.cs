///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////


using System;
using VAPMAdapater.Log;
using VAPMAdapter.OESIS;
using VAPMAdapter.OESIS.POCO;

namespace VAPMAdapter.Tasks
{
    public class TaskOrchestrateDownloadAndInstall
    {
        private static void DownloadPatch(OnlinePatchDetail patchDetail)
        {
            OESISPipe.DownloadMissingPatches("1103", patchDetail.title, patchDetail.product, patchDetail.vendor);
        }

        private static void InstallPatch(OnlinePatchDetail patchDetail)
        {
            OESISPipe.InstallMissingPatches("1103", patchDetail.title, patchDetail.product, patchDetail.vendor);
        }


        public static ProductInstallResult InstallAndDownload(OnlinePatchDetail patchDetail)
        {
            ProductInstallResult result = new ProductInstallResult();

            //
            // First initialize the OESIS Framework
            //
            OESISPipe.InitializeFramework(false);
            
            try
            {
                DownloadPatch(patchDetail);
                InstallPatch(patchDetail); 
            }
            catch(Exception e)
            {
                Logger.Log(e.ToString());
            }


            OESISPipe.Teardown();

            return result;
        }

    }
}
