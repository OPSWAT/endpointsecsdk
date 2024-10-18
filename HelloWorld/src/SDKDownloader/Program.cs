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
using System.Threading;

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
            
            if(Directory.Exists(dir))
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
            if(ValidateCommandLine(args))
            {
                string command = args[0];
                string architecture = "x64";

                if(args.Length > 2)
                {
                    if (args[2] == "x86")
                    {
                        architecture = "win32";
                    }
                }

                switch(command)
                {
                    case "download":
                        {
                            string archivePath = args[1];
                            archivePath = Path.Combine(archivePath, args[2]);
                            string firstFile = FindFirstFile(archivePath);

                            if (!IsFileUpdated(firstFile))
                            {
                                Util.CreateCleanDir(archivePath);
                                DownloadSDK.Download(archivePath);
                            }
                            break;
                        }
                    case "download-vapm":
                        {
                            string archivePath = args[1];
                            string firstFile = FindFirstFile(archivePath);

                            if (!IsFileUpdated(firstFile))
                            {
                                Util.CreateCleanDir(archivePath);
                                DownloadCatalog.Download(archivePath);
                            }
                            break;
                        }
                    case "download-copy-vapm-windows":
                        {
                            Console.WriteLine("Copying vapm files");
                            string libPath = args[1];
                            string checkFilePath = Path.Combine(libPath, "v2mod.dat");

                            if (!IsFileUpdated(checkFilePath))
                            {
                                Util.CreateCleanDir(libPath);
                                ExtractorCatalog.DownloadAndCopy(libPath,1, architecture); // Download Windows Platform
                            }

                            break;
                        }
                    case "download-copy-windows":
                        {
                            Console.WriteLine("Copying windows files");
                            string libPath = args[1];
                            libPath = Path.Combine(libPath, args[2]);
                            string checkFilePath = Path.Combine(libPath, "libwavmodapi.dll"); 

                            if(!IsFileUpdated(checkFilePath))
                            {
                                Util.CreateCleanDir(libPath);
                                ExtractorSDK.DownloadAndCopy(libPath,1,architecture);
                            }

                            break;
                        }
                }
            }

            return 0;
        }
    }
}
