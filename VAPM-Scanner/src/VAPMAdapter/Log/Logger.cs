///////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////



using System;
using System.IO;
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT Endpoint SDK Patch and Vulnerability Modules
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
namespace VAPMAdapater.Log
{
    public class Logger
    {
        public static void Log(string message)
        {
            StreamWriter writer = File.AppendText("Scanner.log");
            writer.WriteLine(message);
            writer.Close();
        }

        public static void Log(string formatableMessage, params object[] values)
        {
            Log(string.Format(formatableMessage, values));
        }
    }
}
