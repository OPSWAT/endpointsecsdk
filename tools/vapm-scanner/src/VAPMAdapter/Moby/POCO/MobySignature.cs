using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAPMAdapter.Moby.POCO
{
    public class MobySignature
    {
        public string Id;
        public string Name;
        public bool supportAutoPatching;
        public bool supportAppRemover;
        public bool validationSupported;
        public bool backgroundPatchingSupported;
        public bool freshInstallable;
        public List<string> categories;
        public List<string> enabledControls;
        public List<string> certifications;
        public List<string> versions;
        public List<string> patchingVersions;
        public List<string> vulnerabilityVersions;
    }
}
