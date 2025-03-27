///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using SDKDownloadLib;
using System;
using System.IO;

namespace SDKDownloader
{
    internal class Program
    {

        private static bool IsFileUpdated(string checkFile)
        {
            bool result = false;

            if (File.Exists(checkFile))
            {
                FileInfo checkFileInfo = new FileInfo(checkFile);

                //
                // Update the SDK every 7 days
                //
                if (checkFileInfo.LastWriteTime > DateTime.Now.AddDays(-7))
                {
                    result = true;
                }
            }

            return result;
        }

        private static string FindFirstFile(string dir)
        {
            string result = null;

            if (Directory.Exists(dir))
            {
                string[] fileList = Directory.GetFiles(dir);
                if (fileList != null && fileList.Length > 0)
                {
                    result = fileList[0];
                }
            }

            return result;
        }

        private void PrintUsage()
        {
            Console.WriteLine("To use this application specify the command and the arguments.  Will only download the files if the archive file is 7 days or older");
            Console.WriteLine("");

            Console.WriteLine("download-copy - Downloads the latest SDK files and extracts just the 64 bit binaries to the parameter specified.");
            Console.WriteLine("     Example: SDKDownloader download-copy lib");
            Console.WriteLine("");

            Console.WriteLine("download - Downloads the latest SDK and database files for Windows to specific archive directory.  NOTE: Directory will be cleaned");
            Console.WriteLine("     Example: SDKDownloader download archives");
            Console.WriteLine("");

            Console.WriteLine("extract - Extracts archive files to using archive path to extracted directory.  NOTE: Directory will be cleaned");
            Console.WriteLine("     Example: SDKDownloader extract archives SDK");
            Console.WriteLine("");

            Console.WriteLine("copylibs - Extracts lib files from extracted path to lib directory.  NOTE: Directory will be cleaned");
            Console.WriteLine("     Example: SDKDownloader copylibs SDK lib");
            Console.WriteLine("");

        }

        static bool ValidateCommandLine(string[] args)
        {
            return true;
        }


        static int Main(string[] args)
        {
            Console.WriteLine("SDKDownloader Started");
            if (ValidateCommandLine(args))
            {
                string command = args[0];

                switch (command)
                {
                    case "clean":
                        {
                            string archivePath = args[1];
                            string sdkFolder = Path.Combine(archivePath, "sdk");
                            string vapmFolder = Path.Combine(archivePath, "vapm");

                            Directory.Delete(sdkFolder, true);
                            Directory.Delete(vapmFolder, true);
                            break;
                        }
                    case "download-windows":
                        {
                            string archivePath = args[1];
                            DownloadSDK.Download(archivePath);
                            DownloadCatalog.Download(archivePath);
                            break;
                        }
                    case "download-copy-files":
                        {
                            string rootPath = args[1];
                            string rootArch = Path.Combine(rootPath, args[1]);
                            string vapmDir = Path.Combine(rootPath, "vapm");
                            string firstFile = Path.Combine(vapmDir, "v2mod.dat");

                            if (!IsFileUpdated(firstFile))
                            {
                                ExtractorCatalog.DownloadAndCopy(rootPath); // Download Windows Platform
                                ExtractorSDK.DownloadAndCopy(rootPath);
                            }
                            break;
                        }
                }

                Console.WriteLine("SDKDownloader Complete");
            }

            return 0;
        }
    }
}
