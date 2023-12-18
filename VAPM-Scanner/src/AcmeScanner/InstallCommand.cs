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
        public Boolean freshInstall;

        public InstallCommand (string signatureId, bool freshInstall)
        {
            this.signatureId = signatureId;
            this.freshInstall = freshInstall;
        }
    }
}
