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
       
        public List<BrowserPlugins> GetPlugins()
        {
            List<BrowserPlugins> result = new List<BrowserPlugins>();

            OESISFramework.InitializeFramework();
            List<Product> productList = OESISCore.DetectAllProducts(OESISCategory.BROWSER);

            foreach(Product current in productList)
            {
                BrowserPlugins plugin = OESISCompliance.GetBrowserPlugin(current.signatureId.ToString());
                ProductInfo productInfo = OESISCore.GetProductInfo(plugin.signatureId);
                plugin.browserName = productInfo.name;

                result.Add(plugin);
            }

            OESISFramework.TearDown();

            return result;
        }

    }
}
