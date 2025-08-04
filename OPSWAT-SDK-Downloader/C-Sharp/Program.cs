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

            string sdkRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            sdkRoot = Path.Combine(sdkRoot, "OPSWAT-SDK");


            if (args.Length > 1)
            {
                sdkRoot = args[1];
                sdkRoot = Path.Combine(sdkRoot, args[2]);
            }

            Downloader.Download(sdkRoot);
            Extractor.Extract(sdkRoot);
            ClientFiles.PrepareFiles(sdkRoot);

            //
            // Cleanup the Archives directory
            //
            Directory.Delete(Util.GetArchivesPath(sdkRoot), true);

            Console.WriteLine("SDKDownloader Complete");
            return 0;
        }
    }
}
