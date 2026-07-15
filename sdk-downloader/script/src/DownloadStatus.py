import json
import os

from datetime import datetime


class DownloadStatus:
    """Persistent record of what has been downloaded, so subsequent runs can skip files whose
    size hasn't changed on the server. Stored as download_status.json at the SDK root (next to
    the extract/ and client/ directories, so it survives the per-run archive cleanup).

    Each entry records the file's size, the URL, the server Last-Modified (when the server
    provides it), and the local update time — i.e. the size of each file downloaded and the
    date the files were updated.
    """

    FILE_NAME = "download_status.json"

    def __init__(self, sdk_root):
        self.sdk_root = sdk_root
        self.path = os.path.join(sdk_root, DownloadStatus.FILE_NAME)
        self.files = {}                 # key -> {size, url, sha256, server_last_modified, updated_at}
        self.downloaded_this_run = set()

    def load(self):
        if os.path.isfile(self.path):
            try:
                with open(self.path, "r", encoding="utf-8") as f:
                    data = json.load(f)
                self.files = data.get("files", {}) or {}
                print(f"Loaded download status: {len(self.files)} file(s) known "
                      f"(last generated {data.get('generated_at', 'unknown')})")
            except (ValueError, OSError) as e:
                print(f"Could not read {self.path} ({e}); treating all files as new.")
                self.files = {}
        else:
            print(f"No download status at {self.path}; all files will be downloaded.")
        return self

    def get_size(self, key):
        rec = self.files.get(key)
        return rec.get("size") if rec else None

    def record(self, key, url, size, sha256=None, server_last_modified=None):
        self.files[key] = {
            "size":                 size,
            "url":                  url,
            "sha256":               sha256,
            "server_last_modified": server_last_modified,
            "updated_at":           datetime.now().isoformat(timespec="seconds"),
        }
        self.downloaded_this_run.add(key)

    def save(self):
        os.makedirs(self.sdk_root, exist_ok=True)
        payload = {
            "generated_at": datetime.now().isoformat(timespec="seconds"),
            "files":        self.files,
        }
        with open(self.path, "w", encoding="utf-8") as f:
            json.dump(payload, f, indent=2)
        print(f"Saved download status: {len(self.files)} file(s) -> {self.path}")
