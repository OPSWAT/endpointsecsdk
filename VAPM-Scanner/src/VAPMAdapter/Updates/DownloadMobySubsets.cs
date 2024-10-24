﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPMAdapater.Updates;
using VAPMAdapater;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace VAPMAdapter.Updates
{
    public class DownloadMobySubsets
    {
        private static async Task DownloadDBFileAsync(string destPath, string fileName, string downloadURL)
        {
            string newFilePath = Path.Combine(destPath, fileName);

            if (File.Exists(newFilePath))
            {
                File.Delete(newFilePath);
            }

            using (HttpClient client = new HttpClient())
            {
                // Download the file asynchronously from the specified URL
                byte[] fileBytes = await client.GetByteArrayAsync(downloadURL);
                await File.WriteAllBytesAsync(newFilePath, fileBytes);
            }
        }

        public static async Task DownloadMobyFilesAsync()
        {
            string destPath = VAPMSettings.GetLocalCatalogDir();
            destPath = Path.Combine(destPath, "analog/server");

            // URLs and filenames for all JSON files
            var files = new (string url, string fileName)[]
            {
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby.json", "moby.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_counts.json", "moby_counts.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_patching.json", "moby_patching.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_vulnerability.json", "moby_vulnerability.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_uninstall.json", "moby_uninstall.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_antimalware.json", "moby_antimalware.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_patch_management.json", "moby_patch_management.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_p2p_agent.json", "moby_p2p_agent.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_messenger.json", "moby_messenger.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_cloud_storage.json", "moby_cloud_storage.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_health_agent.json", "moby_health_agent.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_backup.json", "moby_backup.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_web_conference.json", "moby_web_conference.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_vpn_client.json", "moby_vpn_client.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_virtual_machine.json", "moby_virtual_machine.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_remote_desktop_control.json", "moby_remote_desktop_control.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_unclassified.json", "moby_unclassified.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_antiphishing.json", "moby_antiphishing.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_browser.json", "moby_browser.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_public_file_sharing.json", "moby_public_file_sharing.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_encryption.json", "moby_encryption.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_data_loss_prevention.json", "moby_data_loss_prevention.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_firewall.json", "moby_firewall.json"),
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_certification.json", "moby_certification.json")
            };

            // Create a list of download tasks
            List<Task> downloadTasks = new List<Task>();
            foreach (var (url, fileName) in files)
            {
                downloadTasks.Add(DownloadDBFileAsync(destPath, fileName, url));
            }

            // Wait for all download tasks to complete
            await Task.WhenAll(downloadTasks);
        }

        //could utalize get timestamp method from updateMoby class
        public static Dictionary<string, string> GetMobyFileTimestamps()
        {
            // Filenames for all JSON files
            var fileNames = new string[]
            {
            "moby.json",
            "moby_counts.json",
            "moby_patching.json",
            "moby_vulnerability.json",
            "moby_uninstall.json",
            "moby_antimalware.json",
            "moby_patch_management.json",
            "moby_p2p_agent.json",
            "moby_messenger.json",
            "moby_cloud_storage.json",
            "moby_health_agent.json",
            "moby_backup.json",
            "moby_web_conference.json",
            "moby_vpn_client.json",
            "moby_virtual_machine.json",
            "moby_remote_desktop_control.json",
            "moby_unclassified.json",
            "moby_antiphishing.json",
            "moby_browser.json",
            "moby_public_file_sharing.json",
            "moby_encryption.json",
            "moby_data_loss_prevention.json",
            "moby_firewall.json",
            "moby_certification.json"
            };

            var timestamps = new Dictionary<string, string>();
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string destPath = Path.Combine(basePath, "catalog", "analog", "server");

            foreach (var fileName in fileNames)
            {
                string fullPath = Path.Combine(destPath, fileName);

                if (File.Exists(fullPath))
                {
                    string jsonContent = File.ReadAllText(fullPath);
                    JObject jsonObject = JObject.Parse(jsonContent);
                    string timestamp = jsonObject["timestamp"]?.ToString();

                    if (timestamp != null)
                    {
                        timestamps[fileName] = timestamp;
                    }
                }
            }

            return timestamps;
        }

        public static string GetJsonContent(string fileName)
        {
            // Get the current directory
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Construct the base directory based on the current environment
            string baseDirectory = Path.Combine(currentDirectory, @"catalog\analog\server");

            // Search for the file in the base directory and its subdirectories
            string[] files = Directory.GetFiles(baseDirectory, fileName, SearchOption.AllDirectories);

            if (files.Length > 0)
            {
                // Read the JSON content from the file
                string jsonContent = File.ReadAllText(files[0]);

                // Parse the JSON content into a JObject
                JObject jsonObject = JObject.Parse(jsonContent);

                // Return the formatted JSON string
                return jsonObject.ToString(Newtonsoft.Json.Formatting.Indented);
            }
            else
            {
                // Handle the case where the file is not found
                throw new FileNotFoundException($"Could not find file '{fileName}' in directory '{baseDirectory}' and its subdirectories.");
            }
        }
    }
}
