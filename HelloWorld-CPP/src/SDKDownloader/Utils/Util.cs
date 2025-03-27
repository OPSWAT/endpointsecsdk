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
            {
                Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(dir);
        }

        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
