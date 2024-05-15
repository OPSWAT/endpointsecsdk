///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using ComplianceAdapater.POCO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ComplianceAdapater.OESIS
{
    public class OESISFramework
    {
        public static void InitializeFramework()
        {
            // This code is used to initialize the OESIS Framework
            // The folling linkk describes the setup
            // https://software.opswat.com/OESIS_V4/html/c_sdk.html
            //

            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            
            //
            // Check to make sure there is a license.cfg and a pass_key.txt
            //
            if(!File.Exists("pass_key.txt") || !File.Exists("license.cfg"))
            {
                throw new Exception("License Files Missing!!! \n\nPlease ensure that pass_key.txt and license.cfg are in the running directory: " + Environment.CurrentDirectory);
            }
            
            string passkey = File.ReadAllText(path + "/pass_key.txt");
            string config = "{ \"config\" : { \"passkey_string\": \"" + passkey + "\", \"enable_pretty_print\": true, \"online_mode\": false, \"silent_mode\": true } }";


            IntPtr outPtr = IntPtr.Zero;
            // Note if you get a Bad Image exception, that may be because Prefer 32-bit is checked
            int rc = OESISAdapter.wa_api_setup(config, out outPtr);
            string json_out = "{ }";
            if (outPtr != IntPtr.Zero)
            {

                if (rc < 0)
                {

                    json_out = XStringMarshaler.PtrToString(outPtr);

                    //
                    // Now get the error list
                    //
                    List<SetupErrorDetail> errorList = Util.GetInitializationErrors(json_out);
                    OESISAdapter.wa_api_free(outPtr);


                    //
                    // Iterate and make sure the license is valid
                    //
                    foreach (SetupErrorDetail errorDetail in errorList)
                    {
                        if (errorDetail.module == "libwalocal.dll") // This is the compliance module
                        {
                            if (errorDetail.code < 0)
                            {
                                if (errorDetail.code == -8)
                                {
                                    throw new Exception("Licensing Error.  Check the license to make sure it has not expired");
                                }
                                else
                                {
                                    throw new Exception("Error occurred with Compliance initialization: " + errorDetail.code);
                                }
                            }
                        }
                    }

                    throw new Exception("Failed initilization: " + rc + "\n\n" + json_out);
                }

            }
            else
            {
                Console.Out.WriteLine("Failed to initialize OESIS: " + rc);
                // Refer to the following doc for errors:  https://software.opswat.com/OESIS_V4/html/c_return_codes.html

                throw new Exception("Failed to initialize");
            }
        }




        // This is the main call used to send JSON in and out of the API
        public static int Invoke(string json_config, out string json_out)
        {
            IntPtr outPtr = IntPtr.Zero;
            int rc = OESISAdapter.wa_api_invoke(json_config, out outPtr);
            json_out = "{ }";
            if (outPtr != IntPtr.Zero)
            {
                json_out = XStringMarshaler.PtrToString(outPtr);
                OESISAdapter.wa_api_free(outPtr);
            }
            return rc;
        }

        public static void TearDown()
        {
            OESISAdapter.wa_api_teardown();
        }


    }
}
