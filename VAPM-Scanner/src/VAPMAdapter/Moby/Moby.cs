using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
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
        private MobyTotalCounts counts;

        public Moby()
        {
            counts = new MobyTotalCounts
            {
                TotalProductsCount = new MobyPlatformCounts(),
                TotalSignaturesCount = new MobyPlatformCounts(),
                CveDetection = new MobyPlatformCounts(),
                SupportAutoPatching = new MobyPlatformCounts(),
                BackgroundPatching = new MobyPlatformCounts(),
                FreshInstallable = new MobyPlatformCounts(),
                ValidationSupported = new MobyPlatformCounts(),
                AppRemover = new MobyPlatformCounts()
            };
        }

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
                throw new Exception("Unable to load Json");
            }

            return result;
        }

        private JObject getProductsListJson()
        {
            JObject productJsonObject = getProductJsonObject();
            return (JObject)productJsonObject["products"];
        }

        public List<MobyProduct> GetList()
        {
            // Return the cached version
            if (productList != null)
            {
                return productList;
            }

            List<MobyProduct> result = new List<MobyProduct>();

            JObject jsonProductList = getProductsListJson();

            foreach (JProperty product in jsonProductList.Properties())
            {
                foreach (JProperty os in ((JObject)product.Value).Properties())
                {
                    MobyProduct newProduct = new MobyProduct
                    {
                        name = product.Name + " " + os.Name,
                        Id = (string)os.Value["product_id"],
                        cveDetection = (bool)os.Value["cve_detection"]
                    };
                    Debug.WriteLine("id==" + newProduct.Id);

                    result.Add(newProduct);
                }
            }

            productList = result; // Cache the result
            return result;
        }

        public void LoadCounts()
        {
            JObject jsonObject = getProductJsonObject();
            JObject countsJson = (JObject)jsonObject["counts"];

            counts = new MobyTotalCounts
            {
                TotalProductsCount = ParseCounts((JObject)countsJson["total_products_count"]),
                TotalSignaturesCount = ParseCounts((JObject)countsJson["total_signatures_count"]),
                CveDetection = ParseCounts((JObject)countsJson["cve_detection"]),
                SupportAutoPatching = ParseCounts((JObject)countsJson["support_auto_patching"]),
                BackgroundPatching = ParseCounts((JObject)countsJson["background_patching"]),
                FreshInstallable = ParseCounts((JObject)countsJson["fresh_installable"]),
                ValidationSupported = ParseCounts((JObject)countsJson["validation_supported"]),
                AppRemover = ParseCounts((JObject)countsJson["app_remover"])
            };
        }

        private MobyPlatformCounts ParseCounts(JObject countJson)
        {
            return new MobyPlatformCounts
            {
                Total = (int)countJson["total"],
                Windows = (int)countJson["platforms"]["windows"],
                Mac = (int)countJson["platforms"]["mac"],
                Linux = (int)countJson["platforms"]["linux"]
            };
        }

        public MobyTotalCounts GetCounts()
        {
            return counts;
        }
    }
}
