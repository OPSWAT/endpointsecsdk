// using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;
// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.IO;
// using System.Reflection;
// using System.Xml.Schema;
// using Vulnerability;


// namespace Products
// {
//     internal class Program
//     {
//         private static void InitializeFramework()
//         {
//             string passkeypath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "pass_key.txt");
//             if (!File.Exists(passkeypath))
//             {
//                 Console.WriteLine("Pass_key.txt file not found in the directory");
//                 throw new Exception("Pass_key.txt not found");
//             }

//             string keypass = File.ReadAllText(passkeypath);
//             string json_config = "{\"config\": { \"passkey_string\":\"" + keypass + "\", \"enable_pretty_print\": true, \"silent_mode\": true}}";
//             string json_out = "{ }";

//             IntPtr outPtr = IntPtr.Zero;
//             int return_codes = OESISAdapter.wa_api_setup(json_config, out outPtr);
//             if (outPtr != IntPtr.Zero)
//             {
//                 json_out = XStringMarshaler.PtrToString(outPtr);
//              }
//             else
//             {
//                 Console.WriteLine("Fail to initialize OESIS " + return_codes);

//                 throw new Exception("Fail to initialize");
//             }
//             OESISAdapter.wa_api_free(outPtr);
//         }

//         private static string JsonStructure(int methodNumber, string SecondArgumentName, string SecondArgumentValue)
//         {
//             string jsonConfig;
//             if (SecondArgumentName == "" && SecondArgumentValue == "")
//             {
//                 jsonConfig = $"{{ \"input\": {{ \"method\": {methodNumber} }} }}";
//                 return jsonConfig;
//             }
//             string safeValue = SecondArgumentValue.Replace(@"\", @"\\");

//             bool isNumeric = int.TryParse(SecondArgumentValue, out _);

//             if (isNumeric)
//             {
//                 jsonConfig = $"{{ \"input\": {{ \"method\": {methodNumber}, \"{SecondArgumentName}\": {safeValue} }} }}";
//             }
//             else
//             {
//                 jsonConfig = $"{{ \"input\": {{ \"method\": {methodNumber}, \"{SecondArgumentName}\": \"{safeValue}\" }} }}";
//             }

//             return jsonConfig;
//         }

//         private static void DetectInstalledProducts(int category_number, out List<Product> products_json)
//         {

//             string json_out;
//             int json_config = ApiInvoke(0, "category", category_number.ToString(), out json_out);

//             dynamic jsonOut = JObject.Parse(json_out);
//             var products = jsonOut.result.detected_products;
//             Console.WriteLine(products);
//             List<Product> productList = new List<Product>();
//             foreach (var prod in products)
//             {
//                 Product newProduct = new Product();
//                 newProduct.signatureId = (int)prod.signature;
//                 newProduct.name = (string)prod.product.name;
//                 newProduct.vendor = (string)prod.vendor.name;
//                 newProduct.sig_name = (string)prod.sig_name;

//                 //Getting product version
//                 string version;
//                 int versionStatus = ApiInvoke(100, "signature", (newProduct.signatureId).ToString(), out version);
//                 dynamic version_info = JObject.Parse(version);
//                 string versionNumber = (string)version_info.result.version;
//                 newProduct.version = versionNumber;

//                 // Checking product vulnerability
//                 string vulnerability_info;
//                 int vulnerabilityStatus = ApiInvoke(50505, "signature", (newProduct.signatureId).ToString(), out vulnerability_info);
//                 dynamic vulnerability = JObject.Parse(vulnerability_info);
//                 var results = vulnerability.result;
//                 if (results.has_vulnerability == true)
//                 {
//                     newProduct.vulnerability = (string)results.cves.severity;
//                     //newProduct.vulnerability_detail = (string)vulnerabilityStatus.cves;
//                 }
//                 else
//                 {
//                     newProduct.vulnerability = "Clean";
//                 }


//                 productList.Add(newProduct);
//             }
//             products_json = productList;

//         }

//         private static void CheckRunningApps(out List<RunningProducts> products_json)
//         {
//             string jsonOutput;
//             int jsonConfig = ApiInvoke(100001, "","", out jsonOutput);

