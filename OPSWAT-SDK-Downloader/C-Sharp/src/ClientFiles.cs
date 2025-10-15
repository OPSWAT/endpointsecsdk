﻿using System;
using System.IO;

namespace SDKDownloader
{
    internal class ClientFiles
    {

        private static void CopyExtractedFolder(string destPath, string rootPath, string subfolder)
        {
            string sourceFileFolder = Path.Combine(rootPath, subfolder);

            Util.CopyDirectory(sourceFileFolder, destPath, true);
        }


        private static void CopyExtractedFile(string destPath, string rootPath, string subfolder, string filename)
        {
            string sourceFileFolder = Path.Combine(rootPath, subfolder);
            string sourceFilePath = Path.Combine(sourceFileFolder, filename);
            string destFile = Path.Combine(destPath, filename);

            if(!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            File.Copy(sourceFilePath, destFile, true);
        }

        private static void CopyWindowsArchitecture(string extractPath, string clientPath, string arch)
        {
            Console.WriteLine("Copying Windows Client Files with Architecture: " + arch);

            //
            // Copy the static files from the SDK Directory.  These files change less often
            //
            string destPath = Path.Combine(clientPath, "windows/" + arch);
            string sourcePath = Path.Combine(extractPath, "windows");
            CopyExtractedFile(destPath, sourcePath, "bin/detection/" + arch + "/release", "libwaaddon.dll");
            CopyExtractedFile(destPath, sourcePath, "bin/detection/" + arch + "/release", "libwaapi.dll");
            CopyExtractedFile(destPath, sourcePath, "bin/detection/" + arch + "/release", "libwaheap.dll");
            CopyExtractedFile(destPath, sourcePath, "bin/detection/" + arch + "/release", "libwautils.dll");
            CopyExtractedFile(destPath, sourcePath, "bin/manageability/" + arch + "/release", "libwalocal.dll");
            CopyExtractedFile(destPath, sourcePath, "bin/vulnerability/" + arch + "/release", "libwavmodapi.dll");
            CopyExtractedFile(destPath, sourcePath, "bin/deviceinfo/" + arch + "/release", "libwadeviceinfo.dll");

            // Copy the Windows interaction files
            if(arch == "arm64")
            {
                CopyExtractedFile(destPath, sourcePath, "bin/manageability/" + arch + "/release", "wa_3rd_party_host_ARM64.exe");
            }
            else if (arch == "x64")
            {
                CopyExtractedFile(destPath, sourcePath, "bin/manageability/" + arch + "/release", "wa_3rd_party_host_32.exe");
                CopyExtractedFile(destPath, sourcePath, "bin/manageability/" + arch + "/release", "wa_3rd_party_host_64.exe");
            }
            else if (arch == "win32")
            {
                CopyExtractedFile(destPath, sourcePath, "bin/manageability/" + arch + "/release", "wa_3rd_party_host_32.exe");
            }



            //
            // Copy the dyanmic resource file.  This contains the version information - This file changes every 4 hours
            //
            string resourcePath = Path.Combine(extractPath, "compliance/windows/bin");
            CopyExtractedFile(destPath, resourcePath, "", "libwaresource.dll");

            //
            // Copy the dynamic files - NOTE These are related to Patch and Vulnerability - This file changes every 4 hours
            //
            string patchCatalogPath = Path.Combine(extractPath, "analog/client");
            CopyExtractedFile(destPath, patchCatalogPath, "", "ap_checksum.dat");
            CopyExtractedFile(destPath, patchCatalogPath, "", "patch.dat");
            CopyExtractedFile(destPath, patchCatalogPath, "", "v2mod.dat");
            CopyExtractedFile(destPath, patchCatalogPath, "", "wuo.dat");
            CopyExtractedFile(destPath, patchCatalogPath, "", "wiv-lite.dat");
        }
        private static void CopyWindowsFiles(string extractDir, string clientPath)
        {
            CopyWindowsArchitecture(extractDir, clientPath, "x64");
            CopyWindowsArchitecture(extractDir, clientPath, "win32");
            CopyWindowsArchitecture(extractDir, clientPath, "arm64");
        }


        private static void CopyMacFiles(string extractPath, string clientPath)
        {
            Console.WriteLine("Copying Mac Client Files");

            //
            // Copy the static files from the SDK Directory.  These files change less often
            //
            string destPath = Path.Combine(clientPath, "mac");
            string sourcePath = Path.Combine(extractPath, "mac");
            CopyExtractedFile(destPath, sourcePath, "bin/detection/release","libwaaddon.dylib");
            CopyExtractedFile(destPath, sourcePath, "bin/detection/release", "libwaapi.dylib");
            CopyExtractedFile(destPath, sourcePath, "bin/detection/release", "libwautils.dylib");
            CopyExtractedFile(destPath, sourcePath, "bin/manageability/release", "libwalocal.dylib");
            CopyExtractedFile(destPath, sourcePath, "bin/vulnerability/release", "libwavmodapi.dylib");
            CopyExtractedFile(destPath, sourcePath, "bin/deviceinfo/release", "libwadeviceinfo.dylib");

            //
            // Copy the dyanmic resource file.  This contains the version information - This file changes every 4 hours
            //
            string resourcePath = Path.Combine(extractPath, "compliance/mac/bin");
            CopyExtractedFile(destPath, resourcePath, "", "libwaresource.dylib");

            //
            // Copy the dynamic files - NOTE These are related to Patch and Vulnerability - This file changes every 4 hours
            //
            string patchCatalogPath = Path.Combine(extractPath, "analog/client");
            CopyExtractedFile(destPath, patchCatalogPath, "", "ap_checksum_mac.dat");
            CopyExtractedFile(destPath, patchCatalogPath, "", "patch_mac.dat");
            CopyExtractedFile(destPath, patchCatalogPath, "", "v2mod.dat");
            CopyExtractedFile(destPath, patchCatalogPath, "", "mav.dat");
        }

        private static void CopyLinuxArchitecture(string extractPath, string clientPath, string arch)
        {
            Console.WriteLine("Copying Linux Client Files with Architecture: " + arch);

            //
            // Copy the static files from the SDK Directory.  These files change less often
            //
            string destPath = Path.Combine(clientPath, "linux/" + arch);
            string sourcePath = Path.Combine(extractPath, "linux");
            CopyExtractedFolder(destPath, sourcePath, "bin/detection/" + arch + "/release");
            CopyExtractedFolder(destPath, sourcePath, "bin/deviceinfo/" + arch + "/release");
            CopyExtractedFolder(destPath, sourcePath, "bin/infection/" + arch + "/release");
            CopyExtractedFolder(destPath, sourcePath, "bin/manageability/" + arch + "/release");

            if (arch != "arm64")
            {
                CopyExtractedFolder(destPath, sourcePath, "bin/vulnerability/" + arch + "/release");
            }

            //
            // Copy the dyanmic resource file.  This contains the version information - This file changes every 4 hours
            //
            string resourcePath = Path.Combine(extractPath, "compliance/linux/bin/" + arch);
            CopyExtractedFile(destPath, resourcePath, "", "libwaresource.so");

            //
            // Copy the dynamic files - NOTE These are related to Patch and Vulnerability - This file changes every 4 hours
            //
            string patchCatalogPath = Path.Combine(extractPath, "analog/client");
            CopyExtractedFile(destPath, patchCatalogPath, "", "ap_checksum.dat");
            CopyExtractedFile(destPath, patchCatalogPath, "", "patch_linux.dat");
            CopyExtractedFile(destPath, patchCatalogPath, "", "v2mod.dat");
            CopyExtractedFile(destPath, patchCatalogPath, "", "liv.dat");
        }


        private static void CopyLinuxFiles(string extractDir, string clientPath)
        {
            CopyLinuxArchitecture(extractDir, clientPath, "x64");
            CopyLinuxArchitecture(extractDir, clientPath, "x86");
            CopyLinuxArchitecture(extractDir, clientPath, "arm64");
        }



        public static void PrepareFiles(string sdkRoot)
        {
            string clientPath = Util.GetClientPath(sdkRoot);
            string extractPath = Util.GetExtractPath(sdkRoot);

            CopyWindowsFiles(extractPath, clientPath);
            CopyMacFiles(extractPath, clientPath);
            CopyLinuxFiles(extractPath, clientPath);
        }
    }
}
