
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
            archives = [a for a in os.listdir(archive_directory)
                        if a.endswith(".zip") or a.endswith(".tar")]
            if not archives:
                print(f"  No new archives to extract in {archive_directory}")
                return
            for archive in archives:
                print(f"  Extracting new data: {archive}")
                Util.extract_file(os.path.join(archive_directory, archive), extract_directory)
        else:
            print(f"  Nothing downloaded for {os.path.basename(archive_directory)} this run "
                  f"- skipping (existing extracted data kept).")

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
            print(f"Extracting new data: {dynamic_file}")
            Util.extract_file(archive_file, extract_dir)
        else:
            print(f"Skipping {dynamic_file} - not downloaded this run "
                  f"(unchanged; existing extracted data kept).")

    @staticmethod
    def extract(sdk_root):
        # The extract directory is intentionally NOT cleaned: unchanged data from prior runs is
        # preserved, and only the archives downloaded THIS run (the only files present in the
        # archive directory) are (re)extracted. This is the "only extract new data" behavior.
        extract_root = Util.get_extract_path(sdk_root)
        archive_path = Util.get_archives_path(sdk_root)

        Extractor.extract_dynamic_file(archive_path, extract_root, Constants.CATALOG_FILE)
        Extractor.extract_dynamic_file(archive_path, extract_root, Constants.COMPLIANCE_FILE)

        Extractor.extract_platform(archive_path, extract_root, "linux")
        Extractor.extract_platform(archive_path, extract_root, "windows")
        Extractor.extract_platform(archive_path, extract_root, "mac")