//             dynamic jsonOut = JObject.Parse(jsonOutput);
//             var products = jsonOut.result.detected_products;
//             List<RunningProducts> productList = new List<RunningProducts>();
//             HashSet<string> seen = new HashSet<string>();
//             foreach (var prod in products)
//             {
//                 string appName = (string)prod.product.name;

//                 // Skip duplicates
//                 if (seen.Contains(appName))
//                 {
//                     continue;
//                 }

//                 RunningProducts newProduct = new RunningProducts();
//                 //TO-DO: Create cases where some properties are missing
//                 newProduct.name = (string)prod.product.name;
//                 newProduct.version = (string)prod.version;

//                 if (prod.running_processes != null)
//                 {
//                     newProduct.isRunning = true;
//                 }
//                 else
//                 {
//                     newProduct.isRunning = false;
//                 }

//                 productList.Add(newProduct);
//                 seen.Add(appName);
//             }
//             products_json = productList;
//         }

//         // result is the return code, while outPtr contains all the information that returns for that method choice
//         public static int ApiInvoke(int methodChoice, string SecondArgumentName, string SecondArgumentValue, out string json_out)
//         {
//             string json_config = JsonStructure(methodChoice, SecondArgumentName, SecondArgumentValue);
//             Console.WriteLine(json_config);
//             IntPtr outPtr = IntPtr.Zero;
//             json_out = "{ }";
//             int result = OESISAdapter.wa_api_invoke(json_config, out outPtr);

//             if (outPtr != IntPtr.Zero) // Checking outPtr is not null
//             {
//                 json_out = XStringMarshaler.PtrToString(outPtr);
//                 OESISAdapter.wa_api_free(outPtr);
//             }
//             else
//             {
//                 Console.Out.WriteLine($"Method {methodChoice} returned no output (result={result})");
//             }

//             // if (result < 0)
//             // {
//             //     throw new Exception("Method call failed to run correctly: " + result);
//             // }
//             return result;
//         }

//         private static void showMenu()
//         {
//             bool running = true;
//             Console.WriteLine("====OPSWAT Security SDK====");
//             Console.WriteLine("1. Detected all products");
//             Console.WriteLine("2. Detected running products");
//             Console.WriteLine("3. Detect all app needs update");
//             Console.WriteLine("4. Exit");
//             Console.Write("\nSelect choice: ");

//             string input = Console.ReadLine();
//             switch (input)
//             {
//                 case "1":
//                     List<Product> products_json = new List<Product>();
//                     DetectInstalledProducts(9, out products_json);
//                     foreach (var product in products_json)
//                     {
//                         Console.Out.WriteLine("Product Name: " + product.name);
//                         Console.Out.WriteLine("Product Signature ID: " + product.signatureId);
//                         Console.Out.WriteLine("Product Version: " + product.version);
//                         Console.Out.WriteLine("Product Vulnerability Status: " + product.vulnerability);
//                         //Console.Out.WriteLine("Product isRunning: " + product.isRunning);
//                         Console.Out.WriteLine("---------------------------------");
//                     }
//                     break;
//                 case "2":
//                     List<RunningProducts> running_products = new List<RunningProducts>();
//                     CheckRunningApps(out running_products);
//                     int running_count = 0;
//                     int total_app = 1;
//                     foreach (var product in running_products)
//                     {
//                         if (product.isRunning)
//                         {
//                             Console.Out.WriteLine("Product Name: " + product.name);
//                             Console.Out.WriteLine("Product Version: " + product.version);
//                             Console.Out.WriteLine("Product Running: " + product.isRunning);
//                             Console.Out.WriteLine("---------------------------------");
//                             running_count += 1;
//                         }
//                         total_app += 1;

//                     }
//                     Console.WriteLine("Total running product: " + running_count);
//                     Console.WriteLine("Total products detected: " + total_app);
//                     break;
//                 case "3":
//                     //PatchManagement();
//                     break;
//                 case "4":
//                     MissingPatches();
//                     break;
                
//                 default:
//                     Console.WriteLine("Invalid choice. Try again.");
//                     break;
//             };

//             if (running)
//             {
//                 Console.WriteLine("\nPress any key to return to menu...");
//                 Console.ReadKey();
//             }
//         }

//         static string TrimWithEllipsis(string text, int maxLength)
//         {
//             if (string.IsNullOrEmpty(text)) return text;
//             return text.Length > maxLength ? text.Substring(0, maxLength - 3) + "..." : text;
//         }

