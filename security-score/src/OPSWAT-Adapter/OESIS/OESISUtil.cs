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
using OPSWAT_Adapter.POCO;
using System;
using System.Collections.Generic;


namespace OPSWAT_Adapter.OESIS
{


    public class OESISUtil
    {
        private static Logger checkLog = new Logger();

        private static Logger GetLogger()
        {
            return checkLog;
        }


        public static bool IsFirewallRunning()
        {
            bool result = true;

            List<Product> productList = OESISCore.DetectAllProducts(OESISCategory.FIREWALL);
            foreach (Product current in productList)
            {
                string productName = current.signature_name;
                FirewallState firewallState = OESISCompliance.GetFirewallState(current.signatureId);
                GetLogger().Log(firewallState.enabled, "Firewall: " + productName + ":Is Running  = " + firewallState.enabled);

                if (!firewallState.enabled)
                {
                    result = false;
                }
            }

            return result;
        }

        public static bool IsDiskEncrypted()
        {
            bool result = true;

            List<Product> productList = OESISCore.DetectAllProducts(OESISCategory.DISK_ENCRYPTION);
            foreach (Product current in productList)
            {
                string productName = current.signature_name;
                EncryptionState encryptionState = OESISCompliance.GetEncryptionState(current.signatureId);
                bool encrypted = encryptionState.fully_encrypted;

                GetLogger().Log(encrypted, "Encryption: " + productName + ":Is DiskEncrypted  = " + encrypted);

                if (!encrypted)
                {
                    result = false;
                }
            }

            return result;
        }

        public static bool IsAntimalwareProtected()
        {
            bool result = false;

            List<Product> productList = OESISCore.DetectAllProducts(OESISCategory.DISK_ENCRYPTION);
            foreach (Product current in productList)
            {
                string productName = current.signature_name;

                RealTimeProtectionState rtpState = OESISCompliance.GetRealTimeProtectionState(current.signatureId);
                Threats threats = OESISCompliance.GetThreats(current.signatureId);
                bool antimalwareProtected = rtpState.enabled && threats.no_threats;


                GetLogger().Log(antimalwareProtected, "Antimalware: " + productName + ":Is Antimalware Protected  = " + antimalwareProtected);

                if (antimalwareProtected)
                {
                    result = true;
                }
            }

            return result;
        }


        public static bool IsUpdateDefinitionRecent(DateTime updateWindow)
        {
            bool result = false;

            List<Product> productList = OESISCore.DetectAllProducts(OESISCategory.DISK_ENCRYPTION);
            foreach (Product current in productList)
            {
                string productName = current.signature_name;
                DateTime lastUpdateTime = OESISCompliance.GetLastUpdateTime(current.signatureId);
                GetLogger().Log(true, "Antimalware: " + productName + ":Last defintion update : " + lastUpdateTime);

                if (lastUpdateTime > updateWindow)
                {
                    result = true;
                }

                GetLogger().Log(result, "Antimalware: " + productName + ":Update Defintion is recent : " + result);
            }

            return result;
        }

        public static bool IsScanRecent(DateTime scanWindow)
        {
            bool result = false;

            List<Product> productList = OESISCore.DetectAllProducts(OESISCategory.DISK_ENCRYPTION);
            foreach (Product current in productList)
            {
                string productName = current.signature_name;
                DateTime lastScanTime = OESISCompliance.GetLastScanTime(current.signatureId);
                GetLogger().Log(true, "Antimalware: " + productName + ":Last scan update : " + lastScanTime);

                if (lastScanTime > scanWindow)
                {
                    result = true;
                }

                GetLogger().Log(result, "Antimalware: " + productName + ":Last scan is recent : " + result);
            }

            return result;
        }

        public static bool IsProductInstalled(ProductInfo productInfo, OESISCategory category)
        {
            bool result = false;

            List<Product> productList = OESISCore.DetectAllProducts(category);
            foreach (Product current in productList)
            {
                string productName = current.signature_name;
                GetLogger().Log(true, category.ToString() + ": " + productName + ":Is Installed");

                if (current.signatureId == productInfo.sigId)
                {
                    GetLogger().Log(true, category.ToString() + ":Found expected product");
                    result = true;
                    break;
                }
            }

            if (!result)
            {
                GetLogger().Log(false, "Failed to find product installed:" + productInfo.name);
            }

            return result;
        }



    }
}
