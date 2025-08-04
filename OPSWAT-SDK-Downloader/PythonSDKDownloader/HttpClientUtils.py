import os
import hashlib
import requests
import logging

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
