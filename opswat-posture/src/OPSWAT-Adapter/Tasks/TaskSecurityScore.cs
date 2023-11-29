///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using ComplianceAdapater.Log;
using ComplianceAdapater.OESIS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace OPSWAT_Adapter.Tasks
{
    public class TaskSecurityScore
    {
        private Logger checkLog = new Logger();
        private JArray productList = null;


        public Logger GetLogger()
        {
            return checkLog;
        }

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

        public bool IsFirewallRunning(List<int> firewallProductList)
        {
            bool result = true;

            foreach (int sigId in firewallProductList)
            {
                string productName = Util.GetProductSignatureName(sigId, GetProductList());
                bool running = OESISCompliance.IsFirewallRunning(sigId);
                GetLogger().Log(running, "Firewall: " + productName + ":Is Running  = " + running);

                if (!running)
                {
                    result = false;
                }
            }

            return result;
        }

        private bool IsDiskEncrypted(List<int> encryptionProductList)
        {
            bool result = true;

            foreach (int sigId in encryptionProductList)
            {
                string productName = Util.GetProductSignatureName(sigId, GetProductList());
                bool encrypted = OESISCompliance.IsDiskFullyEncrypted(sigId);
                GetLogger().Log(encrypted, "Encryption: " + productName + ":Is DiskEncrypted  = " + encrypted);

                if (!encrypted)
                {
                    result = false;
                }
            }

            return result;
        }

        private bool IsAntimalwareProtected(List<int> antimalwareProductList)
        {
            bool result = false;

            foreach (int sigId in antimalwareProductList)
            {
                string productName = Util.GetProductSignatureName(sigId, GetProductList());
                bool antimalwareProtected = OESISCompliance.IsAntimalwareProtected(sigId);
                GetLogger().Log(antimalwareProtected, "Antimalware: " + productName + ":Is Antimalware Protected  = " + antimalwareProtected);

                if (antimalwareProtected)
                {
                    result = true;
                }
            }

            return result;
        }


        private bool IsUpdateDefinitionRecent(List<int> antimalwareProductList, DateTime updateWindow)
        {
            bool result = false;

            foreach (int sigId in antimalwareProductList)
            {
                string productName = Util.GetProductSignatureName(sigId, GetProductList());
                DateTime lastUpdateTime = OESISCompliance.GetLastUpdateTime(sigId);
                GetLogger().Log(true, "Antimalware: " + productName + ":Last defintion update : " + lastUpdateTime);

                if (lastUpdateTime > updateWindow)
                {
                    result = true;
                }

                GetLogger().Log(result, "Antimalware: " + productName + ":Update Defintion is recent : " + result);
            }

            return result;
        }

        private bool IsScanRecent(List<int> antimalwareProductList, DateTime scanWindow)
        {
            bool result = false;

            foreach (int sigId in antimalwareProductList)
            {
                string productName = Util.GetProductSignatureName(sigId, GetProductList());
                DateTime lastScanTime = OESISCompliance.GetLastScanTime(sigId);
                GetLogger().Log(true, "Antimalware: " + productName + ":Last scan update : " + lastScanTime);

                if (lastScanTime > scanWindow)
                {
                    result = true;
                }

                GetLogger().Log(result, "Antimalware: " + productName + ":Last scan is recent : " + result);
            }

            return result;
        }


        public int GetSecurityScore()
        {
            int resultCount = 0;

            OESISFramework.InitializeFramework();

            List<int> firewallProducts = Util.GetProductSignaturesByCategory(OESISCategory.FIREWALL, GetProductList());
            if (IsFirewallRunning(firewallProducts))
            {
                resultCount += 2;
            }

            List<int> encryptionProducts = Util.GetProductSignaturesByCategory(OESISCategory.DISK_ENCRYPTION, GetProductList());
            if (IsDiskEncrypted(encryptionProducts))
            {
                resultCount += 2;
            }

            List<int> antimalwareProducts = Util.GetProductSignaturesByCategory(OESISCategory.ANTIMALWARE, GetProductList());
            if (IsAntimalwareProtected(antimalwareProducts))
            {
                resultCount += 2;
            }

            if (IsUpdateDefinitionRecent(antimalwareProducts, DateTime.Now.AddDays(-1)))
            {
                resultCount += 2;
            }

            if (IsScanRecent(antimalwareProducts, DateTime.Now.AddDays(-1)))
            {
                resultCount += 2;
            }


            OESISFramework.TearDown();

            return resultCount;
        }

    }
}
