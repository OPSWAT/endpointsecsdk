///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for Acme Scanner
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.IO;

namespace SDKDownloader
{
    internal class Util
    {
        public static void CreateCleanDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir,true);
            }
            Directory.CreateDirectory(dir);
        }
    }
}
