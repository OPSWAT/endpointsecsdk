///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json.Linq;
using ComplianceAdapater.Log;
using ComplianceAdapater.OESIS;
using System;
using System.Collections.Generic;

namespace ComplianceAdapater.Policy
{
    public class TaskValidatePolicy
    {
        private JArray productList = null;
        private Logger checkLog = new Logger();
        private SecurityPolicy securityPolicy;


        public TaskValidatePolicy(SecurityPolicy secPolicy)
        {
            this.securityPolicy = secPolicy;
        }

        public Logger GetLogger()
        {
            return checkLog;
        }

        private JArray GetProductList()
        {
            if(productList == null)
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

            foreach(int sigId in firewallProductList)
            {
                string productName = Util.GetProductSignatureName(sigId,GetProductList());
                bool running = OESISCompliance.IsFirewallRunning(sigId);
                GetLogger().Log(running, "Firewall: " + productName + ":Is Running  = " + running);

                if(!running)
                {
                    result = false;
                }
            }

            return result;
        }


        public bool IsProductInstalled(List<int> installedProductList, ProductInfo productInfo, string categoryString)
        {
            bool result = false;

            int expectedSignatureId = productInfo.sigId;
            foreach (int installedSigId in installedProductList)
            {
                string productName = Util.GetProductSignatureName(installedSigId, GetProductList());
                GetLogger().Log(true, categoryString + ": " + productName + ":Is Installed");

                if (installedSigId == expectedSignatureId)
                {
                    GetLogger().Log(true, categoryString + ":Found expected product");
                    result = true;
                    break;
                }
            }

            if(!result)
            {
                GetLogger().Log(false, "Failed to find product installed:" + productInfo.name);
            }

            return result;
        }



        public bool ValidateFirewall(FirewallPolicy firewallPolicy)
        {
            bool result = true;

            if (firewallPolicy != null && firewallPolicy.enabled)
            {
                List<int> firewallProviders = OESIS.Util.GetProductSignaturesByCategory(OESISCategory.FIREWALL, GetProductList());

                if (firewallProviders != null && firewallProviders.Count > 0)
                {
                    if(firewallPolicy.expectedProduct != null)
                    {
                        if(!IsProductInstalled(firewallProviders, firewallPolicy.expectedProduct, "Firewall"))
                        {
                            result = false;
                        }
                    }

                    if (firewallPolicy.isProtected)
                    {
                        if (!IsFirewallRunning(firewallProviders))
                        {
                            result = false;
                        }
                    }

                }
                else
                {
                    GetLogger().Log(false, "Failed to find firewall product");
                }

            }
            else
            {
                GetLogger().Log(true, "Firewall: Skipping Firewall, check not Enabled");
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
                GetLogger().Log(true, "Antimalware: " + productName + ":Is Antimalware Protected  = " + antimalwareProtected);

                if (antimalwareProtected)
                {
                    result = true;
                    break;
                }
            }

            if(!result)
            {
                GetLogger().Log(false, "Antimalware: Is Antimalware Protected");
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

                if(lastUpdateTime > updateWindow)
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




        public bool ValidateEncryption(EncryptionPolicy encryptionPolicy)
        {
            bool result = true;

            if (encryptionPolicy != null && encryptionPolicy.enabled)
            {
                List<int> encryptionProviders = OESIS.Util.GetProductSignaturesByCategory(OESISCategory.DISK_ENCRYPTION, GetProductList());

                if (encryptionProviders != null && encryptionProviders.Count > 0)
                {
                    if (encryptionPolicy.expectedProduct != null)
                    {
                        if (!IsProductInstalled(encryptionProviders, encryptionPolicy.expectedProduct, "Encryption"))
                        {
                            result = false;
                        }
                    }

                    if (encryptionPolicy.isEncrypted)
                    {
                        if (!IsDiskEncrypted(encryptionProviders))
                        {
                            result = false;
                        }
                    }

                }
                else
                {
                    GetLogger().Log(false, "Failed to find firewall product");
                }

            }
            else
            {
                GetLogger().Log(true, "Encryption: Skipping Encryption, check not Enabled");
            }


            return result;
        }


        public bool ValidateAntimalware(AntimalwarePolicy antimalwarePolicy)
        {
            bool result = true;

            if (antimalwarePolicy != null && antimalwarePolicy.enabled)
            {
                List<int> antimalwareProviders = OESIS.Util.GetProductSignaturesByCategory(OESISCategory.ANTIMALWARE, GetProductList());

                if (antimalwareProviders != null && antimalwareProviders.Count > 0)
                {
                    if (antimalwarePolicy.expectedProduct != null)
                    {
                        if (!IsProductInstalled(antimalwareProviders, antimalwarePolicy.expectedProduct, "Antimalware"))
                        {
                            result = false;
                        }
                    }

                    if (antimalwarePolicy.isProtected)
                    {
                        if (!IsAntimalwareProtected(antimalwareProviders))
                        {
                            result = false;
                        }
                    }

                    if(antimalwarePolicy.defintionDate != null)
                    {
                        if(!IsUpdateDefinitionRecent(antimalwareProviders,antimalwarePolicy.defintionDate))
                        {
                            result = false;
                        }
                    }

                    if (antimalwarePolicy.scanDate != null)
                    {
                        if (!IsScanRecent(antimalwareProviders, antimalwarePolicy.scanDate))
                        {
                            result = false;
                        }
                    }
                }
                else
                {
                    GetLogger().Log(false, "Failed to find firewall product");
                }

            }
            else
            {
                GetLogger().Log(true, "Antimalware: Skipping Antimalware, check not Enabled");
            }


            return result;
        }



        public bool ValidatePolicy()
        {
            bool result = true;

            OESISFramework.InitializeFramework();

            if (securityPolicy != null)
            {
                if (!ValidateAntimalware(securityPolicy.antimalwarePolicy))
                {
                    result = false;
                }


                if (!ValidateFirewall(securityPolicy.firewallPolicy))
                {
                    result = false;
                }


                if (!ValidateEncryption(securityPolicy.encryptionPolicy))
                {
                    result = false;
                }
            }
            else
            {
                GetLogger().Log(true, "Security Policy: Not configured returning success");
            }

            OESISFramework.TearDown();

            return result;
        }





    }
}