//         static void MissingPatches()
//         {
//             string json_out;
//             int json_config = ApiInvoke(0, "category", "0", out json_out);

//             dynamic jsonOut = JObject.Parse(json_out);
//             var products = jsonOut.result.detected_products;
//             Console.WriteLine(products);

//             foreach (var prod in products)
//             {
//                 int signature = prod.vendor.id;
//                 string jsonInput = "{ \"input\": { \"method\": 50500, \"signature\": " + signature + " }}";

//                 IntPtr outPtr = IntPtr.Zero;
//                 string json__out = "{}";

//                 int result = OESISAdapter.wa_api_invoke(jsonInput, out outPtr);

//                 if (outPtr != IntPtr.Zero)
//                 {
//                     json__out = XStringMarshaler.PtrToString(outPtr);
//                     OESISAdapter.wa_api_free(outPtr);
//                 }

//                 Console.WriteLine($"Result code: {result}");
//                 Console.WriteLine("=== JSON Output ===");
//                 Console.WriteLine(json__out);
//             }
//         }

//         // static void PatchManagement()
//         // {
//         //     ProcessStartInfo psi = new ProcessStartInfo
//         //     {
//         //         FileName = "winget",
//         //         Arguments = "list --upgrade-available",
//         //         RedirectStandardOutput = true,
//         //         RedirectStandardError = true,
//         //         UseShellExecute = false,
//         //         CreateNoWindow = true
//         //     };

//         //     using (Process process = new Process { StartInfo = psi })
//         //     {
//         //         process.Start();

//         //         string output = process.StandardOutput.ReadToEnd();
//         //         string errors = process.StandardError.ReadToEnd();

//         //         process.WaitForExit();

//         //         Console.OutputEncoding = System.Text.Encoding.UTF8;

//         //         if (!string.IsNullOrEmpty(errors))
//         //         {
//         //             Console.WriteLine("=== Errors ===");
//         //             Console.WriteLine(errors);
//         //             return;
//         //         }

//         //         // Split into lines
//         //         string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

//         //         if (lines.Length < 3)
//         //         {
//         //             Console.WriteLine("No upgrades available.");
//         //             return;
//         //         }

//         //         // Skip headers and dashed line (first 2 lines)
//         //         int startIndex = 2;

//         //         // Print our clean table
//         //         Console.WriteLine("=== Applications with Upgrades Available ===");
//         //         Console.WriteLine("------------------------------------------------------------------------------------------------------------------");
//         //         Console.WriteLine("{0,-40} {1,-30} {2,-15}", "Name", "Current Version", "Available Version");
//         //         Console.WriteLine("------------------------------------------------------------------------------------------------------------------");

//         //         for (int i = startIndex; i < lines.Length; i++)
//         //         {
//         //             string line = lines[i];

//         //             // Winget aligns columns with spaces → break by whitespace
//         //             string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

//         //             if (parts.Length < 5) continue; // not a valid row

//         //             string name = string.Join(" ", parts, 0, parts.Length - 4); // everything before Id
//         //             string version = parts[parts.Length - 3];
//         //             string available = parts[parts.Length - 2];
//         //             // string source = parts[parts.Length - 1]; // not shown

//         //             name = TrimWithEllipsis(name, 30);

//         //             Console.WriteLine("{0,-40} {1,-30} {2,-15}", name, version, available);
//         //         }  
//         //     }
//         // }


//         static void CallGetMissingPatches(int signatureId)
//         {
//             string jsonInput = "{ \"input\": { \"method\": 50300, \"signature\": " + signatureId + " }}";

//             IntPtr outPtr = IntPtr.Zero;
//             string json_out = "{}";

//             int result = OESISAdapter.wa_api_invoke(jsonInput, out outPtr);

//             if (outPtr != IntPtr.Zero)
//             {
//                 json_out = XStringMarshaler.PtrToString(outPtr);
//                 OESISAdapter.wa_api_free(outPtr);
//             }

//             Console.WriteLine($"Result code: {result}");
//             Console.WriteLine("=== JSON Output ===");
//             Console.WriteLine(json_out);
//         }
        
//         public static void LoadPatchDatabase(string databaseFile, string checksumFile)
//         {
//             IntPtr outPtr = IntPtr.Zero;
//             string json_out = "{}";
//             string json_in = "{\"input\" : {\"method\" : 50302, \"dat_input_source_file\" : \"" + databaseFile + "\"}}";

