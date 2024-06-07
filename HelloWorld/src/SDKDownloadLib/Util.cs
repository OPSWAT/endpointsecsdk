using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
