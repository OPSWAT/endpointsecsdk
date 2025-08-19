///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PatchWOS
{
    /// <summary>
    /// Provides utility methods for downloading files and validating their integrity
    /// </summary>
    internal static class HttpClientUtils
    {

        /// <summary>
        /// Converts a byte array to a string representation.
        /// </summary>
        /// <param name="array">The byte array to convert.</param>
        /// <returns>A string representation of the byte array.</returns>
        public static string ByteArrayToString(byte[] array)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                result.Append($"{array[i]:X2}");
            }

            return result.ToString();
        }

        /// <summary>
        /// Checks the SHA256 hash of a file against an expected hash.
        /// </summary>
        /// <param name="file">The file to check.</param>
        /// <param name="expectedHash">The expected SHA256 hash.</param>
        /// <returns>True if the hash matches, otherwise false.</returns>
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

                        if (expectedHash.Equals(newHashString, StringComparison.OrdinalIgnoreCase))
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

        /// <summary>
        /// Checks the SHA1 hash of a file against an expected hash.
        /// </summary>
        /// <param name="file">The file to check.</param>
        /// <param name="expectedHash">The expected SHA1 hash.</param>
        /// <returns>True if the hash matches, otherwise false.</returns>
        private static bool CheckSha1(string file, string expectedHash)
        {
            bool result = false;

            using (SHA1 mySHA1 = SHA1.Create())
            {
                FileInfo fileInfo = new FileInfo(file);

                using (FileStream fileStream = fileInfo.Open(FileMode.Open))
                {
                    try
                    {
                        fileStream.Position = 0;
                        byte[] hashValue = mySHA1.ComputeHash(fileStream);
                        string newHashString = ByteArrayToString(hashValue);

                        if (expectedHash.Equals(newHashString, StringComparison.OrdinalIgnoreCase))
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

        /// <summary>
        /// Downloads a file from the specified URL and validates its hash using multiple hash types.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="localFilePath">The local file path where the downloaded file will be saved.</param>
        /// <param name="checksums">List of expected checksums (SHA1, SHA256, etc.). Hash type is auto-detected by length.</param>
        /// <returns>True if the file was downloaded and validated successfully, otherwise false.</returns>
        public static bool DownloadValidFile(string url, string localFilePath, System.Collections.Generic.List<string> checksums)
        {
            bool result = false;

            HttpClientUtils.DownloadFileSynchronous(url, localFilePath);

            if (File.Exists(localFilePath))
            {
                if (checksums != null && checksums.Count > 0)
                {
                    bool hashValid = false;
                    
                    foreach (string checksum in checksums)
                    {
                        if (string.IsNullOrEmpty(checksum))
                            continue;
                            
                        // Auto-detect hash type by length
                        if (checksum.Length == 40) // SHA1
                        {
                            Console.WriteLine("Validating SHA1: " + checksum);
                            if (CheckSha1(localFilePath, checksum))
                            {
                                Console.WriteLine("SHA1 validation successful");
                                hashValid = true;
                                break;
                            }
                            else
                            {
                                Console.WriteLine("SHA1 validation failed for: " + checksum);
                            }
                        }
                        else if (checksum.Length == 64) // SHA256
                        {
                            Console.WriteLine("Validating SHA256: " + checksum);
                            if (CheckSha256(localFilePath, checksum))
                            {
                                Console.WriteLine("SHA256 validation successful");
                                hashValid = true;
                                break;
                            }
                            else
                            {
                                Console.WriteLine("SHA256 validation failed for: " + checksum);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Unsupported hash length (" + checksum.Length + "): " + checksum);
                        }
                    }
                    
                    if (hashValid)
                    {
                        result = true;
                    }
                    else
                    {
                        Console.WriteLine("Failed to validate any checksums for: " + localFilePath);
                    }
                }
                else
                {
                    Console.WriteLine("No checksums provided, skipping validation");
                    result = true;
                }
            }
            else
            {
                Console.WriteLine("Download Failed URL: " + url);
            }

            return result;
        }

        /// <summary>
        /// Downloads a file from the specified URL and saves it to the destination path synchronously.
        /// </summary>
        /// <param name="url">The URL of the file to download.</param>
        /// <param name="destPath">The local file path where the downloaded file will be saved.</param>
        public static void DownloadFileSynchronous(string url, string destPath)
        {

            using (var client = new System.Net.Http.HttpClient()) // WebClient
            {

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


        /// <summary>
        /// Asynchronously downloads a file from the specified URI and saves it to the given file name.
        /// </summary>
        /// <param name="client">The HttpClient to use for downloading the file.</param>
        /// <param name="uri">The URI of the file to download.</param>
        /// <param name="fileName">The local file path where the downloaded file will be saved.</param>
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