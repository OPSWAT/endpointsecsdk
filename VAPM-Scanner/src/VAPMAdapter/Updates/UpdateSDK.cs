///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace VAPMAdapater.Updates
{
    public class UpdateSDK
    {
        public static bool isSDKUpdated()
        {
            bool result = false;

            if(File.Exists("libwavmodapi.dll"))
            {
                FileInfo vmodInfo = new FileInfo("libwavmodapi.dll");

                //
                // Update the SDK every 7 days
                //
                if(vmodInfo.LastWriteTime > DateTime.Now.AddDays(-7))
                {
                    result = true;
                }
            }

            return result;
        }

        
        private static void CopySdkFile(string sdkDir, string folder, string filename)
        {
            string rootFile = Path.Combine(sdkDir, folder);
            rootFile = Path.Combine(rootFile, "x64/release");
            rootFile = Path.Combine(rootFile, filename);

            File.Copy(rootFile, filename, true);
        }
        

        private static void CleanSDKFiles(string sdkDir)
        {
            Directory.Delete(sdkDir, true);
        }

        
        private static void CopyAllFiles(string sdkDir)
        {
            CopySdkFile(sdkDir, "bin/detection", "libwaaddon.dll");
            CopySdkFile(sdkDir, "bin/detection", "libwaapi.dll");
            CopySdkFile(sdkDir, "bin/detection", "libwaheap.dll");
            CopySdkFile(sdkDir, "bin/detection", "libwautils.dll");
            CopySdkFile(sdkDir, "bin/manageability", "libwalocal.dll");
            CopySdkFile(sdkDir, "bin/manageability", "wa_3rd_party_host_32.exe");
            CopySdkFile(sdkDir, "bin/manageability", "wa_3rd_party_host_64.exe");
            CopySdkFile(sdkDir, "bin/vulnerability", "libwavmodapi.dll");
            File.Copy(Path.Combine(sdkDir, "bin/libwaresource.dll"), "libwaresource.dll",true);
        }


        //
        // Extract all the zip files in the SDK Directory
        //
        private static void ExtractZipFiles(string sdkDir)
        {
            string[] sdkFiles = Directory.GetFiles(sdkDir);

            if (sdkFiles != null)
            {
                foreach(string current in sdkFiles)
                {
                    FileInfo fileInfo = new FileInfo(current);
                    if(fileInfo.Extension == ".zip")
                    {
                        //
                        // New Directory
                        //
                        System.IO.Compression.ZipFile.ExtractToDirectory(fileInfo.FullName, sdkDir, true);
                        File.Delete(fileInfo.FullName);
                    }
                }
            }
        }


        public static string getLocalSDKDir()
        {
            //
            // First delete the SDK directory if it exists
            //
            string sdkDir = Path.Combine(Directory.GetCurrentDirectory(), "sdktemp");

            if (Directory.Exists(sdkDir))
            {
                Directory.Delete(sdkDir, true);
            }

            Directory.CreateDirectory(sdkDir);

            return sdkDir;
        }


        public static void DownloadAndInstall_OPSWAT_SDK()
        {
            string sdkDir = getLocalSDKDir();

            DownloadSDK.DownloadAllSDKFiles(sdkDir);
            ExtractZipFiles(sdkDir);
            CopyAllFiles(sdkDir);
            CleanSDKFiles(sdkDir);
        }


    }
}