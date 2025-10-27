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
    internal class Program
    {
      
        static int Main(string[] args)
        {
            Console.WriteLine("SDKDownloader Started");

            string sdkRoot = Util.GetSDKRoot();
            sdkRoot = Path.Combine(sdkRoot, "OPSWAT-SDK");


            if (args.Length > 1)
            {
                sdkRoot = args[1];
                sdkRoot = Path.Combine(sdkRoot, args[2]);
            }

            //
            // Check to see if the downloaded files are up to date only download once a day
            //
            bool update= true;
            string analogHeader = Path.Combine(sdkRoot, "extract/analog/header.json");
            FileInfo analogHeaderInfo = new FileInfo(analogHeader);
            if (analogHeaderInfo.Exists)
            {
                if(analogHeaderInfo.LastWriteTime.AddDays(1) > DateTime.Now)
                {
                    update = false;
                }


            }


            if (update)
            {
                Downloader.Download(sdkRoot);
                Extractor.Extract(sdkRoot);
                ClientFiles.PrepareFiles(sdkRoot);

                //
                // Cleanup the Archives directory
                //
                Directory.Delete(Util.GetArchivesPath(sdkRoot), true);
            }


            Console.WriteLine("SDKDownloader Complete");
            return 0;
        }
    }
}
