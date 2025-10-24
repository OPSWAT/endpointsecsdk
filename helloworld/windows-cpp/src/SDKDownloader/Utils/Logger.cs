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
    internal class Logger
    {
        public static void Log(string message)
        {
            StreamWriter writer = File.AppendText("Downloader.log");
            writer.WriteLine(message);
            writer.Close();
        }

        public static void Log(string formatableMessage, params object[] values)
        {
            Log(string.Format(formatableMessage, values));
        }
    }
}
