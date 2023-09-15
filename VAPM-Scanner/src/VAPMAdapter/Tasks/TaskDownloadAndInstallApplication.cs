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
using VAPMAdapter.POCO;
using System.Collections.Generic;
using System.IO;
using System;

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


        public static void DownloadPatches(InstallerDetail installerDetail, string localFileName)
        {
            string url;
            string checksum;

            url = installerDetail.url;
            checksum = installerDetail.checksumList[0];

            // TODO:  Make sure to replace the token with the public token
            if(!HttpClientUtils.DownloadValidFile(url, localFileName, checksum))
            {
                throw (new Exception("Failed to download file or validate checksum"));
            }
        }

        public static void InstallPatch(string signatureId, string localFileName)
        {
            OESISPipe.InstallFromFiles(signatureId, 0, localFileName);
        }

        private static List<InstallerDetail> GetInstallerDetailList(string signatureId)
        {
            List<InstallerDetail> result = new List<InstallerDetail>();
            bool installerStillExists = true;
            int index = 0;

            while (installerStillExists)
            {
                try
                {
                    string installDetailString = OESISPipe.GetLatestInstaller(signatureId, 0, index);
                    InstallerDetail currentDetail = OESISUtil.GetInstallerDetail(installDetailString);
                    index++;
                    if (currentDetail.result_code != -1039)
                    {
                        result.Add(currentDetail);
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


        public static ProductInstallResult InstallAndDownload(string signatureId)
        {
            ProductInstallResult result = new ProductInstallResult();


            // 
            // Check to make sure that vmod.dat is available in the working directory
            //
            OESISUtil.ValidateDatabaseFiles();

            //
            // First initialize the OESIS Framework
            //
            OESISPipe.InitializeFramework(false);

            //
            // Now Load all the Patch databases
            //
            OESISPipe.LoadPatchDatabase("patch.dat", "ap_checksum.dat"); // Note for a scan only you can skip loading this.  Checksums are not needed for scan
            OESISPipe.ConsumeOfflineVmodDatabase("vmod.dat"); // This loads Third-Party Patches. You can also use vmod-oft.dat if you do not need a CVE Description or text

            //
            // Load Windows Detection libraries
            //
            if (signatureId == "1103")
            {
                OESISPipe.LoadPatchDatabase("wuo.dat", ""); // This is needed for scanning for CVE's in Microsoft Products. 
            }

            List<InstallerDetail> installDetailList = GetInstallerDetailList(signatureId);

            foreach (InstallerDetail current in installDetailList)
            {
                string localFileName = OESISUtil.GetFilenameFromUrl(current);

                try
                {
                    DownloadPatches(current, localFileName);
                    InstallPatch(signatureId, localFileName);
                }
                catch(Exception e)
                {
                    Logger.Log("Failed to apply patch: " + e);
                    result.success = false;
                    result.errorMessage = e.ToString();
                }
            }

            OESISPipe.Teardown();

            return result;
        }

    }
}
