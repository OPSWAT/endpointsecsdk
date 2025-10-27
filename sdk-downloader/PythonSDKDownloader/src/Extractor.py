
import os
import shutil

import Util
import Constants

from Util import Util
from Constants import Constants

class Extractor:
    @staticmethod
    def extract_archives(archive_directory, extract_directory):
        if archive_directory and os.path.isdir(archive_directory):
            dir_list = os.listdir(archive_directory)
            for archive in dir_list:
                if archive.endswith(".zip") or archive.endswith(".tar"):
                    Util.extract_file(os.path.join(archive_directory, archive), extract_directory)
        else:
            print("Unable to extract because directory is null or non existent")

    @staticmethod
    def extract_platform(archive_root, extract_dir, platform):
        print(f"Extracting Platform: {platform}")
        platform_dir = os.path.join(archive_root, platform)
        platform_extract_dir = os.path.join(extract_dir, platform)
        Extractor.extract_archives(platform_dir, platform_extract_dir)

    @staticmethod
    def extract_dynamic_file(archive_root, extract_dir, dynamic_file):
        archive_file = os.path.join(archive_root, dynamic_file)
        if os.path.isfile(archive_file):
            print(f"Extracting Dynamic File: {dynamic_file}")
            Util.extract_file(archive_file, extract_dir)
        else:
            print(f"Dynamic file not found: {dynamic_file}")

    @staticmethod
    def extract(sdk_root):
        extract_root = Util.get_extract_path(sdk_root)
        archive_path = Util.get_archives_path(sdk_root)

        Util.create_clean_dir(extract_root)

        Extractor.extract_dynamic_file(archive_path, extract_root, Constants.CATALOG_FILE)
        Extractor.extract_dynamic_file(archive_path, extract_root, Constants.COMPLIANCE_FILE)

        Extractor.extract_platform(archive_path, extract_root, "linux")
        Extractor.extract_platform(archive_path, extract_root, "windows")
        Extractor.extract_platform(archive_path, extract_root, "mac")
