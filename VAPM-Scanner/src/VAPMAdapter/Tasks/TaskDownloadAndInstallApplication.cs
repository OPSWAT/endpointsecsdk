///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using VAPMAdapater.Log;
using VAPMAdapater.Updates;
using VAPMAdapter.OESIS;
using System.Collections.Generic;
using System.IO;
using System;
using VAPMAdapter.OESIS.POCO;
using VAPMAdapater;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace VAPMAdapter.Tasks
{
    public class TaskDownloadAndInstallApplication
    {

        private static string getLocalPathForInstaller(InstallerDetail installerDetails)
        {
            string result;

            string url = installerDetails.url;
            string filename = url.GetHashCode() + "." + installerDetails.fileType;
            result = Path.Combine(Directory.GetCurrentDirectory(), filename);

            return result;
        }

        public static void InstallPatch(string signatureId, InstallerDetail installDetail, bool forceClose, bool isBackground, bool usePatchId)
        {
            string patchId = null;

            //
            // This is an advanced option.  The preferred method for installing patches
            // is to use just the signature ID and allow the system to patch the correct installer
            // the PatchID is only needed for the use case of enforcing specific versions.  The recommendation
            // is to not use this feature.
            //
            if(usePatchId)
            {
                patchId = installDetail.patch_id;
            }



            OESISPipe.InstallFromFiles(signatureId, installDetail.path,patchId,installDetail.language,forceClose, isBackground);
        }

        private static bool ValidateInstaller(string signatureId, bool isFreshInstall)
        {
            bool result = false;

            //
            // If this is a fresh install pass in the language for the OS
            //
            string language = null;
            if (isFreshInstall)
            {
                CultureInfo ci = CultureInfo.InstalledUICulture;
                language = ci.ToString();
            }

            //
            // Use the Download=2 for query only details and to validate the install
            //
            string validateResult = OESISPipe.GetLatestInstaller(signatureId, 2, 0, language,false,true,null);
            InstallerDetail currentDetail = OESISUtil.GetInstallerDetail(validateResult);

            int result_code = currentDetail.result_code;
            if(result_code >= 0)
            {
                result = true;
            }

            return result;
        }

        //  Note for patching the language will be automatically detected, but for new install a language needs to be specified
        private static List<InstallerDetail> GetInstallerDetailList(string signatureId, bool isFreshInstall, bool isBackgroundInstall)
        {
            List<InstallerDetail> result = new List<InstallerDetail>();
            bool installerStillExists = true;
            int index = 0;

            while (installerStillExists)
            {
                try
                {
                    string installDetailString = null;
                    string language = null;

                    //
                    // If this is a fresh install pass in the language for the OS
                    //
                    if (isFreshInstall)
                    {
                        CultureInfo ci = CultureInfo.InstalledUICulture;
                        language = ci.ToString();
                    }


                    installDetailString = OESISPipe.GetLatestInstaller(signatureId, 1, Directory.GetCurrentDirectory(), language,isBackgroundInstall); 


                    InstallerDetail currentDetail = OESISUtil.GetInstallerDetail(installDetailString);
                    index++;

                    if (currentDetail.result_code != -1039)
                    {
                        result.Add(currentDetail);
                        break; // Need to fix this for WIndows patches
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Logger.Log("Installer not available. " + e);
                    break;
                }
            }

            return result;
        }


        public static ProductInstallResult InstallAndDownload(  string signatureId, 
                                                                bool isFreshInstall, 
                                                                bool isBackgroundInstall, 
                                                                bool isValidatorInstaller,
                                                                bool forceClose,
                                                                bool usePatchId)
        {
            ProductInstallResult result = new ProductInstallResult();
            result.success = true;


            // 
            // Check to make sure that vmod.dat is available in the working directory
            //
            OESISUtil.ValidateDatabaseFiles();

            //
            // First initialize the OESIS Framework
            // Always enable debugging on an install.  Clean this up on a success, but save this on a failure
            //
            OESISPipe.InitializeFramework(true);

            //
            // Now Load all the Patch databases
            //
            OESISPipe.LoadPatchDatabase(VAPMSettings.THIRD_PARTY_PATCH_DB, VAPMSettings.PATCH_CHECKSUMS_DB); // Note for a scan only you can skip loading this.  Checksums are not needed for scan
            OESISPipe.ConsumeOfflineVmodDatabase(VAPMSettings.THIRD_PARTY_VULNERABILITY_DB); // This loads Third-Party Patches. You can also use vmod-oft.dat if you do not need a CVE Description or text

            //
            // Load Windows Detection libraries
            //
            if (signatureId == "1103")
            {
                OESISPipe.LoadPatchDatabase(VAPMSettings.WINDOWS_PATCH_DB, ""); // This is needed for scanning for CVE's in Microsoft Products. 
            }


            //
            // Validate the Installer
            //
            if(isValidatorInstaller)
            {
                if(!ValidateInstaller(signatureId,isFreshInstall))
                {
                    result.success = false;
                    return result;
                }
            }



            // Make sure to get the languge of the OS on a Fresh Install.  This will attempt to download the patch
            Logger.Log("Getting Install Details");
            List<InstallerDetail> installDetailList = GetInstallerDetailList(signatureId,isFreshInstall,isBackgroundInstall);
            
            foreach (InstallerDetail current in installDetailList)
            {
                try
                {
                    Logger.Log("Installing " + current.title);
                    InstallPatch(signatureId, current, forceClose, isBackgroundInstall, usePatchId);

                    //
                    // Cleanup the installer if success
                    //
                    File.Delete(current.path);

                }
                catch(Exception e)
                {
                    Logger.Log("Failed to apply patch: " + e);
                    result.success = false;
                    result.errorMessage = e.ToString();
                }
            }

            OESISPipe.Teardown();

            if(result.success)
            {
                //
                // Now delete the log file here
                //
                OESISUtil.CleanupDebugFiles();
            }
            else
            {
                OESISUtil.MoveDebugFiles(Path.Combine(Directory.GetCurrentDirectory(), "FailedInstalls"));
            }

            return result;
        }

    }
}
