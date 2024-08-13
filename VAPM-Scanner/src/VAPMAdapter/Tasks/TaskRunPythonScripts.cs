using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPMAdapter.Moby;

namespace VAPMAdapter.Tasks
{
    public class TaskRunPythonScripts
    {
        public static JObject Execute(string pythonScript)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string targetPath = Path.Combine(basePath, @"SanityChecks");
            string fullPath = Path.GetFullPath(targetPath);    
            // Append the script name to the base path for the script
            string scriptPath = Path.Combine(fullPath, pythonScript + ".py");
            string jsonPath = Path.Combine(basePath, pythonScript + ".json");

            // Delete the existing JSON file if it exists
            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }

            // Create an instance of MobyPythonRunner
            MobyPythonRunner pythonRunner = new MobyPythonRunner();

            // Run the script
            MobyPythonRunner.RunScript(scriptPath);

            // Read and return the contents of the produced JSON file
            if (File.Exists(jsonPath))
            {
                string jsonContent = File.ReadAllText(jsonPath);
                JObject jsonObject = JObject.Parse(jsonContent);
                return jsonObject;
            }
            else
            {
                throw new FileNotFoundException("The JSON file was not found after running the script.");
            }
        }
    }
}
