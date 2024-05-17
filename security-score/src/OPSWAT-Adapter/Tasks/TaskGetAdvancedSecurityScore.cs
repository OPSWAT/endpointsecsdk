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
using OPSWAT_Adapter.TaskPOCO;
using System.Collections.Generic;
using System.Text;

namespace OPSWAT_Adapter.Tasks
{
    public class TaskGetAdvancedSecurityScore
    {
        private Logger checkLog = new Logger();
        public Logger GetLogger()
        {
            return checkLog;
        }

        private SecurityScoreEntry GetScoreEntry(Product product, bool running, Dictionary<string,string> attributes)
        {
            SecurityScoreEntry result = new SecurityScoreEntry();

            result.product = product;
            result.running = running;
            result.attributes = attributes;

            return result;
        }

        private SecurityScoreEntry GetScoreEntry(Product product, bool running)
        {
            return GetScoreEntry(product, running, null);
        }


        private int GetSecurityScore(List<SecurityScoreEntry> entryList, int expectedValue)
        {
            int result = 0;

            foreach(SecurityScoreEntry entry in entryList)
            {
                if(entry.running)
                {
                    result = expectedValue;
                    break;
                }
            }


            return result;
        }


        private SecurityScoreModule GetFirewallScore()
        {
            SecurityScoreModule result = new SecurityScoreModule();

            List<Product> productList = OESISCore.DetectAllProducts(OESISCategory.FIREWALL);
            foreach (Product current in productList)
            {
                FirewallState firewallState = OESISCompliance.GetFirewallState(current.signatureId);

                if (firewallState.success)
                {
                    bool enabled = firewallState.enabled;
                    SecurityScoreEntry entry = GetScoreEntry(current, enabled);
                    result.entries.Add(entry);
                }
            }

            result.score = GetSecurityScore(result.entries, 1);
            result.name = "Firewall";

            return result;
        }

        private SecurityScoreModule GetEncryptionScore()
        {
            SecurityScoreModule result = new SecurityScoreModule();

            List<Product> productList = OESISCore.DetectAllProducts(OESISCategory.DISK_ENCRYPTION);
            foreach (Product current in productList)
            {
                EncryptionState encryptionState = OESISCompliance.GetEncryptionState(current.signatureId);

                if (encryptionState.success)
                {
                    bool enabled = encryptionState.fully_encrypted;
                    SecurityScoreEntry entry = GetScoreEntry(current, enabled);
                    result.entries.Add(entry);
                }
            }

            result.score = GetSecurityScore(result.entries, 1);
            result.name = "Encryption";

            return result;
        }


        private SecurityScoreModule GetBackupScore()
        {
            SecurityScoreModule result = new SecurityScoreModule();

            List<Product> productList = OESISCore.DetectAllProducts(OESISCategory.BACKUP);
            foreach (Product current in productList)
            {
                BackupState backupState = OESISCompliance.GetBackupState(current.signatureId);

                if (backupState.success)
                {
                    bool enabled = backupState.backup_active;
                    SecurityScoreEntry entry = GetScoreEntry(current, enabled);
                    result.entries.Add(entry);
                }
            }

            result.score = GetSecurityScore(result.entries, 1);
            result.name = "Backup";

            return result;
        }


        private SecurityScoreModule GetAntimalwareScore()
        {
            SecurityScoreModule result = new SecurityScoreModule();
            int resultScore = 0;

            List<Product> productList = OESISCore.DetectAllProducts(OESISCategory.ANTIMALWARE);
            foreach (Product current in productList)
            {
                string productName = current.signature_name;
                RealTimeProtectionState rtpProtectionState = OESISCompliance.GetRealTimeProtectionState(current.signatureId);

                if (rtpProtectionState.success)
                {
                    Threats threats = OESISCompliance.GetThreats(current.signatureId);
                    bool enabled = rtpProtectionState.enabled;

                    SecurityScoreEntry entry = GetScoreEntry(current, enabled);
                    entry.attributes.Add("threats", threats.rawJson);
                    result.entries.Add(entry);

                    if(enabled && threats.no_threats)
                    {
                        resultScore = 2;
                    }
                    else if(enabled)
                    {
                        resultScore = 1;
                    }
                }
            }

            result.score = GetSecurityScore(result.entries, resultScore);
            result.name = "Antimalware";

            return result;
        }

        private void AddSecurityScoreModule(SecurityScoreResult scoreResult, SecurityScoreModule scoreModule)
        {
            scoreResult.moduleList.Add(scoreModule);
            scoreResult.score += scoreModule.score;
        }




        public SecurityScoreResult GetAdvancedSecurityScore()
        {
            SecurityScoreResult result = new SecurityScoreResult();

            OESISFramework.InitializeFramework();

            AddSecurityScoreModule(result, GetFirewallScore());
            AddSecurityScoreModule(result, GetEncryptionScore());
            AddSecurityScoreModule(result, GetBackupScore());
            AddSecurityScoreModule(result, GetAntimalwareScore());

            return result;
        }

    }
}
