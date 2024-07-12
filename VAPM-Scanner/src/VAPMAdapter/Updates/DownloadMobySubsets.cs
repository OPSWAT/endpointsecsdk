﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPMAdapater.Updates;
using VAPMAdapater;

namespace VAPMAdapter.Updates
{
    internal class DownloadMobySubsets
    {
        private static void DownloadDBFile(string destPath, string fileName, string downloadURL)
        {
            string newFilePath = Path.Combine(destPath, fileName);

            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }

            // Download the file synchronously from the specified URL
            HttpClientUtils.DownloadFileSynchronous(downloadURL, newFilePath);
        }

        public static void DownloadMobyFiles()
        {
            string destPath = VAPMSettings.getLocalCatalogDir();
            destPath = Path.Combine(destPath, "analog/server");

            // URLs and filenames for all JSON files
            var files = new (string url, string fileName)[]
            {
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
            ("https://oesis-downloads-portal.s3.amazonaws.com/moby_firewall.json", "moby_firewall.json")
            };

            // Download all JSON files
            foreach (var (url, fileName) in files)
            {
                DownloadDBFile(destPath, fileName, url);
            }
        }

        public static bool DoesMobyFileExist(string fileName)
        {
            // Get the current directory of the executable
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            // Define the relative path to the file from the base directory
            string relativePath = $@"catalog\analog\server\{fileName}";

            // Combine the base path with the relative path
            string fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));

            // Check if the file exists at the specified path
            return File.Exists(fullPath);
        }

        public static bool DoAllMobyFilesExist()
        {
            // Filenames for all JSON files
            var fileNames = new string[]
            {
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
            "moby_firewall.json"
            };

            // Check if all JSON files exist
            foreach (var fileName in fileNames)
            {
                if (!DoesMobyFileExist(fileName))
                {
                    return false;
                }
            }

            return true;
        }
    }
}