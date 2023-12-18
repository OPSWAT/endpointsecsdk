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

        public static void InstallPatch(string signatureId, InstallerDetail installDetail)
        {
            OESISPipe.InstallFromFiles(signatureId, 0, installDetail.path,installDetail.patch_id,installDetail.language);
        }


        //  Note for patching the language will be automatically detected, but for new install a language needs to be specified
        private static List<InstallerDetail> GetInstallerDetailList(string signatureId, bool isFreshInstall)
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


                    // Office 365 requires you pass in a 1 because it's Orchestrating the download tool for office 365
                    if (signatureId != "3029")
                    {
                        installDetailString = OESISPipe.GetLatestInstaller(signatureId, 1, Directory.GetCurrentDirectory(), language); 
                        //installDetailString = OESISPipe.GetLatestInstaller(signatureId, 0, index);
                    }
                    else
                    {
                        installDetailString = OESISPipe.GetLatestInstaller(signatureId, 1, Directory.GetCurrentDirectory(),language);                        
                    }

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


        public static ProductInstallResult InstallAndDownload(string signatureId, bool isFreshInstall)
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




            // Make sure to get the languge of the OS on a Fresh Install.  This will attempt to download the patch
            Logger.Log("Getting Install Details");
            List<InstallerDetail> installDetailList = GetInstallerDetailList(signatureId,isFreshInstall);
            
            foreach (InstallerDetail current in installDetailList)
            {
                try
                {
                    Logger.Log("Installing " + current.title);
                    InstallPatch(signatureId, current);

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
