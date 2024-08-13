///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using Vulnerability;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;

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
            if (File.Exists(licensePath))
            {
                File.Delete("license.hide");
                // Doing this to make sure the code does not accidentally load the license on the file system.
                File.Move(licensePath, "license.hide");
            }

       


            // This code is used to initialize the OESIS Framework
            // The folling linkk describes the setup
            // https://software.opswat.com/OESIS_V4/html/c_sdk.html
            //
            string passkey = File.ReadAllText(passKeyPath);

            string licenseJson = File.ReadAllText("license.hide");
            dynamic jsonOut = JObject.Parse(licenseJson);
            string licenseKey = jsonOut.license_key;
            string license = jsonOut.license;

       
            string config = "{ \"config\" : { \"license_bytes\":\"" + license + "\",\"license_key_bytes\":\"" + licenseKey + "\",\"passkey_string\": \"" + passkey + "\", \"enable_pretty_print\": true, \"online_mode\": true, \"silent_mode\": true } }";


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
