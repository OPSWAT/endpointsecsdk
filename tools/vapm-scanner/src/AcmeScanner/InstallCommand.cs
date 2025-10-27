using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcmeScanner
{
    public class InstallCommand
    {
        public string signatureId;
        public bool freshInstall;
        public bool backgroundInstall;
        public bool validateInstall;
        public bool forceClose;
        public bool usePatchId;


        public InstallCommand ( string signatureId, 
                                bool freshInstall, 
                                bool backgroundInstall, 
                                bool validateInstall,
                                bool forceClose,
                                bool usePatchId)
        {
            this.signatureId = signatureId;
            this.freshInstall = freshInstall;
            this.backgroundInstall = backgroundInstall;
            this.validateInstall = validateInstall;
            this.forceClose = forceClose;
            this.usePatchId = usePatchId;
        }
    }
}
