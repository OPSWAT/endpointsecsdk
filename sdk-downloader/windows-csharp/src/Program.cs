///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OESIS Framework
///  
///  Created by Chris Seiler
///  OPSWAT OEM Field CTO
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;

namespace SDKDownloader
{
    internal class Program
    {

        static int Main(string[] args)
        {
            Console.WriteLine("SDKDownloader Started");

            // --clean erases the OPSWAT-SDK directory first and downloads a clean version.
            bool clean = args.Contains("--clean");
            string[] rest = args.Where(a => a != "--clean").ToArray();

            string sdkRoot = Util.GetSDKRoot();
            sdkRoot = Path.Combine(sdkRoot, "OPSWAT-SDK");


            if (rest.Length > 1)
            {
                sdkRoot = rest[1];
                if (rest.Length > 2)
                    sdkRoot = Path.Combine(sdkRoot, rest[2]);
            }

            if (clean && Directory.Exists(sdkRoot))
            {
                Console.WriteLine("--clean: erasing " + sdkRoot + " for a fresh, clean download");
                Directory.Delete(sdkRoot, true);
            }

            //
            // The per-file download status (download_status.json) governs what is actually
            // re-downloaded, so we always run and let it skip files whose size hasn't changed -
            // only new data is downloaded and extracted.
            //
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
