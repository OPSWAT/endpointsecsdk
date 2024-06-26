﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPMAdapter.Tasks
{
    internal class TaskRunPythonScripts
    {
        private string _basePath;

        public TaskRunPythonScripts(string basePath)
        {
            _basePath = basePath;
        }

        public string Execute(string pythonScript)
        {
            // Append the script name to the base path for both the script and the executable
            string scriptPath = Path.Combine(_basePath, pythonScript + ".py");
            //check to see if this actually works
            string exePath = Path.Combine(_basePath, "python.exe");
            string jsonPath = Path.Combine(_basePath, pythonScript + ".json");

            // Delete the existing JSON file if it exists
            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }

            // Create an instance of PythonRunner
            Moby.MobyPythonRunner pythonRunner = new Moby.MobyPythonRunner(exePath);

            // Run the script
            pythonRunner.RunScript(scriptPath);

            // Read and return the contents of the produced JSON file
            if (File.Exists(jsonPath))
            {
                string jsonContent = File.ReadAllText(jsonPath);
                return jsonContent;
            }
            else
            {
                throw new FileNotFoundException("The JSON file was not found after running the script.");
            }
        }
    }
}
