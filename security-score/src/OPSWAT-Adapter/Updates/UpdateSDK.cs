///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using VAPMAdapter.Updates;

namespace VAPMAdapater.Updates
{
    public class UpdateSDK
    {
        public static bool isSDKUpdated()
        {
            bool result = false;

            if(File.Exists("libwalocal.dll"))
            {
                FileInfo vmodInfo = new FileInfo("libwalocal.dll");

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

        private static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                DirectoryInfo[] dirs = dir.GetDirectories();
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private static void CopyAllFiles(string sdkDir)
        {
            CopyDirectory("sdktemp\\1\\docs\\support_charts\\xml_supportCharts_v2", ".", false);
            CopySdkFile(sdkDir, "1/bin/detection", "libwaaddon.dll");
            CopySdkFile(sdkDir, "1/bin/detection", "libwaapi.dll");
            CopySdkFile(sdkDir, "1/bin/detection", "libwaheap.dll");
            CopySdkFile(sdkDir, "1/bin/detection", "libwautils.dll");
            CopySdkFile(sdkDir, "1/bin/manageability", "libwalocal.dll");
            CopySdkFile(sdkDir, "1/bin/deviceinfo", "libwadeviceinfo.dll");
            CopySdkFile(sdkDir, "1/bin/manageability", "wa_3rd_party_host_32.exe");
            CopySdkFile(sdkDir, "1/bin/manageability", "wa_3rd_party_host_64.exe");
            File.Copy(Path.Combine(sdkDir, "2/bin/libwaresource.dll"), "libwaresource.dll",true);
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
            ExtractUtils.ExtractZipFiles(sdkDir);
            CopyAllFiles(sdkDir);
            CleanSDKFiles(sdkDir);
        }


    }
}