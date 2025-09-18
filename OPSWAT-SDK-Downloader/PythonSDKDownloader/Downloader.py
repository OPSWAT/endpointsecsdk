import os
import xml.etree.ElementTree as ET
import Util
import Constants
import HttpClientUtils
import concurrent.futures
import hashlib

from Util import Util
from Constants import Constants
from HttpClientUtils import HttpClientUtils

class Downloader:
    @staticmethod
    def file_hash_match(local_file_path, expected_hash):
        if os.path.exists(local_file_path):
            return False
        sha256 = hashlib.sha256()
        with open(local_file_path, "rb") as f:
            for chunk in iter(lambda: f.read(8192), b""):
                sha256.update(chunk)
            return sha256.hexdigest().upper() == expected_hash.upper()
    @staticmethod
    def get_attribute(element, key):
        if element is not None and key in element.attrib:
            return element.attrib[key]
        return ""

    @staticmethod
    #File check if exists before downloading
    def download_release_files(release_element, dest_path):
        def should_download(file_name):
            return ("Adapter" not in file_name and "offline" not in file_name and
                        (file_name.endswith(".zip") or file_name.endswith(".tar")))
        def download_task(file_url, local_file_path, sha256_hash):
            HttpClientUtils.download_valid_file(file_url, local_file_path, sha256_hash)
            return 
        task=[]
        if release_element is not None:
            for package_element in release_element:
                if package_element.tag == "Package":
                    file_url = Downloader.get_attribute(package_element, "Link")
                    sha256_hash = Downloader.get_attribute(package_element, "sha256")
                    file_name = os.path.basename(file_url)
                    local_file_path = os.path.join(dest_path, file_name)

                    # Only Download the static release files
                    if should_download(file_name):
                        task.append((file_url, local_file_path, sha256_hash))
        with concurrent.futures.ThreadPoolExecutor(max_threads=4) as executor:
            futures = [executor.submit(download_task, file_url, local_file_path, sha256_hash) for file_url, local_file_path, sha256_hash in task]
            concurrent.futures.wait(futures)
                        

    @staticmethod
    def download_platform(platform_element, dest_path):
        for release_element in platform_element:
            if (release_element.tag == "Releases" and
                Downloader.get_attribute(release_element, "Name") == "OESIS Local V4"):
                latest_release_element = release_element.find("LatestRelease")
                Downloader.download_release_files(latest_release_element, dest_path)

    @staticmethod
    def download_releases(sdk_dir, xml_description):
        print("Downloading Releases")
        doc = ET.fromstring(xml_description)
        for element in doc:
            platform_name = Downloader.get_attribute(element, "Name")
            if platform_name in ("Windows", "Linux", "Mac"):
                print(f"Downloading Platform: {platform_name}")
                platform_dir = os.path.join(sdk_dir, platform_name.lower())
                os.makedirs(platform_dir, exist_ok=True)
                Downloader.download_platform(element, platform_dir)

    @staticmethod
    def get_sdk_url():
        return Util.get_token_download_url(Constants.DESCRIPTOR_FILE)

    @staticmethod
    def download_archives(archive_root):
        oesis_file_path = os.path.join(archive_root, Constants.DESCRIPTOR_FILE)
        HttpClientUtils.download_file_synchronous(Downloader.get_sdk_url(), oesis_file_path)
        if os.path.exists(oesis_file_path):
            with open(oesis_file_path, "r", encoding="utf-8") as f:
                xml_string = f.read()
            Downloader.download_releases(archive_root, xml_string)

    @staticmethod
    def download_dynamic_file(archive_root, file_name):
        print(f"Downloading Dynamic File: {file_name}")
        file_path = os.path.join(archive_root, file_name)
        file_url = Util.get_token_download_url(file_name)
        print(f"File URL: {file_url}")
        HttpClientUtils.download_file_synchronous(file_url, file_path)

    @staticmethod
    def download(sdk_root):
        archive_path = Util.get_archives_path(sdk_root)
        first_file = Util.find_first_file(archive_path)
        # if not Util.is_file_updated(first_file):
        print(f"Downloading SDK to {archive_path}")
        Util.create_clean_dir(archive_path)
        Downloader.download_archives(archive_path)

        # Now Download the dynamic files
        Downloader.download_dynamic_file(archive_path, Constants.COMPLIANCE_FILE)
        Downloader.download_dynamic_file(archive_path, Constants.CATALOG_FILE)