//             if (!string.IsNullOrEmpty(checksumFile))
//             {
//                 json_in = "{\"input\" : {\"method\" : 50302, \"dat_input_source_file\" : \"" + databaseFile + "\", \"dat_input_checksum_file\" : \"" + checksumFile + "\"}}";
//             }

//             int rc = OESISAdapter.wa_api_invoke(json_in, out outPtr);

//             if (outPtr != IntPtr.Zero)
//             {
//                 json_out = XStringMarshaler.PtrToString(outPtr);
//                 OESISAdapter.wa_api_free(outPtr);
//             }

//             Console.WriteLine(rc);
//             if (rc < 0)
//             {
//                 throw new Exception("LoadPatchDatabase failed to run correctly.  " + result);
//             }
//         }
        
//         static void Main(string[] args)
//         {
//             List<Product> products_json = new List<Product>();
//             List<RunningProducts> running_products = new List<RunningProducts>();
//             try
//             {
//                 InitializeFramework();
//                 LoadPatchDatabase("patch.dat","ap_checksum.dat");
//                 ApiInvoke(50520, "dat_input_source_file", "v2mod.dat", out _);

//                 CallGetMissingPatches(3039);

//                 showMenu();

//                 OESISAdapter.wa_api_teardown();
//             }
//             catch (Exception e)
//             {
//                 Console.Out.WriteLine("Received an Exception: " + e);
//                 Console.Out.WriteLine("JSON_RESULT: " + products_json);
//             }
//         }


//         }
//     }



// /* After calling the product, it immediately check for product's vulnerability rate. */
// /*research winget, how integrate to sdk. the best way to integrate, would it be better to use winget or sdk.*/
// /*
// {Method = {Int32 ApiInvoke(Int32, System.String, System.String, System.String ByRef)}}
// */

// 2 mode code

//
// MERGED CODE: OESIS SDK Scanner with AI Assistant
//
// This application combines the deep system scanning capabilities of the OESIS SDK
// with an AI-powered chatbot that can perform remediation actions using winget.
//
// Required Dependencies:
// 1. Newtonsoft.Json NuGet Package
// 2. Your custom OESISAdapter P/Invoke wrapper class.
// 3. Your custom XStringMarshaler class.
// 4. A valid "pass_key.txt" file in the same directory as the executable.
// 5. Patch database files: "patch.dat" and "ap_checksum.dat".
// 6. A valid OpenAI API Key.
//

///////////////////////////////////////////////////////////////////////////////////////////////
///  Refactored Sample Code for OESIS SDK
///  Implements product detection, running process checks, and patch management.
///  This version uses the specified original implementations for detection methods.
///////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Vulnerability;

namespace Products
{
    // The 'Product' and 'RunningProducts' classes are referenced from the 'Vulnerability' namespace.

    internal class Program
    {
        #region OESIS SDK Core Interaction

        /// <summary>
        /// Initializes the OESIS SDK framework with a passkey.
        /// </summary>
        private static void InitializeFramework()
        {
            string passkeypath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "pass_key.txt");
            if (!File.Exists(passkeypath))
            {
                throw new Exception("Pass_key.txt file not found in the directory.");
            }

            string keypass = File.ReadAllText(passkeypath).Trim();
            string json_config = "{\"config\": { \"passkey_string\":\"" + keypass + "\", \"enable_pretty_print\": true, \"silent_mode\": true}}";

            IntPtr outPtr = IntPtr.Zero;
            int return_codes = OESISAdapter.wa_api_setup(json_config, out outPtr);

            if (outPtr != IntPtr.Zero)
            {
                OESISAdapter.wa_api_free(outPtr);
            }

            if (return_codes < 0)
            {
                throw new Exception("Failed to initialize OESIS SDK. Return code: " + return_codes);
            }
        }

