///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json.Linq;
using OPSWAT_Adapter.POCO;
using System.Collections.Generic;

namespace ComplianceAdapater.OESIS
{
    public enum OESISCategory
    {
        ALL = 0,
        FILE_SHARING = 1,
        BACKUP = 2,
        DISK_ENCRYPTION = 3,
        ANTIPHISHING = 4,
        ANTIMALWARE = 5,
        BROWSER = 6,
        FIREWALL = 7,
        MESSENGER = 8,
        CLOUD_STORAGE = 9,
        UNCLASSIFIED = 10,
        DATA_LOSS_PREVENTION = 11,
        PATCH_MANAGEMENT = 12,
        VPN_CLIENT = 13,
        VIRTUAL_MACHINE = 14,
        HEALTH_AGENT = 15,
        REMOTE_CONTROL = 16,
    }



    public class OESISCore
    {
        // 
        // This will return JSON for all of the products found in the system
        // https://software.opswat.com/OESIS_V4/html/c_method.html
        // on the left select OESIS Core/Discover Products
        public static List<Product> DetectAllProducts(OESISCategory category)
        {
            List<Product> result = null;
            string json_in = "{\"input\": { \"method\": 0, \"category\": " + (int)category + " } }";


            string json_out;
            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                result = new List<Product>();

                dynamic parsedObject = JObject.Parse(json_out);
                var jsonProducts = parsedObject["result"]["detected_products"];

                for (int i = 0; i < jsonProducts.Count; i++)
                {
                    Product newProduct = new Product();

                    newProduct.signatureId = jsonProducts[i]["signature"];
                    newProduct.signature_name = jsonProducts[i]["sig_name"];
                    //newProduct.categories_json = jsonProducts[i]["categories"];
                    newProduct.productId = jsonProducts[i]["product"]["id"];
                    newProduct.productName = jsonProducts[i]["product"]["name"];
                    newProduct.vendorId = jsonProducts[i]["vendor"]["id"];
                    newProduct.vendorName = jsonProducts[i]["vendor"]["name"];
                    newProduct.uninstall_hive_json= jsonProducts[i]["uninstall_hive"];
                    newProduct.bundle_info_json = jsonProducts[i]["buncle_info"];

                    result.Add(newProduct);
                }
            }

            return result;
        }


        // 
        // This will return JSON for all of the products found in the system
        // https://software.opswat.com/OESIS_V4/html/c_method.html
        // on the left select OESIS Core/Discover Products
        public static List<Product> DetectAllProducts()
        {
            return DetectAllProducts(OESISCategory.ALL);
        }




        // 
        // This will return JSON for all of the products found in the system
        // https://software.opswat.com/OESIS_V4/html/c_method.html
        public static ProductInfo GetProductInfo(string signatureId)
        {
            string json_out;
            ProductInfo result = null;
            string json_in = "{\"input\": { \"method\": 109, \"signature\":" + signatureId + " } }";
            int callResult = OESISFramework.Invoke(json_in, out json_out);

            if (callResult >= 0)
            {
                result = new ProductInfo();
                dynamic parsedObject = JObject.Parse(json_out);
                
               result.name = parsedObject["result"]["detected_product"]["sig_name"];
            }

            return result;
        }




        // This method just is used to quickly parse the products
        public static JArray GetProductArrayFromString(string product_json)
        {
            JArray result = new JArray();

            dynamic jsonOut = JObject.Parse(product_json);
            var products = jsonOut.result.detected_products;

            for (int i = 0; i < products.Count; i++)
            {
                JObject newEntry = JObject.FromObject(new
                {
                    signature = (int)products[i].signature,
                    sig_name = (string)products[i].sig_name,
                    categories = (JArray)products[i].categories
                });
                result.Add(newEntry);
            }

            return result;
        }





    }
}
