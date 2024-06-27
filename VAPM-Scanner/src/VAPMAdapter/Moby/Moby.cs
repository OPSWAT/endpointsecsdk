using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

            foreach (JProperty pName in jsonProductList.Properties())
            {
                foreach (JProperty current in pName)
                {
                    MobyProduct newProduct = new MobyProduct();
                    newProduct.name = pName.Name +" "+ current.Name;
                    //newProduct.Id = current.Name["product_id"];
                }
                

            }
            return result;
        }
    }
}
