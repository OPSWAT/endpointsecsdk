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

    /// <summary>
    /// Represents a class to orchestrate the download and installation of patches using the OESIS Framework.
    /// </summary>
    public class TaskOrchestrateDownloadAndInstall
    {
        /// <summary>
        /// Downloads a patch using the OESIS Framework.
        /// </summary>
        /// <param name="patchDetail">The patch detail specifying the patch to download.</param>
        private static void DownloadPatch(OnlinePatchDetail patchDetail)
        {
            OESISPipe.DownloadMissingPatches("1103", patchDetail.title, patchDetail.product, patchDetail.vendor);
        }

        /// <summary>
        /// Installs a patch using the OESIS Framework.
        /// </summary>
        /// <param name="patchDetail">The patch detail specifying the patch to install.</param>
        private static void InstallPatch(OnlinePatchDetail patchDetail)
        {
            OESISPipe.InstallMissingPatches("1103", patchDetail.title, patchDetail.product, patchDetail.vendor);
        }

        /// <summary>
        /// Orchestrates the download and installation of a patch.
        /// </summary>
        /// <param name="patchDetail">The patch detail specifying the patch to download and install.</param>
        /// <returns>A ProductInstallResult indicating the result of the installation process.</returns>
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

            // Return the result object indicating the installation result.
            return result;
        }

    }
}
