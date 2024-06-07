using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SDKDownloader
{
    internal static class HttpClientUtils
    {

        // Display the byte array in a readable format.
        public static string ByteArrayToString(byte[] array)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                result.Append($"{array[i]:X2}");
            }

            return result.ToString();
        }

        private static bool CheckSha256(string file, string expectedHash)
        {
            bool result = false;

            using (SHA256 mySHA256 = SHA256.Create())
            {
                FileInfo fileInfo = new FileInfo(file);

                // Compute and print the hash values for each file in directory.
                using (FileStream fileStream = fileInfo.Open(FileMode.Open))
                {
                    try
                    {
                        // Create a fileStream for the file.
                        // Be sure it's positioned to the beginning of the stream.
                        fileStream.Position = 0;
                        // Compute the hash of the fileStream.
                        byte[] hashValue = mySHA256.ComputeHash(fileStream);
                        // Write the name and hash value of the file to the console.

                        string newHashString = ByteArrayToString(hashValue);

                        if (expectedHash == newHashString)
                        {
                            result = true;
                        }
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine($"I/O Exception: {e.Message}");
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Console.WriteLine($"Access Exception: {e.Message}");
                    }
                }
            }

            return result;
        }


        public static bool DownloadValidFile(string url, string localFilePath, string sha256Hash)
        {
            bool result = false;

            HttpClientUtils.DownloadFileSynchronous(url, localFilePath);

            if (File.Exists(localFilePath))
            {
                if (sha256Hash != null)
                {

                    if (CheckSha256(localFilePath, sha256Hash))
                    {
                        result = true;
                    }
                    else
                    {
                        Console.WriteLine("Failed to validate hash : " + localFilePath);
                        Console.WriteLine("Failed to validate hash : " + sha256Hash);
                    }
                }
                else
                {
                    Logger.Log("Checksum Validation did not occur because there was no 256 key");
                    result = true;
                }

            }
            else
            {
                Console.WriteLine("Downlaod Failed URL: " + url);
            }

            return result;
        }

        public static void DownloadFileSynchronous(string url, string destPath)
        {
            using (var client = new System.Net.Http.HttpClient()) // WebClient
            {
                Logger.Log("Downloading URL: " + url);
                Logger.Log("Downloading: " + destPath);

                var fileName = destPath;
                var uri = new Uri(url);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                Task result = client.DownloadFileTaskAsync(uri, fileName);
                result.Wait();
            }
        }



        public static async Task DownloadFileTaskAsync(this HttpClient client, Uri uri, string FileName)
        {
            using (var s = await client.GetStreamAsync(uri))
            {
                using (var fs = new FileStream(FileName, FileMode.CreateNew))
                {
                    await s.CopyToAsync(fs);
                }
            }
        }
    }

}

