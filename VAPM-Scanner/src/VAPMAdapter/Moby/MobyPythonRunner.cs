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
        private string _pythonExePath;

        public MobyPythonRunner(string pythonExePath)
        {
            _pythonExePath = pythonExePath;
        }

        public string RunScript(string scriptPath)
        {
            // Create process info
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = _pythonExePath;
            start.Arguments = scriptPath;
            start.UseShellExecute = false;       // Required to redirect output
            start.RedirectStandardOutput = true; // Capture stdout

            using (Process process = Process.Start(start))
            {
                // Read the standard output of the script
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    return result;
                }
            }
        }
    }
}
