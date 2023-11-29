///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json.Linq;

namespace ComplianceAdapater.OESIS
{
    public enum OESISCategory
    {
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
        public static int DetectAllProducts(out string json_out, int category)
        {
            int result = 0;
            string json_in = "{\"input\": { \"method\": 0, \"category\":" + category + " } }";
            result = OESISFramework.Invoke(json_in, out json_out);
            return result;
        }

        // 
        // This will return JSON for all of the products found in the system
        // https://software.opswat.com/OESIS_V4/html/c_method.html
        // on the left select OESIS Core/Discover Products
        public static int DetectAllProducts(out string json_out)
        {
            int result = 0;
            string json_in = "{\"input\": { \"method\": 0 } }";
            result = OESISFramework.Invoke(json_in, out json_out);
            return result;
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
