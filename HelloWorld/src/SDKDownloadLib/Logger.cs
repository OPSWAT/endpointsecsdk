using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
