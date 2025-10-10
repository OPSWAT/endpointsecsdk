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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vulnerability;

namespace Products
{
    // The 'Product' and 'RunningProducts' classes are referenced from the 'Vulnerability' namespace.

    internal class Program
    {
        
        private static readonly HttpClient GptClient = new HttpClient();
        private static List<Product> CachedProducts = new List<Product>();

        // Helper constant for robust winget parsing and display alignment
        private const int NameColumnWidth = 36;
        private const int IdColumnWidth = 36;
        private const int VersionColumnWidth = 16;
        private const int UpgradeColumnWidth = 16;

        class WingetProduct
        {
            public string name { get; set; }
            public string vendor { get; set; }
            public string version { get; set; }
            public string status { get; set; }
            public bool isCompliant { get; set; }
            public string lastUpdate { get; set; }
            public string Id { get; set; }
            // This is the core data point provided by WinGet, enriching our Product model.
            public string UpgradeVersion { get; set; }
        }

        class SecurityReport
        {
            public int Score { get; set; }
            public List<string> Reasons { get; set; }
        }

        class Tool
        {
            public string type { get; set; } = "function";
            public Function function { get; set; } = new Function();
        }

        class Function
        {
            public string name { get; set; } = "ExecuteWingetCommandFromAI";
            public string description { get; set; } = "Execute a winget command (install, upgrade, uninstall) on the local system. For complex tasks like downgrading, this tool can execute chained commands.";
            public Parameters parameters { get; set; } = new Parameters();
        }

        class Parameters
        {
            public string type { get; set; } = "object";
            public Dictionary<string, object> properties { get; set; } = new Dictionary<string, object>
            {
                // Updated description to allow command chaining
                { "command", new { type = "string", description = "The full, validated winget command(s) to run. For multi-step tasks like downgrade, use chained commands separated by '&&', e.g., 'winget uninstall Adobe.Reader --silent && winget install Adobe.Reader --version 20.1.0.1 --silent --accept-source-agreements --accept-package-agreements'"} }
            };
            public List<string> required { get; set; } = new List<string> { "command" };
        }


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
            CachedProducts = products_json;
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
                string latestInstallerJson;
                // Pass the product's signatureId to the API call as requested
                int resultCode = ApiInvoke(50500, "signature", signatureId.ToString(), out patchInfoJson); //Returns the product Patch Level
                int resultCode2 = ApiInvoke(50300, "signature", signatureId.ToString(), out latestInstallerJson); // 50500 is not always correct, so we call this

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
                else if (resultCode2 == -12)
                {
                    Console.WriteLine($"This product is not supported by the method");
                }
                else
                {
                    Console.WriteLine($"Method failed successfully with code {resultCode}");
                }

