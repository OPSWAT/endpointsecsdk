///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for OPSWAT Posture
///  Reference Implementation using OPSWAT Endpoint SDK Compliance module for demoing the 
///  Compliance capability
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace ComplianceAdapater.Log
{

    public class Logger
    {
        private List<LogEntry> logList = new List<LogEntry>();

        public void Log(bool status, string message)
        {
            LogEntry newEntry = new LogEntry();
            newEntry.message = message;
            newEntry.success = status;

            logList.Add(newEntry);
        }

        public List<LogEntry> GetLogEntryList()
        {
            return logList;
        }

    }
}
