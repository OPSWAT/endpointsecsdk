using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPMAdapter.Catalog;
using VAPMAdapter.Catalog.POCO;
using VAPMAdapter.Moby.POCO;

namespace VAPMAdapter.Moby
{
    public class Moby
    {
        private string jsonLocation;
        private List<MobyProduct> productList;
        private void CheckSuccess(bool result)
        {
            if (!result)
            {
                throw new Exception("Failed to run routine");
            }

        }

        public bool Load(string catalogRoot)
        {
            bool result = false;

            if (Directory.Exists(catalogRoot))
            {
                try
                {
                    jsonLocation = catalogRoot + "/moby.json";
                    result = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to load catalog: " + e);
                }
            }
            else
            {
                Console.WriteLine("Failed to find catalog directory. Please run 'catlog download --token %token%' ");
            }


            return result;
        }

        private JObject getProductJsonObject()
        {
            JObject result;
            string jsonString = File.ReadAllText(jsonLocation);

            if (!string.IsNullOrEmpty(jsonString))
            {
                result = JObject.Parse(jsonString);
            }
            else
            {
                Console.WriteLine("Failed to load Product Json");
                throw (new Exception("Unable to load Json"));
            }

            return result;
        }

        private JObject getProductsListJson()
        {
            JObject result = null;

            JObject productJsonObject = getProductJsonObject();
            JObject productsJson = (JObject)productJsonObject["products"];

            result = productsJson;

            return result;
        }

        public List<MobyProduct> GetList()
        {

            // Return the cache version
            if (productList != null)
            {
                return productList;
            }

            List<MobyProduct> result = new List<MobyProduct>();

            JObject jsonProductList = (JObject)getProductsListJson();

            foreach (JProperty product in jsonProductList.Properties())
            {
                foreach (JProperty os in ((JObject)product.Value).Properties())
                {
                    MobyProduct newProduct = new MobyProduct();
                    newProduct.name = product.Name + " " + os.Name;
                    newProduct.Id = (string)os.Value["product_id"];
                    newProduct.cveDetection = (bool)os.Value["cve_detection"];
                    JArray signatures = (JArray)os.Value["signatures"];
                    foreach (JObject currentSig in signatures.Children<JObject>())
                    {
                        MobySignature newSignature= new MobySignature();
                        newSignature.Name = (string)currentSig["signature_name"];
                        newSignature.Id = (string)currentSig["signature_id"];
                        newSignature.supportAutoPatching = (bool)currentSig["support_auto_patching"];
                        newSignature.supportAppRemover = (bool)currentSig["support_app_remover"];
                        newSignature.validationSupported = (bool)currentSig["validation_supported"];
                        newSignature.categories=new List<String>();
                        newSignature.enabledControls = new List<String>();
                        newSignature.certifications = new List<String>();
                        newSignature.versions = new List<String>();
                        newSignature.patchingVersions = new List<String>();
                        newSignature.vulnerabilityVersions = new List<String>();
                        foreach (string  currentCategory in currentSig["categories"])
                        {
                            newSignature.categories.Add(currentCategory);
                        }
                        foreach (string currentControl in currentSig["enabled_controls"])
                        {
                            newSignature.enabledControls.Add(currentControl);
                        }
                        foreach (string curr in currentSig["certifications"])
                        {
                            newSignature.certifications.Add(curr);
                        }
                        foreach (string curr in currentSig["versions"])
                        {
                            newSignature.versions.Add(curr);
                        }
                        foreach (string curr in currentSig["patching_versions"])
                        {
                            newSignature.patchingVersions.Add(curr);
                        }
                        foreach (string curr in currentSig["vulnerability_versions"])
                        {
                            newSignature.vulnerabilityVersions.Add(curr);
                        }
                        newProduct.sigList.Add(newSignature);

                    }
                    result.Add(newProduct);

                }
            }
            return result;
        }
    }
}