                if (resultCode2 >= 0)
                {
                    try
                    {
                        var getLatestInstallerInfo = JObject.Parse(latestInstallerJson);
                        Console.WriteLine(getLatestInstallerInfo.ToString(Formatting.Indented));
                    }
                    catch (JsonReaderException)
                    {
                        Console.WriteLine("Received non-JSON response or empty result:");
                        Console.WriteLine(latestInstallerJson);
                    }
                }
                else if (resultCode2 == -1026)
                {
                    Console.WriteLine($"Method returns the following: \"An error when a product is not supported\"");
                }
                else
                {
                    Console.WriteLine($"Method failed successfully with code {resultCode2}");
                }
            }
        }

        // =====AI Winget Integration below ======

        private static async Task<Dictionary<string, string>> GetWingetUpgrades()
        {
            var upgrades = new Dictionary<string, string>();
            string output = await ExecuteCommand("winget list --upgrade-available");

            var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            // Regex to robustly capture the fixed-width columns from winget's output
            var regex = new Regex(@"^(.{36})(.{36})(.{16})(.{16})", RegexOptions.Multiline);

            // Find the start of the data rows (after the "----" line)
            int dataStartIndex = Array.FindIndex(lines, l => l.Contains("----"));
            if (dataStartIndex == -1) return upgrades; // No upgrades or unexpected format

            for (int i = dataStartIndex + 1; i < lines.Length; i++)
            {
                var match = regex.Match(lines[i]);
                if (match.Success)
                {
                    string name = match.Groups[1].Value.Trim();
                    string availableVersion = match.Groups[4].Value.Trim();

                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(availableVersion))
                    {
                        upgrades[name.ToLower()] = availableVersion;
                    }
                }
            }
            return upgrades;
        }

        private static async Task<string> ExecuteCommand(string command)
        {
            try
            {
                Console.WriteLine($"\n[EXEC] Running command: {command}");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C {command}", // /C allows command chaining
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await Task.Run(() => process.WaitForExit());

                if (process.ExitCode != 0)
                {
                    // Report failure, but include all output for context
                    return $"[Command Failed] Exit Code: {process.ExitCode}\nError: {error}\nOutput: {output}";
                }

                return output;
            }
            catch (Exception ex)
            {
                return $"[Execution Error] {ex.Message}";
            }
        }

        /// <summary>
        /// Tool function that the AI calls to execute a winget command.
        /// </summary>

        private static async Task<Dictionary<string, string>> GetWingetPackageInfo()
        {
            var packages = new Dictionary<string, string>();
            string output = await ExecuteCommand("winget list");

            var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var regex = new Regex(@"^(.{36})(.{36})", RegexOptions.Multiline); // Capture only Name and ID

            int dataStartIndex = Array.FindIndex(lines, l => l.Contains("----"));
            if (dataStartIndex == -1) return packages;

            for (int i = dataStartIndex + 1; i < lines.Length; i++)
            {
                var match = regex.Match(lines[i]);
                if (match.Success)
                {
                    string name = match.Groups[1].Value.Trim();
                    string id = match.Groups[2].Value.Trim();

                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(id))
                    {
                        packages[name.ToLower()] = id;
                    }
                }
            }
            return packages;
        }
        
        private static string ExecuteWingetCommandFromAI(string command)
        {
            // Ensure only winget or chained winget commands are attempted
            if (string.IsNullOrWhiteSpace(command) || !command.ToLower().Contains("winget"))
            {
                return "Error: Only 'winget' commands or chained 'winget' commands are permitted for execution.";
            }

            // Run the winget command(s) and wait for the result
            string result = ExecuteCommand(command).GetAwaiter().GetResult();

            // Re-scan products after the action to update status
            DetectInstalledProducts(out CachedProducts);

            return $"Command executed successfully. New system status scanned. Output:\n{result}";
        }

        private static async Task<string> CallGPTWithContext(
            string userPrompt,
            List<Product> installedProducts)
        {
            // Use your actual API Key or set environment variable OPENAI_API_KEY
            string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";

            var contextBuilder = new StringBuilder();

            contextBuilder.AppendLine("You are the official MetaDefender Core Zero Trust Assistant, with the ability to execute winget commands on the local machine.");
            contextBuilder.AppendLine("Your goal is to provide concise, authoritative risk management guidance based on the scanned system data and to execute requested actions using the provided tool.");

            // *** UPDATED Instruction for SDK Context ***
            contextBuilder.AppendLine("Note that the application list is generated by an **SDK-Integrated Scanner** that uses deep system checks, providing a more complete list and better version coverage than simple WinGet lists.");
            contextBuilder.AppendLine("To reliably **uninstall, downgrade, or install a specific version** of an application, you MUST recommend a two-step process.");
            contextBuilder.AppendLine("1. **Uninstall:** Use 'winget uninstall <Winget ID> --silent'");
            contextBuilder.AppendLine("2. **Install Specific Version:** Use 'winget install <ID> --version <X.Y.Z> --silent --accept-source-agreements --accept-package-agreements'");
            contextBuilder.AppendLine("Chain these commands together using '&&' in a single tool call for reliability (e.g., 'winget uninstall APP.ID --silent && winget install APP.ID --version X.Y.Z --silent ...').");
            contextBuilder.AppendLine("For simple upgrades, 'winget upgrade --all' or 'winget upgrade <ID>' is sufficient. If the user asks to **install** a new app (like Facebook or a package not already listed), you MUST use **'winget install <ID>'** not 'winget upgrade --all'. All upgrade/install commands MUST include the flags: '--silent --accept-source-agreements --accept-package-agreements'.");
            contextBuilder.AppendLine("If the installation of an app is not found online, then try to search via Microsoft Store. You can search for the app via the following command: winget search \"<app name>\" --source msstore. Do not prompt back to user to ask if they want to proceed with that, just install" );
            contextBuilder.AppendLine("ONLY use the ExecuteWingetCommandFromAI tool if the user explicitly requests an action like 'update', 'install', 'downgrade', 'delete', 'version' or 'uninstall'.  You MUST use the string-based 'Winget ID'. Do not invent commands.");
            contextBuilder.AppendLine("If you decide to use the tool, respond ONLY with the JSON function call. Do NOT provide any preceding or subsequent text in that turn.");
            // *** End UPDATED Instruction ***

            contextBuilder.AppendLine("\n=== End-Device Trust Posture (Installed Apps) ===");
            foreach (var p in installedProducts)
            {
                string wingetIdText = string.IsNullOrEmpty(p.WingetId) ? "Not Found" : p.WingetId;
                contextBuilder.AppendLine($"- Product: {p.name} (Winget ID: {wingetIdText}) (SDK ID: {p.signatureId}) v{p.version}");
            }
            contextBuilder.AppendLine("\nRespond to the user's query.");
            string context = contextBuilder.ToString();

            var tools = new List<Tool> { new Tool() };

            var initialMessages = new List<object>
            {
                new { role = "system", content = context },
                new { role = "user", content = userPrompt }
            };
            
            var payload = new { model = "gpt-4o-mini", temperature = 0.1, messages = initialMessages.ToArray(), tools = tools };

            GptClient.DefaultRequestHeaders.Clear();
            GptClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            string json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await GptClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            string respJson = await resp.Content.ReadAsStringAsync();
            dynamic parsed = JObject.Parse(respJson);
            
            var assistantMessage = parsed["choices"]?[0]?["message"];
            
            if (assistantMessage?["tool_calls"] != null)
            {
                var assistantToolCallMessage = assistantMessage;
                var toolCall = assistantToolCallMessage["tool_calls"][0];
                string functionName = toolCall["function"]["name"].ToString();
                string argumentsJson = toolCall["function"]["arguments"].ToString();
                string toolCallId = toolCall["id"].ToString();

                if (functionName == "ExecuteWingetCommandFromAI")
                {
                    var args = JObject.Parse(argumentsJson);
                    string wingetCommand = args["command"].ToString();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    // CHANGED: Updated the logging message.
                    Console.WriteLine($"\n[AI Action Request] Tool: ExecuteWingetCommandFromAI");
                    Console.WriteLine($"[AI Action Request] Command: {wingetCommand}");
                    Console.ResetColor();

                    string toolResult = ExecuteWingetCommandFromAI(wingetCommand);
                    
                    var toolResultMessage = new
                    {
                        role = "tool",
                        tool_call_id = toolCallId,
                        name = functionName,
                        content = toolResult
                    };

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    // CHANGED: Updated the logging message.
                    Console.WriteLine("\n[AI Summary] Sending result back to AI for summarization...");
                    Console.ResetColor();

                    var toolCallsList = ((JArray)assistantToolCallMessage["tool_calls"]).ToList();
                    var cleanAssistantMessage = new
                    {
                        role = "assistant",
                        tool_calls = toolCallsList
                    };

                    var conversationMessages = new List<object>
                    {
                        new { role = "system", content = context },
                        new { role = "user", content = userPrompt },
                        cleanAssistantMessage,
                        toolResultMessage 
                    };

                    var secondPayload = new { model = "gpt-4o-mini", messages = conversationMessages.ToArray() };
                    json = JsonConvert.SerializeObject(secondPayload);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                    resp = await GptClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                    respJson = await resp.Content.ReadAsStringAsync();
                    parsed = JObject.Parse(respJson);
                }
            }

            if (parsed["error"] != null)
            {
                return $"API Error: {(string)parsed["error"]["message"]}";
            }

            return (string)(parsed["choices"]?[0]?["message"]?["content"] ?? "No response received.");
        }

        
        private static async Task RunAiAssistant()
        {
            Console.WriteLine("\n=== OESIS AI Security Assistant ===");
            Console.WriteLine("Running SDK scan to get product list...");
            DetectInstalledProducts(out CachedProducts); // Always get fresh SDK data

            Console.WriteLine("Checking for available upgrades via Winget...");
            var availableUpgrades = await GetWingetUpgrades();

            Console.WriteLine("Getting Winget package IDs...");
            var wingetPackages = await GetWingetPackageInfo();

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\n[You]: ");
                Console.ResetColor();
                string userPrompt = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(userPrompt) || userPrompt.ToLower() == "exit") break;

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\n[Assistant]: Thinking...");
                Console.ResetColor();

                string response = await CallGPTWithContext(userPrompt, CachedProducts);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(response);
                Console.ResetColor();
            }
        }


        #endregion

        #region UI and Main Program Flow



        private static async Task ShowMenu()
        {
            bool running = true;
            while (running)
            {
                Console.WriteLine("\n========== OPSWAT OESIS SDK Menu ==========");
                Console.WriteLine("1. Detect All Products and Vulnerabilities");
                Console.WriteLine("2. Detect Running Products");
                Console.WriteLine("3. Check for Missing Patches (Method 50500)");
                Console.WriteLine("4. AI Assistant");
                Console.WriteLine("5. Exit");
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
                        await RunAiAssistant();
                        break;
                    case "5":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                ;
            }
        }

        static async Task Main(string[] args)
        {
            try
            {
                InitializeFramework();
                // Pre-load the database needed for patch management features
                LoadPatchDatabase("patch.dat", "ap_checksum.dat");
                ApiInvoke(50520, "dat_input_source_file", "v2mod.dat", out _);

                await ShowMenu();

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

