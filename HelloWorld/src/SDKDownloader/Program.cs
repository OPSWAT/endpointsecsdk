///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

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
            
            if(Directory.Exists(dir))
            {
                string[] fileList = Directory.GetFiles(dir);
                result = fileList[0];
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


        static void Main(string[] args)
        {
            if(ValidateCommandLine(args))
            {
                string command = args[0];

                switch(command)
                {
                    case "download":
                        {
                            string archivePath = args[1];
                            string firstFile = FindFirstFile(archivePath);

                            if (!IsFileUpdated(firstFile))
                            {
                                if (Directory.Exists(archivePath))
                                {
                                    Directory.Delete(archivePath, true);
                                }
                                Directory.CreateDirectory(archivePath);


                                DownloadSDK.DownloadAllSDKFiles(archivePath);
                            }

                            DownloadSDK.DownloadAllSDKFiles(archivePath);
                            break;
                        }
                    case "extract":
                        {
                            string archivePath = args[1];
                            string sdkPath = args[2];

                            DownloadSDK.DownloadAllSDKFiles(archivePath);
                            break;
                        }
                    case "copylibs":
                        {
                            string sdkPath = args[1];
                            string libPath = args[2];

                            SDKExtractor.CopyAllLibFiles(sdkPath, libPath);
                            break;
                        }
                    case "download-copy":
                        {
                            string libPath = args[1];
                            string checkFilePath = Path.Combine(libPath, "libwavmodapi.dll"); 

                            if(!IsFileUpdated(checkFilePath))
                            {
                                if(!Directory.Exists(libPath))
                                {
                                    Directory.CreateDirectory(libPath);
                                }

                                SDKExtractor.DownloadAndCopy(libPath);
                            }

                            break;
                        }
                }
            }
        }
    }
}
