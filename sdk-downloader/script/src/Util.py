import os
import shutil
import zipfile
import tarfile
import tempfile
import json

from datetime import datetime, timedelta


class Util:
    VCR_URL = "https://vcr.opswat.com/gw/file/download/%file%?type=1&token=%token%"

    # JSON files larger than this are left compact (not pretty-printed). Pretty-printing
    # loads the whole file into memory and inflates it on disk, which is impractical for the
    # huge catalog files (e.g. cves.json, vuln_system_associations.json).
    MAX_PRETTIFY_BYTES = 100 * 1024 * 1024  # 100 MB

    @staticmethod
    def extract_tar(tar_file_path, dest_dir):
        with tarfile.open(tar_file_path, "r") as tar:
            tar.extractall(path=dest_dir)
            return tar.getnames()

    @staticmethod
    def extract_file(archive_file, dest_dir):
        if archive_file.endswith(".zip"):
            print(f"Extracting ZIP file: {archive_file} to {dest_dir}")
            with zipfile.ZipFile(archive_file, 'r') as zip_ref:
                zip_ref.extractall(dest_dir)
                Util.prettify_json_files(zip_ref.namelist(), dest_dir)
        elif archive_file.endswith(".tar"):
            print(f"Extracting TAR file: {archive_file} to {dest_dir}")
            names = Util.extract_tar(archive_file, dest_dir)
            Util.prettify_json_files(names, dest_dir)

    @staticmethod
    def prettify_json_files(member_names, dest_dir):
        """Reformat every extracted .json file as indented ('pretty') JSON in place."""
        for name in member_names:
            if name.lower().endswith(".json"):
                Util.prettify_json(os.path.join(dest_dir, name))

    @staticmethod
    def prettify_json(file_path):
        """Rewrite a single JSON file with 2-space indentation in place. Leaves the file
        untouched if it is missing, unreadable, or not valid JSON (so non-JSON payloads
        that happen to use a .json extension are never corrupted)."""
        if not os.path.isfile(file_path):
            return
        size = os.path.getsize(file_path)
        if size > Util.MAX_PRETTIFY_BYTES:
            print(f"  Skipping JSON prettify (too large: {size // (1024 * 1024)} MB > "
                  f"{Util.MAX_PRETTIFY_BYTES // (1024 * 1024)} MB): {file_path}")
            return
        try:
            with open(file_path, "r", encoding="utf-8") as f:
                data = json.load(f)
        except (ValueError, OSError, UnicodeDecodeError) as e:
            print(f"  Skipping JSON prettify (unreadable/invalid): {file_path} ({e})")
            return
        try:
            with open(file_path, "w", encoding="utf-8") as f:
                json.dump(data, f, indent=2, ensure_ascii=False)
            print(f"  Prettified JSON: {file_path}")
        except OSError as e:
            print(f"  Failed to write prettified JSON: {file_path} ({e})")

    @staticmethod
    def get_clean_temp_dir(dir_name):
        temp_path = os.path.join(tempfile.gettempdir(), dir_name)
        Util.create_clean_dir(temp_path)
        return temp_path

    @staticmethod
    def create_clean_dir(dir_path):
        if os.path.exists(dir_path):
            shutil.rmtree(dir_path)
        os.makedirs(dir_path, exist_ok=True)

    @staticmethod
    def find_first_file(dir_path):
        if os.path.exists(dir_path):
            file_list = [f for f in os.listdir(dir_path) if os.path.isfile(os.path.join(dir_path, f))]
            if file_list:
                return os.path.join(dir_path, file_list[0])
        return None

    @staticmethod
    def is_file_updated(check_file):

        print(f"Checking if file {check_file} is updated in the last 7 days")
        if check_file is not None:
            if os.path.exists(check_file):
                last_write = datetime.fromtimestamp(os.path.getmtime(check_file))
                if last_write > datetime.now() - timedelta(days=7):
                    return True
        return False

    
    @staticmethod
    def get_sdk_root() -> str:
        """
        Searches upward from the current directory for a file named 'sdkroot'.
        Returns the directory where it's found, or the current directory if not found.
        """
        search_directory = os.getcwd()
        found = False

        while True:
            sdkroot_file = os.path.join(search_directory, "sdkroot")
            if os.path.isfile(sdkroot_file):
                found = True
                break

            parent_dir = os.path.dirname(search_directory)
            if parent_dir == search_directory:  # reached filesystem root
                break
            search_directory = parent_dir

        if not found:
            search_directory = os.getcwd()

        return search_directory


    @staticmethod
    def get_download_token() -> str:
        """
        Reads the download token from 'download_token.txt' in the current directory
        or from the '%sdkroot%/eval-license' directory if present.
        Raises an exception if the token file cannot be found.
        """
        sdk_token_file = "download_token.txt"

        # Check local directory first
        if not os.path.isfile(sdk_token_file):
            from pathlib import Path

            sdk_root = Util.get_sdk_root()
            license_path = os.path.join(sdk_root, "eval-license")

            if os.path.isdir(license_path):
                sdk_token_file = os.path.join(license_path, sdk_token_file)
                if not os.path.isfile(sdk_token_file):
                    raise FileNotFoundError(
                        f"Make sure there is a download token file available in the "
                        f"%sdk-root%/eval-license directory: {license_path}"
                    )
            else:
                raise FileNotFoundError(
                    f"Make sure there is a download token file available in the running "
                    f"directory: {os.getcwd()}"
                )

        # Read and return token
        with open(sdk_token_file, "r", encoding="utf-8") as f:
            download_token = f.read().strip()

        return download_token


    @staticmethod
    def get_token_download_url(file_name):
        result = Util.VCR_URL.replace("%token%", Util.get_download_token())
        result = result.replace("%file%", file_name)
        return result

    @staticmethod
    def get_archives_path(sdk_root):
        archive_path = os.path.join(sdk_root, "archives")
        if not os.path.exists(archive_path):
            os.makedirs(archive_path, exist_ok=True)
        return archive_path

    @staticmethod
    def get_extract_path(sdk_root):
        extract_path = os.path.join(sdk_root, "extract")
        if not os.path.exists(extract_path):
            os.makedirs(extract_path, exist_ok=True)
        return extract_path

    @staticmethod
    def get_client_path(sdk_root):
        client_path = os.path.join(sdk_root, "client")
        if not os.path.exists(client_path):
            os.makedirs(client_path, exist_ok=True)
        return client_path

    @staticmethod
    def copy_directory(source_dir, destination_dir, overwrite=True):
        os.makedirs(destination_dir, exist_ok=True)
        for file_name in os.listdir(source_dir):
            src_file = os.path.join(source_dir, file_name)
            dst_file = os.path.join(destination_dir, file_name)
            if os.path.isfile(src_file):
                if overwrite or not os.path.exists(dst_file):
                    shutil.copy2(src_file, dst_file)


