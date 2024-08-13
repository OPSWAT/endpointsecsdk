﻿///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Vulnerability
{
    internal class Program
    {

        private static void InitializeFramework()
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string passKeyPath = Path.Combine(currentPath, "pass_key.txt");

            // Check to makes sure the license is in the directory
            if(!File.Exists(passKeyPath))
            {
                Console.WriteLine("Could not find a pass_key.txt file.  Make sure that the license provided during evaluation is in the 'solutionRoot/license' directory.  That will compy the file to each sample code.");
                throw new Exception("License pass_key.txt file not found");
            }


            // This code is used to initialize the OESIS Framework
            // The folling linkk describes the setup
            // https://software.opswat.com/OESIS_V4/html/c_sdk.html
            //
            string passkey = File.ReadAllText(passKeyPath);
            string config = "{ \"config\" : { \"passkey_string\": \"" + passkey + "\", \"enable_pretty_print\": true, \"online_mode\": true, \"silent_mode\": true } }";


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




        // 
        // This will return JSON for all of the products found in the system
        // https://software.opswat.com/OESIS_V4/html/c_method.html
        // on the left select OESIS Core/Discover Products
        //
        //
        // All Categories
        /* Defines common product classification.This classification includes all product categories

         #define WAAPI_CATEGORY_ALL 0
                 Public File Sharing
                 Defines public file sharing product classification

         #define WAAPI_CATEGORY_PUBLIC_FILE_SHARING 1
                 Backup
         Defines backup client product classification

         #define WAAPI_CATEGORY_BACKUP_CLIENT 2
         Encryption
         Defines disk encryption product classification

         #define WAAPI_CATEGORY_DISK_ENCRYPTION 3
         Antiphishing
         Defines antiphishing product classification

          #define WAAPI_CATEGORY_ANTIPHISHING 4
         Antimalware
         Defines antimalware product classification

         #define WAAPI_CATEGORY_ANTIMALWARE 5
         Browser
         Defines browser product classification

          #define WAAPI_CATEGORY_BROWSER 6
         Firewall
         Defines firewall product classification

         #define WAAPI_CATEGORY_FIREWALL 7
         Messenger
         Defines instant messenger product classification

         #define WAAPI_CATEGORY_INSTANT_MESSENGER 8
         Cloud Storage
         Defines cloud storage product classification

          #define WAAPI_CATEGORY_CLOUD_STORAGE 9
         Data Loss Prevention
         Defines data loss prevention product classification

         #define WAAPI_CATEGORY_DATA_LOSS_PREVENTION 11
         Patch Management
         Defines patch management product classification

          #define WAAPI_CATEGORY_PATCH_MANAGEMENT 12
         VPN Client
         Defines VPN client product classification

         #define WAAPI_CATEGORY_VPN_CLIENT 13
         Virtual Machine
         Defines Virtual Machine product classification

          #define WAAPI_CATEGORY_VIRTUAL_MACHINE 14
         Health Agent
         Defines health agent product classification

         #define WAAPI_CATEGORY_HEALTH_AGENT 15
         Remote Desktop Control
         Defines remote desktop control classification

         #define WAAPI_CATEGORY_REMOTE_CONTROL 16
         P2P Agent
         Defines a peer to peer application classification

          #define WAAPI_CATEGORY_PEER_TO_PEER 17
         Web Conference
         Defines that the product has an online video and audio conferencing classification

          #define WAAPI_CATEGORY_WEB_CONFERENCE 18
         Unclassified
         Defines that the product does not have an official classification

         #define WAAPI_CATEGORY_UNCLASSIFIED 10*/
        public static int DetectProducts(int category, out string json_out)
        {
            int result = 0;
            string json_in = "{\"input\": { \"method\": 0, \"category\": " + category + " } }";
            result = Invoke(json_in, out json_out);
            return result;
        }


        // 
        // This will return JSON details on whether a firewall is enabled
        // https://software.opswat.com/OESIS_V4/html/c_method.html
        // on the left select Manageability/GetFirewallState
        public static bool IsFirewallRunning(int signature)
        {
            bool result = false;
            string json_in = "{\"input\": { \"method\": 1007 \"signature\":" + signature + " } }";
            string json_out = "";
            int callResult = Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
                result = parsedObject["result"]["enabled"];
            }

            return result;
        }


        // Note this is just passing in the json_result to be able to print it out on a failure
        private static void CheckSuccess(int rc)
        {
            // Return code link: https://software.opswat.com/OESIS_V4/html/c_return_codes.html
            // Note any code above 0 is a SUCCESS
            if (rc < 0)
            {
                Console.Out.WriteLine("Failed to execute call: " + rc);
                throw new Exception();
            }
        }


        // This is the main call used to send JSON in and out of the API
        private static int Invoke(string json_config, out string json_out)
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

        // Expects JSON from DETECT Products
        public static List<Product> GetProductList(string detect_product_json)
        {
            List<Product> result = new List<Product>();

            dynamic jsonOut = JObject.Parse(detect_product_json);
            var products = jsonOut.result.detected_products;

            for (int i = 0; i < products.Count; i++)
            {
                Product newProduct = new Product();
                newProduct.signatureId = products[i].signature;
                newProduct.name = (string)products[i].product.name;
                newProduct.vendor = (string)products[i].vendor.name;
                result.Add(newProduct);
            }

            return result;
        }



        // 
        // This will return JSON details on whether a firewall is enabled
        // https://software.opswat.com/OESIS_V4/html/c_method.html
        // on the left select Manageability/GetFirewallState
        public static bool GetDeviceIdentity()
        {
            bool result = false;
            string json_in = "{\"input\": { \"method\": 30010 } }";
            string json_out = "";
            int callResult = Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                dynamic parsedObject = JObject.Parse(json_out);
                result = parsedObject["result"]["enabled"];
            }

            return result;
        }




        static void Main(string[] args)
        {
            string products_json = "";
            try
            {
                // Setup the default initialization
                InitializeFramework();


                // Detect all of the products
                // Note using the 7 which maps to the Firewall Category
                Console.WriteLine("Discovering Firewall Products");
                CheckSuccess(DetectProducts(7, out products_json));

                GetDeviceIdentity();

                List<Product> productList = GetProductList(products_json);
                foreach(Product product in productList)
                {
                    // Each product is identified with a product ID and signature ID.  The signature ID is the one to use
                    // to look up different details.
                    bool firewallRunning = IsFirewallRunning(product.signatureId);
                    Console.WriteLine("Found: " + product.name + "  Running: " + firewallRunning);
                }

                OESISAdapter.wa_api_teardown();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Received an Exception: " + e);
                Console.Out.WriteLine("JSON_RESULT: " + products_json);
            }
        }
    }
}
