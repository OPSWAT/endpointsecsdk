///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace SDKDownloader
{
    public class ExtractorSDK
    {
        private static void CopyExtractedFile(string destDir, string sdkDir, string architecture, string folder, string filename)
        {
            string rootFile = Path.Combine(sdkDir, "sdk");
            rootFile = Path.Combine(rootFile, folder);
            rootFile = Path.Combine(rootFile, architecture);
            rootFile = Path.Combine(rootFile, "release");
            rootFile = Path.Combine(rootFile, filename);

            Util.MakeDirs(destDir);
            string destFile = Path.Combine(destDir, filename);
            File.Copy(rootFile, destFile, true);
        }

        private static void CopyLinkFiles(string sdkDir, string linkDir, string architecture)
        {
            CopyExtractedFile(linkDir, sdkDir, architecture, "lib", "libwaapi.lib");
        }

        private static void CopySDKFiles(string sdkDir, string libDir, string architecture)
        {
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/detection", "libwaaddon.dll");
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/detection", "libwaapi.dll");
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/detection", "libwaheap.dll");
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/detection", "libwautils.dll");
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/manageability", "libwalocal.dll");
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/vulnerability", "libwavmodapi.dll");
            CopyExtractedFile(libDir, sdkDir, architecture, "bin/deviceinfo", "libwadeviceinfo.dll");

            if (architecture != "arm64")
            {
                CopyExtractedFile(libDir, sdkDir, architecture, "bin/manageability", "wa_3rd_party_host_32.exe");
                CopyExtractedFile(libDir, sdkDir, architecture, "bin/manageability", "wa_3rd_party_host_64.exe");
            }
            else
            {
                CopyExtractedFile(libDir, sdkDir, architecture, "bin/manageability", "wa_3rd_party_host_ARM64.exe");
            }

        }

        private static void CopyResourceFiles(string destDir, string libDir)
        {
            string destFile = Path.Combine(libDir, "libwaresource.dll");

            Util.MakeDirs(destDir);
            string sourceFile = Path.Combine(destDir, "resource/bin/libwaresource.dll");
            File.Copy(sourceFile, destFile, true);
        }

        public static void CopyAllLibFiles(string extractDir, string libDir, string architecture)
        {
            string destArchFolder = Path.Combine(libDir, architecture);
            Util.MakeDirs(destArchFolder);

            CopySDKFiles(extractDir, destArchFolder, architecture);
            CopyResourceFiles(extractDir, destArchFolder);
        }

        public static void CopyAllLinkFiles(string extractDir, string linkDir, string architecture)
        {
            string sourceLinkFile = Path.Combine(extractDir, "sdk\\lib");
            sourceLinkFile = Path.Combine(sourceLinkFile, architecture);
            sourceLinkFile = Path.Combine(sourceLinkFile, "release\\libwaapi.lib");

            string destLinkFolder = Path.Combine(linkDir, architecture);
            Util.MakeDirs(destLinkFolder);

            string destLinkFile = Path.Combine(destLinkFolder, "libwaapi.lib");
            File.Copy(sourceLinkFile, destLinkFile, true);
        }
        public static void CopyAllIncFiles(string extractDir, string incDir)
        {
            string sourceIncFolder = Path.Combine(extractDir, "sdk\\inc");
            Util.MakeDirs(incDir);
            Util.CopyDirectory(sourceIncFolder, incDir, true);
        }

        //
        // Platforms are as follows
        // 1 - Windows
        // 2 - Mac
        // 3 - Linux
        public static void DownloadAndCopy(string rootDir)
        {
            string tempSDKDir = Util.GetCleanTempDir("OESIS-SDK");
            string tempArchiveDir = Util.GetCleanTempDir("OESIS-ARCHIVE");

            //
            // Download the build and extract it into a temp directory
            //
            DownloadSDK.Download(tempArchiveDir);

            Console.WriteLine("Extracting SDK Files");
            Util.CreateCleanDir(tempSDKDir);
            Util.ExtractArchives(tempArchiveDir, tempSDKDir);

            string rootSDKDir = Path.Combine(rootDir, "sdk");
            string rootLibDir = Path.Combine(rootSDKDir, "lib");
            string rootLinkDir = Path.Combine(rootSDKDir, "link");
            string rootIncDir = Path.Combine(rootSDKDir, "inc");

            Console.WriteLine("Copying Windows SDK Files");
            CopyAllIncFiles(tempSDKDir, rootIncDir);

            CopyAllLibFiles(tempSDKDir, rootLibDir, "x64");
            CopyAllLinkFiles(tempSDKDir, rootLinkDir, "x64");

            CopyAllLibFiles(tempSDKDir, rootLibDir, "win32");
            CopyAllLinkFiles(tempSDKDir, rootLinkDir, "win32");

            CopyAllLibFiles(tempSDKDir, rootLibDir, "arm64");
            CopyAllLinkFiles(tempSDKDir, rootLinkDir, "arm64");

            //
            // Cleanup the temp paths
            //
            Directory.Delete(tempArchiveDir, true);
            Directory.Delete(tempSDKDir, true);
        }
    }
}
