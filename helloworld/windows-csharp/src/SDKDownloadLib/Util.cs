///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System.IO;

namespace SDKDownloader
{
    public class Util
    {
        public static void ExtractArchives(string archiveDirectory, string extractDirectory)
        {
            if (archiveDirectory != null && Directory.Exists(archiveDirectory))
            {
                string[] dirList = Directory.GetFiles(archiveDirectory);

                // Extract just the first 2 files in the directory
                foreach (string archive in dirList)
                {
                    if (archive.EndsWith(".zip"))
                    {
                        string extractionPath = extractDirectory;
                        if (archive.Contains("resource"))
                        {
                            extractionPath = Path.Combine(extractionPath, "resource");
                        }
                        else
                        {
                            extractionPath = Path.Combine(extractionPath, "sdk");
                        }

                        Util.CreateCleanDir(extractionPath);
                        System.IO.Compression.ZipFile.ExtractToDirectory(archive, extractionPath);
                    }
                }
            }
            else
            {
                Logger.Log("Unable to extract because directory is null or non existent");
            }
        }

        public static string GetCleanTempDir(string dirName)
        {
            string tempPath;

            tempPath = Path.Combine(Path.GetTempPath(), dirName);
            Util.CreateCleanDir(tempPath);

            return tempPath;
        }
        public static void CreateCleanDir(string dir)
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir,true);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}
