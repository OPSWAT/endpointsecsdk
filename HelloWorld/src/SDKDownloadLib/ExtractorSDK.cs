///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.IO;

namespace SDKDownloader
{
    public class ExtractorSDK
    {
        private static void CopyExtractedFile(string libDir, string sdkDir, string architecture, string folder, string filename)
        {
            string rootFile = Path.Combine(sdkDir, "sdk");
            rootFile = Path.Combine(rootFile, folder);
            rootFile = Path.Combine(rootFile, architecture);
            rootFile = Path.Combine(rootFile, "release");
            rootFile = Path.Combine(rootFile, filename);

            string destFile = Path.Combine(libDir, filename);


            File.Copy(rootFile, destFile, true);
        }

        private static void CopySDKFiles(string sdkDir,string libDir, string architecture)
        {
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/detection", "libwaaddon.dll");
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/detection", "libwaapi.dll");
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/detection", "libwaheap.dll");
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/detection", "libwautils.dll");
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/manageability", "libwalocal.dll");
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/manageability", "wa_3rd_party_host_32.exe");
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/manageability", "wa_3rd_party_host_64.exe");
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/vulnerability", "libwavmodapi.dll");
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/deviceinfo", "libwadeviceinfo.dll");
        }

        private static void CopyResourceFiles(string sdkDir, string libDir)
        {
            string destFile = Path.Combine(libDir, "libwaresource.dll");
            string sourceFile = Path.Combine(sdkDir, "resource/bin/libwaresource.dll");
            File.Copy(sourceFile, destFile, true);
        }

        public static void CopyAllLibFiles(string extractDir, string libDir, string architecture)
        {
            CopySDKFiles(extractDir, libDir, architecture);
            CopyResourceFiles(extractDir, libDir);
        }



        //
        // Platforms are as follows
        // 1 - Windows
        // 2 - Mac
        // 3 - Linux
        public static void DownloadAndCopy(string libDir, int platform, string architecture)
        {
            string tempArchiveDir = Util.GetCleanTempDir("OESIS-ARCHIVE");
            DownloadSDK.Download(tempArchiveDir);


            string tempSDKDir = Util.GetCleanTempDir("OESIS-SDK");
            Util.ExtractArchives(tempArchiveDir, tempSDKDir);

            if (platform == 1)
            {
                ExtractorSDK.CopyAllLibFiles(tempSDKDir, libDir, architecture);
            }
            if (platform == 2)
            {
                // TODO: Copy Mac files
            }
            if (platform == 3)
            {
                // TODO: Copy Windows files
            }


            //
            // Cleanup the temp paths
            //
            Directory.Delete(tempArchiveDir,true);
            Directory.Delete(tempSDKDir,true);
        }
    }
}
