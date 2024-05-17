///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////


using System;

namespace OPSWAT_Adapter.POCO
{
    public class BackupState : MethodResult
    {
        public DateTime last_backup_activity;
        public bool backup_active;
        public string backupJson;
    }
}
