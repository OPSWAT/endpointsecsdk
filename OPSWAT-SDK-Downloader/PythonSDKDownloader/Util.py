import os
import shutil
import zipfile
import tarfile

from datetime import datetime, timedelta


class Util:
    VCR_URL = "https://vcr.opswat.com/gw/file/download/%file%?type=1&token=%token%"

    @staticmethod
    def extract_tar(tar_file_path, dest_dir):
        with tarfile.open(tar_file_path, "r") as tar:
            tar.extractall(path=dest_dir)

    @staticmethod
    def extract_file(archive_file, dest_dir):
        print("Howdy")
        if archive_file.endswith(".zip"):
            print(f"Extracting ZIP file: {archive_file} to {dest_dir}")
            with zipfile.ZipFile(archive_file, 'r') as zip_ref:
                zip_ref.extractall(dest_dir)
        elif archive_file.endswith(".tar"):
            print(f"Extracting TAR file: {archive_file} to {dest_dir}")      
            Util.extract_tar(archive_file, dest_dir)

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
    def get_download_token():
        sdk_token_file = "download_token.txt"
        if not os.path.exists(sdk_token_file):
            raise Exception("Make sure there is a download token file available in the running directory: " + os.getcwd())
        with open(sdk_token_file, "r") as f:
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
