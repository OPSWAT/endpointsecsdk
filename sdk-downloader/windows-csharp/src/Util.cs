///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OESIS Framework
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Tar;

namespace SDKDownloader
{
    public class Util
    {
        private static string VCR_URL = "https://vcr.opswat.com/gw/file/download/%file%?type=1&token=%token%";

        public static void ExtractTar(string tarFilePath, string destDir)
        {
            using (FileStream fs = File.OpenRead(tarFilePath))
            using (TarInputStream tarStream = new TarInputStream(fs, System.Text.Encoding.UTF8)) // Specify UTF-8 encoding to handle non-ASCII bytes
            {
                TarEntry entry;
                while ((entry = tarStream.GetNextEntry()) != null)
                {
                    string outPath = Path.Combine(destDir, entry.Name);

                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(outPath);
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(outPath));
                    using (FileStream outFile = File.Create(outPath))
                    {
                        tarStream.CopyEntryContents(outFile);
                    }
                }
            }
        }

        public static void ExtractFile(string archiveFile, string destDir)
        {
            if (archiveFile.EndsWith(".zip"))
            {
                ZipFile.ExtractToDirectory(archiveFile, destDir);
            }
            else if (archiveFile.EndsWith(".tar"))
            {
                ExtractTar(archiveFile, destDir);
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

        public static string FindFirstFile(string dir)
        {
            string result = null;

            if (Directory.Exists(dir))
            {
                string[] fileList = Directory.GetFiles(dir);
                if (fileList != null && fileList.Length > 0)
                {
                    result = fileList[0];
                }
            }

            return result;
        }

        public static bool IsFileUpdated(string checkFile)
        {
            bool result = false;

            if (File.Exists(checkFile))
            {
                FileInfo checkFileInfo = new FileInfo(checkFile);

                //  
                // Update the SDK every 7 days  
                //  
                if (checkFileInfo.LastWriteTime > DateTime.Now.AddDays(-7))
                {
                    result = true;
                }
            }

            return result;
        }

        private static string GetDownloadToken()
        {
            string sdk_token_file = "download_token.txt";
            if (!File.Exists(sdk_token_file))
            {
                string sdkRoot = GetSDKRoot();
                string licensePath = Path.Combine(sdkRoot, "eval-license");

                if (Directory.Exists(licensePath))
                {
                    sdk_token_file = Path.Combine(licensePath, sdk_token_file);
                    if (!File.Exists(sdk_token_file))
                        throw new Exception("Make sure there is a download token file available in the %sdk-root%/eval-license directory: " + licensePath);
                }
                else
                {
                    throw new Exception("Make sure there is a download token file available in the running directory: " + Directory.GetCurrentDirectory());
                }

            }

            string downloadToken = File.ReadAllText(sdk_token_file);
            return downloadToken;
        }

        public static string GetTokenDownloadURL(string fileName)
        {
            string result = VCR_URL.Replace("%token%", GetDownloadToken());
            result = result.Replace("%file%", fileName);
            return result;
        }

        public static string GetArchivesPath(string sdkRoot)
        {
            string archivePath = Path.Combine(sdkRoot, "archives");
            if (!Directory.Exists(archivePath))
            {
                Directory.CreateDirectory(archivePath);
            }

            return archivePath;
        }

        public static string GetExtractPath(string sdkRoot)
        {
            string archivePath = Path.Combine(sdkRoot, "extract");
            if (!Directory.Exists(archivePath))
            {
                Directory.CreateDirectory(archivePath);
            }

            return archivePath;
        }

        public static string GetClientPath(string sdkRoot)
        {
            string archivePath = Path.Combine(sdkRoot, "client");
            if (!Directory.Exists(archivePath))
            {
                Directory.CreateDirectory(archivePath);
            }

            return archivePath;
        }

        public static void CopyDirectory(string sourceDir, string destinationDir, bool overwrite = true)
        {
            // Create the destination directory if it doesn't exist
            Directory.CreateDirectory(destinationDir);

            // Copy all files
            foreach (string filePath in Directory.GetFiles(sourceDir))
            {
                string destFilePath = Path.Combine(destinationDir, Path.GetFileName(filePath));
                File.Copy(filePath, destFilePath, overwrite);
            }
        }
    
        public static string GetSDKRoot()
        {
            bool found = false;
            string searchDirectory = Directory.GetCurrentDirectory();

            while(!found)
            {
                string sdkrootFile = Path.Combine(searchDirectory, "sdkroot");
                if(File.Exists(sdkrootFile))
                {
                    found = true;
                }
                else
                {
                    DirectoryInfo parentDir = Directory.GetParent(searchDirectory);
                    if (parentDir != null)
                        searchDirectory = parentDir.FullName;
                    else
                        break;
                }
            }

            if (!found)
                searchDirectory = Directory.GetCurrentDirectory();

            return searchDirectory;
        }


    }


}
