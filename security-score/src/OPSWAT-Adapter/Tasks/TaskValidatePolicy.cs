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
using System.Collections.Generic;
using OPSWAT_Adapter.POCO;
using OPSWAT_Adapter.OESIS;

namespace ComplianceAdapater.Policy
{
    public class TaskValidatePolicy
    {
        private Logger checkLog = new Logger();
        private SecurityPolicy securityPolicy;


        public TaskValidatePolicy(SecurityPolicy secPolicy)
        {
            securityPolicy = secPolicy;
        }

        public Logger GetLogger()
        {
            return checkLog;
        }


        public bool ValidateFirewall(FirewallPolicy firewallPolicy)
        {
            bool result = true;

            if (firewallPolicy != null && firewallPolicy.enabled)
            {
                List<Product> productList = OESISCore.DetectAllProducts(OESISCategory.FIREWALL);
                if (productList != null && productList.Count > 0)
                {
                    if(firewallPolicy.expectedProduct != null)
                    {
                        if(!OESISUtil.IsProductInstalled(firewallPolicy.expectedProduct, OESISCategory.FIREWALL))
                        {
                            result = false;
                        }
                    }

                    if (firewallPolicy.isProtected)
                    {
                        if (!OESISUtil.IsFirewallRunning())
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


        public bool ValidateEncryption(EncryptionPolicy encryptionPolicy)
        {
            bool result = true;

            if (encryptionPolicy != null && encryptionPolicy.enabled)
            {
                List<Product> productList = OESISCore.DetectAllProducts(OESISCategory.DISK_ENCRYPTION);
                if (productList != null && productList.Count > 0)
                {
                    if (encryptionPolicy.expectedProduct != null)
                    {
                        if (!OESISUtil.IsProductInstalled(encryptionPolicy.expectedProduct, OESISCategory.DISK_ENCRYPTION))
                        {
                            result = false;
                        }
                    }

                    if (encryptionPolicy.isEncrypted)
                    {
                        if (!OESISUtil.IsDiskEncrypted())
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
                List<Product> productList = OESISCore.DetectAllProducts(OESISCategory.ANTIMALWARE);
                if (productList != null && productList.Count > 0)
                {
                    if (antimalwarePolicy.expectedProduct != null)
                    {
                        if (!OESISUtil.IsProductInstalled(antimalwarePolicy.expectedProduct, OESISCategory.ANTIMALWARE))
                        {
                            result = false;
                        }
                    }

                    if (antimalwarePolicy.isProtected)
                    {
                        if (!OESISUtil.IsAntimalwareProtected())
                        {
                            result = false;
                        }
                    }

                    if(antimalwarePolicy.defintionDate != null)
                    {
                        if(!OESISUtil.IsUpdateDefinitionRecent(antimalwarePolicy.defintionDate))
                        {
                            result = false;
                        }
                    }

                    if (antimalwarePolicy.scanDate != null)
                    {
                        if (!OESISUtil.IsScanRecent(antimalwarePolicy.scanDate))
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
