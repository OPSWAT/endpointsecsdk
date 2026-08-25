///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json.Linq;
using System.Windows.Forms;

namespace ComplianceAdapater.OESIS
{
    // OESIS product categories. Keep in sync with the SDK's authoritative list
    // (docs/html/data/auto_product_categories.js -> G_CATEGORIES / WAAPI_CATEGORY_*).
    public enum OESISCategory
    {
        ALL = 0,                            // WAAPI_CATEGORY_ALL (all categories)
        FILE_SHARING = 1,                   // WAAPI_CATEGORY_PUBLIC_FILE_SHARING
        BACKUP = 2,                         // WAAPI_CATEGORY_BACKUP_CLIENT
        DISK_ENCRYPTION = 3,
        ANTIPHISHING = 4,
        ANTIMALWARE = 5,
        BROWSER = 6,
        FIREWALL = 7,
        MESSENGER = 8,                      // WAAPI_CATEGORY_INSTANT_MESSENGER
        CLOUD_STORAGE = 9,
        UNCLASSIFIED = 10,
        DATA_LOSS_PREVENTION = 11,
        PATCH_MANAGEMENT = 12,
        VPN_CLIENT = 13,
        VIRTUAL_MACHINE = 14,
        HEALTH_AGENT = 15,
        REMOTE_CONTROL = 16,                // WAAPI_CATEGORY_REMOTE_CONTROL
        PEER_TO_PEER = 17,                  // WAAPI_CATEGORY_PEER_TO_PEER (P2P Agent)
        WEB_CONFERENCE = 18,
        GAMING = 19,
        VULNERABILITY_MANAGEMENT = 20,
        AI_SOFTWARE = 21,
        SYSTEM_DIAGNOSTIC_AND_CLEANUP = 22,
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
        // Same as DetectAllProducts, but also enumerates winget-sourced applications alongside the
        // OPSWAT-curated products (each detected product is annotated with a "data_source" field of
        // "opswat" or "winget"). Windows only; the flag is silently ignored on other platforms.
        //
        public static int DetectAllProductsIncludingWinget(out string json_out)
        {
            string json_in = "{\"input\": { \"method\": 0, \"enable_winget_source\": true } }";
            return OESISFramework.Invoke(json_in, out json_out);
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

            JObject jsonOut = JObject.Parse(product_json);
            JToken resultNode = jsonOut["result"];
            JToken products = (resultNode != null) ? resultNode["detected_products"] : null;

            if (products == null)
            {
                // No detected_products node. Surface it (rather than popping a MessageBox here,
                // which would run on a background worker thread) so the caller can report the error.
                throw new System.Exception("Product detection returned no result:\r\n\r\n" + product_json);
            }

            foreach (JToken product in products)
            {
                // Winget-sourced products may not carry a numeric signature; default to 0.
                int signature = 0;
                JToken signatureToken = product["signature"];
                if (signatureToken != null && signatureToken.Type == JTokenType.Integer)
                {
                    signature = (int)signatureToken;
                }

                JObject newEntry = new JObject();
                newEntry["signature"] = signature;
                newEntry["sig_name"] = (string)product["sig_name"];
                newEntry["categories"] = (product["categories"] as JArray) ?? new JArray();
                newEntry["data_source"] = (string)product["data_source"];
                result.Add(newEntry);
            }

            return result;
        }





    }
}
