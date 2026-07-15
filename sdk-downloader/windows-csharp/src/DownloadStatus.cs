///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OESIS Framework
///
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace SDKDownloader
{
    /// <summary>
    /// Persistent record of what has been downloaded, so subsequent runs can skip files whose
    /// size hasn't changed on the server. Stored as download_status.json at the SDK root (next
    /// to the extract/ and client/ directories, so it survives the per-run archive cleanup).
    ///
    /// Each entry records the file's size, the URL, the server Last-Modified (when the server
    /// provides it), and the local update time - i.e. the size of each file downloaded and the
    /// date the files were updated.
    /// </summary>
    public class DownloadStatus
    {
        public const string FileName = "download_status.json";

        private readonly string _sdkRoot;
        private readonly string _path;
        private Dictionary<string, Dictionary<string, object>> _files =
            new Dictionary<string, Dictionary<string, object>>();

        public HashSet<string> DownloadedThisRun = new HashSet<string>();

        public DownloadStatus(string sdkRoot)
        {
            _sdkRoot = sdkRoot;
            _path = Path.Combine(sdkRoot, FileName);
        }

        public DownloadStatus Load()
        {
            if (File.Exists(_path))
            {
                try
                {
                    var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                    var data = serializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(_path));
                    if (data != null && data.ContainsKey("files") &&
                        data["files"] is Dictionary<string, object> filesObj)
                    {
                        foreach (var kv in filesObj)
                        {
                            if (kv.Value is Dictionary<string, object> rec)
                                _files[kv.Key] = rec;
                        }
                    }
                    Console.WriteLine("Loaded download status: " + _files.Count + " file(s) known.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not read " + _path + " (" + e.Message +
                                      "); treating all files as new.");
                    _files = new Dictionary<string, Dictionary<string, object>>();
                }
            }
            else
            {
                Console.WriteLine("No download status at " + _path + "; all files will be downloaded.");
            }
            return this;
        }

        /// <summary>Recorded size for a file key, or null if not previously downloaded.</summary>
        public long? GetSize(string key)
        {
            if (_files.TryGetValue(key, out var rec) && rec.ContainsKey("size") && rec["size"] != null)
            {
                try { return Convert.ToInt64(rec["size"]); }
                catch { return null; }
            }
            return null;
        }

        public void Record(string key, string url, long size, string sha256, string serverLastModified)
        {
            _files[key] = new Dictionary<string, object>
            {
                ["size"] = size,
                ["url"] = url,
                ["sha256"] = sha256,
                ["server_last_modified"] = serverLastModified,
                ["updated_at"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
            };
            DownloadedThisRun.Add(key);
        }

        public void Save()
        {
            Directory.CreateDirectory(_sdkRoot);
            var payload = new Dictionary<string, object>
            {
                ["generated_at"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                ["files"] = _files,
            };
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            File.WriteAllText(_path, serializer.Serialize(payload));
            Console.WriteLine("Saved download status: " + _files.Count + " file(s) -> " + _path);
        }
    }
}
