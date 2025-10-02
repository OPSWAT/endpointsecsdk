using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Schema;
using Vulnerability;


namespace Products
{
    internal class Program
    {
        private static void InitializeFramework()
        {
            string passkeypath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "pass_key.txt");
            if (!File.Exists(passkeypath))
            {
                Console.WriteLine("Pass_key.txt file not found in the directory");
                throw new Exception("Pass_key.txt not found");
            }

            string keypass = File.ReadAllText(passkeypath);
            string json_config = "{\"config\": { \"passkey_string\":\"" + keypass + "\", \"enable_pretty_print\": true, \"silent_mode\": true}}";
            string json_out = "{ }";

            IntPtr outPtr = IntPtr.Zero;
            int return_codes = OESISAdapter.wa_api_setup(json_config, out outPtr);
            if (outPtr != IntPtr.Zero)
            {
                json_out = XStringMarshaler.PtrToString(outPtr);
             }
            else
            {
                Console.WriteLine("Fail to initialize OESIS " + return_codes);

                throw new Exception("Fail to initialize");
            }
            OESISAdapter.wa_api_free(outPtr);
        }

        private static string JsonStructure(int methodNumber, string SecondArgumentName, string SecondArgumentValue)
        {
            string jsonConfig;
            if (SecondArgumentName == "" && SecondArgumentValue == "")
            {
                jsonConfig = $"{{ \"input\": {{ \"method\": {methodNumber} }} }}";
                return jsonConfig;
            }
            string safeValue = SecondArgumentValue.Replace(@"\", @"\\");

            bool isNumeric = int.TryParse(SecondArgumentValue, out _);

            if (isNumeric)
            {
                jsonConfig = $"{{ \"input\": {{ \"method\": {methodNumber}, \"{SecondArgumentName}\": {safeValue} }} }}";
            }
            else
            {
                jsonConfig = $"{{ \"input\": {{ \"method\": {methodNumber}, \"{SecondArgumentName}\": \"{safeValue}\" }} }}";
            }

            return jsonConfig;
        }

        private static void DetectInstalledProducts(int category_number, out List<Product> products_json)
        {

            string json_out;
            string json_config = ApiInvoke(0, "category", category_number.ToString(), out json_out);

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
                dynamic versionStatus = JObject.Parse(ApiInvoke(100, "signature", (newProduct.signatureId).ToString(), out _));
                string versionNumber = (string)versionStatus.result.version;
                newProduct.version = versionNumber;

                // Checking product vulnerability
                dynamic vulnerabilityStatus = JObject.Parse(ApiInvoke(50505, "signature", (newProduct.signatureId).ToString(), out _));
                var results = vulnerabilityStatus.result;
                if (results.has_vulnerability == true)
                {
                    newProduct.vulnerability = (string)results.cves.severity;
                    //newProduct.vulnerability_detail = (string)vulnerabilityStatus.cves;
                }
                else
                {
                    newProduct.vulnerability = "Clean";
                }


                productList.Add(newProduct);
            }
            products_json = productList;

        }

        private static void CheckRunningApps(out List<RunningProducts> products_json)
        {
            string jsonOutput;
            string jsonConfig = ApiInvoke(100001, "","", out jsonOutput);

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

        // result is the return code, while outPtr contains all the information that returns for that method choice
        public static string ApiInvoke(int methodChoice, string SecondArgumentName, string SecondArgumentValue, out string json_out)
        {
            string json_config = JsonStructure(methodChoice, SecondArgumentName, SecondArgumentValue);
            IntPtr outPtr = IntPtr.Zero;
            json_out = "{ }";
            int result = OESISAdapter.wa_api_invoke(json_config, out outPtr);

            if (outPtr != IntPtr.Zero) // Checking outPtr is not null
            {
                json_out = XStringMarshaler.PtrToString(outPtr);
                OESISAdapter.wa_api_free(outPtr);
            }
            else
            {
                Console.Out.WriteLine($"⚠️ Method {methodChoice} returned no output (result={result})");
            }

            if (result < 0)
            {
                throw new Exception("Method call failed to run correctly: " + result);
            }
            return json_out;
        }

        private static void showMenu()
        {
            bool running = true;
            Console.WriteLine("====OPSWAT Security SDK====");
            Console.WriteLine("1. Detected all products");
            Console.WriteLine("2. Detected running products");
            Console.WriteLine("3. Exit");
            Console.Write("\nSelect choice: ");

            string input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    List<Product> products_json = new List<Product>();
                    DetectInstalledProducts(0, out products_json);
                    foreach (var product in products_json)
                    {
                        Console.Out.WriteLine("Product Name: " + product.name);
                        Console.Out.WriteLine("Product Signature ID: " + product.signatureId);
                        Console.Out.WriteLine("Product Version: " + product.version);
                        Console.Out.WriteLine("Product Vulnerability Status: " + product.vulnerability);
                        //Console.Out.WriteLine("Product isRunning: " + product.isRunning);
                        Console.Out.WriteLine("---------------------------------");
                    }
                    break;
                case "2":
                    List<RunningProducts> running_products = new List<RunningProducts>();
                    CheckRunningApps(out running_products);
                    int running_count = 0;
                    int total_app = 1;
                    foreach (var product in running_products)
                    {
                        if (product.isRunning)
                        {
                            Console.Out.WriteLine("Product Name: " + product.name);
                            Console.Out.WriteLine("Product Version: " + product.version);
                            Console.Out.WriteLine("Product Running: " + product.isRunning);
                            Console.Out.WriteLine("---------------------------------");
                            running_count += 1;
                        }
                        total_app += 1;

                    }
                    Console.WriteLine("Total running product: " + running_count);
                    Console.WriteLine("Total products detected: " + total_app);
                    break;
                case "3":
                    running = false;
                    Console.WriteLine("Exiting...");
                    break;
                default:
                    Console.WriteLine("Invalid choice. Try again.");
                    break;
            };

            if (running)
            {
                Console.WriteLine("\nPress any key to return to menu...");
                Console.ReadKey();
            }
        }

        static void Main(string[] args)
        {
            List<Product> products_json = new List<Product>();
            List<RunningProducts> running_products = new List<RunningProducts>();
            try
            {
                InitializeFramework();
                ApiInvoke(50520, "dat_input_source_file", "v2mod.dat", out _);

                showMenu();

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



/* After calling the product, it immediately check for product's vulnerability rate. */
/*research winget, how integrate to sdk. the best way to integrate, would it be better to use winget or sdk.*/