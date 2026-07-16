import os
import hashlib
import logging
from DependencyUtils import DependencyUtils

requests = DependencyUtils.import_required_module("requests")

class HttpClientUtils:
    @staticmethod
    def byte_array_to_string(array: bytes) -> str:
        return ''.join(f"{b:02X}" for b in array)

    @staticmethod
    def check_sha256(file_path: str, expected_hash: str) -> bool:
        try:
            with open(file_path, "rb") as f:
                sha256 = hashlib.sha256()
                for chunk in iter(lambda: f.read(4096), b""):
                    sha256.update(chunk)
                new_hash_string = HttpClientUtils.byte_array_to_string(sha256.digest())
                return expected_hash == new_hash_string
        except (IOError, PermissionError) as e:
            print(f"Exception: {e}")
            return False

    @staticmethod
    def download_valid_file(url: str, local_file_path: str, sha256_hash: str = None) -> bool:
        HttpClientUtils.download_file_synchronous(url, local_file_path)
        if os.path.exists(local_file_path):
            if sha256_hash is not None:
                if HttpClientUtils.check_sha256(local_file_path, sha256_hash):
                    return True
                else:
                    print(f"Failed to validate hash : {local_file_path}")
                    print(f"Failed to validate hash : {sha256_hash}")
                    return False
            else:
                logging.info("Checksum Validation did not occur because there was no 256 key")
                return True
        else:
            print(f"Download Failed URL: {url}")
            return False

    @staticmethod
    def head(url: str):
        """Return (content_length:int|None, last_modified:str|None) via an HTTP HEAD request.
        Returns (None, None) if HEAD fails or the server doesn't report a size — the caller
        then downloads to be safe."""
        try:
            resp = requests.head(url, allow_redirects=True, timeout=30)
            resp.raise_for_status()
            cl = resp.headers.get("Content-Length")
            last_modified = resp.headers.get("Last-Modified")
            size = int(cl) if cl is not None and str(cl).isdigit() else None
            return size, last_modified
        except Exception as e:
            print(f"  HEAD request failed ({e}); will download to be safe.")
            return None, None

    @staticmethod
    def remote_size(url: str):
        """Best-effort (content_length:int|None, last_modified:str|None) for a URL. Tries HEAD
        first; if the server doesn't support HEAD, falls back to a ranged GET that reads only
        the response headers (Content-Range total) without downloading the body."""
        size, last_modified = HttpClientUtils.head(url)
        if size is not None:
            return size, last_modified
        try:
            resp = requests.get(url, headers={"Range": "bytes=0-0"}, stream=True,
                                allow_redirects=True, timeout=30)
            resp.raise_for_status()
            probe_size = None
            content_range = resp.headers.get("Content-Range")   # e.g. "bytes 0-0/858822610"
            if content_range and "/" in content_range:
                tail = content_range.rsplit("/", 1)[-1].strip()
                if tail.isdigit():
                    probe_size = int(tail)
            if probe_size is None and resp.status_code == 200:
                # Server ignored the Range and returned 200; its Content-Length is the full size
                # (we never read the body, so nothing is downloaded).
                cl = resp.headers.get("Content-Length")
                if cl is not None and str(cl).isdigit():
                    probe_size = int(cl)
            last_modified = resp.headers.get("Last-Modified") or last_modified
            resp.close()
            return probe_size, last_modified
        except Exception as e:
            print(f"  Size probe (ranged GET) failed ({e}).")
            return None, last_modified

    @staticmethod
    def download_file_synchronous(url: str, dest_path: str):
        logging.info(f"Downloading URL: {url}")
        logging.info(f"Downloading: {dest_path}")
        if os.path.exists(dest_path):
            os.remove(dest_path)
        HttpClientUtils.download_file_task(url, dest_path)

    @staticmethod
    def download_file_task(url: str, file_name: str):
        response = requests.get(url, stream=True)
        response.raise_for_status()
        with open(file_name, "wb") as f:
            for chunk in response.iter_content(chunk_size=8192):
                if chunk:
                    f.write(chunk)
