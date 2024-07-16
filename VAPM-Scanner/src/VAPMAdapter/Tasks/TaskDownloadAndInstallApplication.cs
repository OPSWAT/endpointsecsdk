///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using VAPMAdapater.Log;
using VAPMAdapter.OESIS;
using System.Collections.Generic;
using System.IO;
using System;
using VAPMAdapter.OESIS.POCO;
using VAPMAdapater;
using System.Globalization;

namespace VAPMAdapter.Tasks
{
    /// <summary>
    /// Provides methods for downloading and installing application patches using the OESIS framework.
    /// </summary>
    public class TaskDownloadAndInstallApplication
    {

        /// <summary>
        /// Generates the local file path for the installer based on the provided details.
        /// </summary>
        /// <param name="installerDetails">An object containing the installer details.</param>
        /// <returns>The local file path for the installer.</returns>
        private static string GetLocalPathForInstaller(InstallerDetail installerDetails)
        {
            string result;

            string url = installerDetails.url;
            //generate a filename based on the hash code of the URL and the file type
            string filename = url.GetHashCode() + "." + installerDetails.fileType;
            //combine the current directory path with the generated filename
            result = Path.Combine(Directory.GetCurrentDirectory(), filename);

            return result;
        }

        /// <summary>
        /// Installs a patch using the provided details.
        /// </summary>
        /// <param name="signatureId">The signature ID for the patch.</param>
        /// <param name="installDetail">An object containing the installer details.</param>
        /// <param name="forceClose"> Boolean that indicates whether to force close the application during installation.</param>
        /// <param name="isBackground">Boolean that indicates whether the installation should run in the background.</param>
        /// <param name="usePatchId">Boolean that indicates whether to use the patch ID for installation.</param>
        /// <returns>A string result indicating the outcome of the installation.</returns>
        public static string InstallPatch(string signatureId, InstallerDetail installDetail, bool forceClose, bool isBackground, bool usePatchId)
        {
            string result = null;
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


            //perform installation
            result = OESISPipe.InstallFromFiles(signatureId, installDetail.path,patchId,installDetail.language,forceClose, isBackground);
            return result;
        }

        /// <summary>
        /// Validates the installer using the provided signature ID and installation type.
        /// </summary>
        /// <param name="signatureId">The signature ID of the installer to validate.</param>
        /// <param name="isFreshInstall">Indicates whether this is a fresh installation.</param>
        /// <returns>A boolean indicating whether the installer is valid.</returns>
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

            // check the result code and determine if the installer is valid
            int result_code = currentDetail.result_code;
            if(result_code >= 0)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Retrieves a list of installer details based on the provided signature ID and installation type.
        /// </summary>
        /// <param name="signatureId">The signature ID of the installer to retrieve details for.</param>
        /// <param name="isFreshInstall">Indicates whether this is a fresh installation.</param>
        /// <param name="isBackgroundInstall">Indicates whether the installation should run in the background.</param>
        /// <returns>A list of <see cref="InstallerDetail"/> objects containing installer details.</returns>
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

                    //retrive the latest installer details
                    installDetailString = OESISPipe.GetLatestInstaller(signatureId, 1, Directory.GetCurrentDirectory(), language,isBackgroundInstall); 

                    //parses the installer info from the retrieved strings and stores it into InstallerDetail POCO
                    InstallerDetail currentDetail = OESISUtil.GetInstallerDetail(installDetailString);
                    index++;

                    //check to see if the installer is valid
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

        /// <summary>
        /// Installs and downloads patches based on the provided parameters.
        /// </summary>
        /// <param name="signatureId">The signature ID for the patches.</param>
        /// <param name="isFreshInstall">Indicates whether this is a fresh installation.</param>
        /// <param name="isBackgroundInstall">Indicates whether the installation should run in the background.</param>
        /// <param name="isValidatorInstaller">Indicates whether to validate the installer before proceeding.</param>
        /// <param name="forceClose">Indicates whether to force close the application during installation.</param>
        /// <param name="usePatchId">Indicates whether to use the patch ID for installation.</param>
        /// <returns>A <see cref="ProductInstallResult"/> object containing the result of the installation.</returns>
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
            
            // install each patch from the retrieved details
            foreach (InstallerDetail current in installDetailList)
            {
                try
                {
                    Logger.Log("Installing " + current.title);
                    string installResult = InstallPatch(signatureId, current, forceClose, isBackgroundInstall, usePatchId);
                    result.installResult = OESISUtil.GetInstallResult(installResult);

                    //
                    // Cleanup the installer if success
                    //
                    File.Delete(current.path);

                }
                catch (OESISException oe)
                {
                    Logger.Log("Failed to apply patch: " + oe);
                    result.success = false;
                    result.errorResult = oe.GetErrorResult();
                }
                catch (Exception e)
                {
                    Logger.Log("Failed to apply patch: " + e);
                    result.success = false;
                    result.message = e.ToString();
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
