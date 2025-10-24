using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPMAdapter.Moby
{
    public class MobyPythonRunner
    {
        public static void RunScript(string scriptPath)
        {
            //this is what we need to test if it works
            // Create process info
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "python"; // Use 'python' directly assuming it's in the system PATH
            start.Arguments = scriptPath;
            start.UseShellExecute = false;       // Required to redirect output
            start.RedirectStandardOutput = true; // Capture stdout
            start.RedirectStandardError = true;  // Capture stderr for error handling

            using (Process process = Process.Start(start))
            {
                // Read the standard error of the script to handle errors
                using (StreamReader errorReader = process.StandardError)
                {
                    string error = errorReader.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception($"Error running script: {error}");
                    }
                }
            }
        }
    }
}