        /// <summary>
        /// Loads the patch management database required for method 50500.
        /// </summary>
        public static void LoadPatchDatabase(string databaseFile, string checksumFile)
        {
            Console.WriteLine("Loading patch management database...");
            string json_in = "{\"input\" : {\"method\" : 50302, \"dat_input_source_file\" : \"" + databaseFile + "\"}}";
            if (!string.IsNullOrEmpty(checksumFile))
            {
                json_in = "{\"input\" : {\"method\" : 50302, \"dat_input_source_file\" : \"" + databaseFile + "\", \"dat_input_checksum_file\" : \"" + checksumFile + "\"}}";
            }

            IntPtr outPtr = IntPtr.Zero;
            int rc = OESISAdapter.wa_api_invoke(json_in, out outPtr);

            if (outPtr != IntPtr.Zero)
            {
                OESISAdapter.wa_api_free(outPtr);
            }

            if (rc < 0)
            {
                Console.WriteLine($"[Warning] LoadPatchDatabase failed with code: {rc}. Patch management may not work.");
            }
            else
            {
                Console.WriteLine("Patch database loaded successfully.");
            }
        }


        /// <summary>
        /// Creates the JSON string for an API call.
        /// </summary>
        private static string JsonStructure(int methodNumber, string secondArgumentName, string secondArgumentValue)
        {
            if (string.IsNullOrEmpty(secondArgumentName) && string.IsNullOrEmpty(secondArgumentValue))
            {
                return $"{{ \"input\": {{ \"method\": {methodNumber} }} }}";
            }

            string safeValue = secondArgumentValue.Replace(@"\", @"\\");
            bool isNumeric = int.TryParse(secondArgumentValue, out _);

            if (isNumeric)
            {
                return $"{{ \"input\": {{ \"method\": {methodNumber}, \"{secondArgumentName}\": {safeValue} }} }}";
            }
            else
            {
                return $"{{ \"input\": {{ \"method\": {methodNumber}, \"{secondArgumentName}\": \"{safeValue}\" }} }}";
            }
        }

        /// <summary>
        /// A generic wrapper to invoke an OESIS SDK method.
        /// </summary>
        public static int ApiInvoke(int methodChoice, string secondArgumentName, string secondArgumentValue, out string json_out)
        {
            string json_config = JsonStructure(methodChoice, secondArgumentName, secondArgumentValue);
            IntPtr outPtr = IntPtr.Zero;
            json_out = "{ }";
            int result = OESISAdapter.wa_api_invoke(json_config, out outPtr);

            if (outPtr != IntPtr.Zero)
            {
                json_out = XStringMarshaler.PtrToString(outPtr);
                OESISAdapter.wa_api_free(outPtr);
            }
            else
            {
                Console.Out.WriteLine($"Method {methodChoice} returned no output (result={result})");
            }
            return result;
        }

        #endregion

        #region Application Features

        /// <summary>
        /// Scans for all installed products using the user-specified correct implementation.
        /// </summary>
        private static void DetectInstalledProducts(out List<Product> products_json)
        {

            string json_out;
            int json_config = ApiInvoke(0, "category", "0", out json_out);

            dynamic jsonOut = JObject.Parse(json_out);
            var products = jsonOut.result.detected_products;
            List<Product> productList = new List<Product>();
            foreach (var prod in products)
            {
                Product newProduct = new Product();
                newProduct.signatureId = (int)prod.signature;
                newProduct.name = (string)prod.product.name;
                newProduct.vendor = (string)prod.vendor.name;
                newProduct.sig_name = (string)prod.sig_name;

                //Getting product version
                string version;
                int versionStatus = ApiInvoke(100, "signature", (newProduct.signatureId).ToString(), out version);
                dynamic version_info = JObject.Parse(version);
                string versionNumber = (string)version_info.result.version;
                newProduct.version = versionNumber;

                productList.Add(newProduct);
            }
            products_json = productList;
        }

        /// <summary>
        /// Scans for running applications using the user-specified correct implementation.
        /// </summary>
        private static void CheckRunningApps(out List<RunningProducts> products_json)
        {
            string jsonOutput;
            int jsonConfig = ApiInvoke(100001, "", "", out jsonOutput);

            dynamic jsonOut = JObject.Parse(jsonOutput);
            var products = jsonOut.result.detected_products;
            List<RunningProducts> productList = new List<RunningProducts>();
            HashSet<string> seen = new HashSet<string>();
            foreach (var prod in products)
            {
                string appName = (string)prod.product.name;

                // Skip duplicates
                if (seen.Contains(appName))
                {
                    continue;
                }

                RunningProducts newProduct = new RunningProducts();
                //TO-DO: Create cases where some properties are missing
                newProduct.name = (string)prod.product.name;
                newProduct.version = (string)prod.version;

                if (prod.running_processes != null)
                {
                    newProduct.isRunning = true;
                }
                else
                {
                    newProduct.isRunning = false;
                }

                productList.Add(newProduct);
                seen.Add(appName);
            }
            products_json = productList;
        }


        /// <summary>
        /// Uses method 50500 to find missing patches for installed products.
        /// </summary>
        /// <summary>
        /// Uses method 50500 to find missing patches for each installed product by its signatureId.
        /// </summary>
        private static void CheckForMissingPatches()
        {
            Console.WriteLine("\nGetting a list of all installed products...");
            string all_products_json;
            int rc = ApiInvoke(0, "category", "0", out all_products_json);
            if (rc < 0)
            {
                Console.WriteLine("Could not retrieve product list. Aborting patch check.");
                return;
            }

            dynamic jsonOut = JObject.Parse(all_products_json);
            var products = jsonOut.result.detected_products;

            Console.WriteLine($"Found {products.Count} products to check for missing patches.");

            // Iterate through each product individually
            foreach (var product in products)
            {
                int signatureId = (int)product["signature"];
                string productName = (string)product["product"]["name"];

                Console.WriteLine($"\n--- Checking Product: {productName} (Signature ID: {signatureId}) ---");

                string patchInfoJson;
                // Pass the product's signatureId to the API call as requested
                int resultCode = ApiInvoke(50500, "signature", signatureId.ToString(), out patchInfoJson);

                if (resultCode >= 0)
                {
                    try
                    {
                        var parsedJson = JObject.Parse(patchInfoJson);
                        Console.WriteLine(parsedJson.ToString(Formatting.Indented));
                    }
                    catch (JsonReaderException)
                    {
                        Console.WriteLine("Received non-JSON response or empty result:");
                        Console.WriteLine(patchInfoJson);
                    }
                }
                else
                {
                    Console.WriteLine($"API call failed with result code: {resultCode}");
                }
            }
        }

        #endregion

        #region UI and Main Program Flow

        private static void ShowMenu()
        {
            bool running = true;
            while (running)
            {
                Console.WriteLine("\n========== OPSWAT OESIS SDK Menu ==========");
                Console.WriteLine("1. Detect All Products and Vulnerabilities");
                Console.WriteLine("2. Detect Running Products");
                Console.WriteLine("3. Check for Missing Patches (Method 50500)");
                Console.WriteLine("4. Exit");
                Console.Write("\nSelect choice: ");

                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        Console.WriteLine("\nScanning for installed products...");
                        List<Product> products_json;
                        DetectInstalledProducts(out products_json);
                        Console.WriteLine("--- Scan Results ---");
                        foreach (var product in products_json)
                        {
                            Console.WriteLine($"Product Name: {product.name}");
                            Console.WriteLine($"  - Vendor: {product.vendor}");
                            Console.WriteLine($"  - Version: {product.version}");
                            Console.WriteLine("---------------------------------");
                        }
                        break;
                    case "2":
                        Console.WriteLine("\nScanning for running products...");
                        List<RunningProducts> running_products;
                        CheckRunningApps(out running_products);
                        Console.WriteLine("--- Running Products ---");
                        int running_count = 0;
                        foreach (var product in running_products)
                        {
                            if (product.isRunning)
                            {
                                Console.WriteLine($"Product Name: {product.name}");
                                Console.WriteLine($"  - Version: {product.version}");
                                Console.WriteLine($"  - Is Running: {product.isRunning}");
                                Console.WriteLine("---------------------------------");
                                running_count++;
                            }
                        }
                        Console.WriteLine($"Total running products detected: {running_count}");
                        break;
                    case "3":
                        CheckForMissingPatches();
                        break;
                    case "4":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                ;
            }
        }

        static void Main(string[] args)
        {
            try
            {
                InitializeFramework();
                // Pre-load the database needed for patch management features
                LoadPatchDatabase("patch.dat", "ap_checksum.dat");
                ApiInvoke(50520, "dat_input_source_file", "v2mod.dat", out _);

                ShowMenu();

                OESISAdapter.wa_api_teardown();
                Console.WriteLine("\nApplication terminated.");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Out.WriteLine("\n[CRITICAL ERROR] An exception occurred: " + e.Message);
                Console.ResetColor();
            }
        }

        #endregion
    }
}