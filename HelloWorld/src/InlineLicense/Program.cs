using Compliance;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InlineLicense
{
    internal class Program
    {

        private static void InitializeFramework()
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string passKeyPath = Path.Combine(currentPath, "pass_key.txt");
            string licensePath = Path.Combine(currentPath, "license.cfg");

            // Check to makes sure the license is in the directory
            if (!File.Exists(passKeyPath))
            {
                Console.WriteLine("Could not find a pass_key.txt file.  Make sure that the license provided during evaluation is in the 'solutionRoot/license' directory.  That will compy the file to each sample code.");
                throw new Exception("License pass_key.txt file not found");
            }

            // Check to makes sure the license is in the directory
            if (!File.Exists(licensePath))
            {
                Console.WriteLine("Could not find a license.cfg file.  Make sure that the license provided during evaluation is in the 'solutionRoot/license' directory.  That will compy the file to each sample code.");
                throw new Exception("License license.cfg file not found");
            }

       


            // This code is used to initialize the OESIS Framework
            // The folling linkk describes the setup
            // https://software.opswat.com/OESIS_V4/html/c_sdk.html
            //
            string passkey = File.ReadAllText(passKeyPath);

            string licenseJson = File.ReadAllText(licensePath);
            dynamic jsonOut = JObject.Parse(licenseJson);
            string license = jsonOut.license_key;

            string config = "{ \"config\" : { \"license_key_bytes\":\"" + license + "\",\"passkey_string\": \"" + passkey + "\", \"enable_pretty_print\": true, \"online_mode\": true, \"silent_mode\": true } }";


            IntPtr outPtr = IntPtr.Zero;
            // Note if you get a Bad Image exception, that may be because Prefer 32-bit is checked
            int rc = OESISAdapter.wa_api_setup(config, out outPtr);
            string json_out = "{ }";
            if (outPtr != IntPtr.Zero)
            {
                json_out = XStringMarshaler.PtrToString(outPtr);
                OESISAdapter.wa_api_free(outPtr);
            }
            else
            {
                Console.Out.WriteLine("Failed to initialize OESIS: " + rc);
                // Refer to the following doc for errors:  https://software.opswat.com/OESIS_V4/html/c_return_codes.html

                throw new Exception("Failed to initialize");
            }
        }


        static void Main(string[] args)
        {
            try
            {
                InitializeFramework();
                Console.WriteLine("Success");
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed Initialization: " + e);
            }

        }
    }
}
