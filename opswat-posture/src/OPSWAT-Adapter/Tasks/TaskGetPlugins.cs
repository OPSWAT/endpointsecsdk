///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using ComplianceAdapater.OESIS;
using Newtonsoft.Json.Linq;
using OPSWAT_Adapter.POCO;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace OPSWAT_Adapter.Tasks
{
    public class TaskGetPlugins
    {
        private JArray productList = null;

        private JArray GetProductList()
        {
            if (productList == null)
            {
                string json_product;
                OESISCore.DetectAllProducts(out json_product);
                productList = OESISCore.GetProductArrayFromString(json_product);
            }

            return productList;
        }




        public List<BrowserPlugins> GetPlugins()
        {
            List<BrowserPlugins> result = new List<BrowserPlugins>();

            OESISFramework.InitializeFramework();

            List<int> browserProducts = Util.GetProductSignaturesByCategory(OESISCategory.BROWSER, GetProductList());

            foreach(int signature in browserProducts)
            {
                BrowserPlugins plugin = OESISCompliance.GetBrowserPlugin(signature.ToString());
                ProductInfo productInfo = OESISCore.GetProductInfo(plugin.signatureId);
                plugin.browserName = productInfo.name;

                result.Add(plugin);
            }

            OESISFramework.TearDown();

            return result;
        }

    }
}
