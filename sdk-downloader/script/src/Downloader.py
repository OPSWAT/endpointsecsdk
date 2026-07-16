import os
import xml.etree.ElementTree as ET
import Util
import Constants
import HttpClientUtils

from Util import Util
from Constants import Constants
from HttpClientUtils import HttpClientUtils
from DownloadStatus import DownloadStatus

class Downloader:
    @staticmethod
    def get_attribute(element, key):
        if element is not None and key in element.attrib:
            return element.attrib[key]
        return ""

    @staticmethod
    def _download_if_changed(key, url, local_path, status, sha256=None):
        """Compare the server size to the recorded size and download only when the file is new
        or its size changed; otherwise skip it (neither re-downloaded nor re-extracted). If the
        server size can't be determined up front, download to verify, then compare the
        downloaded size to the recorded size - if it matches, the content is unchanged, so the
        archive is removed and extraction is skipped. Records the new size / update date only
        when the file actually changed."""
        remote_size, last_modified = HttpClientUtils.remote_size(url)
        prev_size = status.get_size(key)

        # Fast path: server size confirms unchanged -> skip download AND extract.
        if remote_size is not None and prev_size is not None and remote_size == prev_size:
            print(f"  Unchanged (size {remote_size} bytes) - skipping download and extract: {key}")
            return False

        if prev_size is None:
            reason = "new"
        elif remote_size is None:
            reason = "size unverifiable up front - downloading to verify"
        else:
            reason = f"size changed {prev_size} -> {remote_size} bytes"
        print(f"  Downloading ({reason}): {key}")

        ok = HttpClientUtils.download_valid_file(url, local_path, sha256)
        if not (ok and os.path.exists(local_path)):
            print(f"  Download FAILED: {key}")
            return False

        downloaded_size = os.path.getsize(local_path)

        # Post-download check: even though we re-downloaded, if the size matches what we already
        # have then the content is unchanged -> remove the archive so the extractor ignores it,
        # and leave the recorded status as-is. This skips a needless re-extract.
        if prev_size is not None and downloaded_size == prev_size:
            print(f"  Re-downloaded but size unchanged ({downloaded_size} bytes) - skipping extract: {key}")
            try:
                os.remove(local_path)
            except OSError:
                pass
            return False

        status.record(key, url, downloaded_size, sha256, last_modified)
        return True

    @staticmethod
    def download_release_files(release_element, dest_path, platform, status):
        if release_element is not None:
            for package_element in release_element:
                if package_element.tag == "Package":
                    file_url = Downloader.get_attribute(package_element, "Link")
                    sha256_hash = Downloader.get_attribute(package_element, "sha256")

                    file_name = os.path.basename(file_url)
                    local_file_path = os.path.join(dest_path, file_name)

                    # Only Download the static release files
                    if ("Adapter" not in file_name and "offline" not in file_name and
                        (file_name.endswith(".zip") or file_name.endswith(".tar"))):
                        key = f"{platform}/{file_name}"
                        Downloader._download_if_changed(
                            key, file_url, local_file_path, status, sha256_hash or None)

    @staticmethod
    def download_platform(platform_element, dest_path, platform, status):
        for release_element in platform_element:
            if (release_element.tag == "Releases" and
                Downloader.get_attribute(release_element, "Name") == "OESIS Local V4"):
                latest_release_element = release_element.find("LatestRelease")
                Downloader.download_release_files(latest_release_element, dest_path, platform, status)

    @staticmethod
    def download_releases(sdk_dir, xml_description, status):
        print("Downloading Releases")
        doc = ET.fromstring(xml_description)
        for element in doc:
            platform_name = Downloader.get_attribute(element, "Name")
            if platform_name in ("Windows", "Linux", "Mac"):
                print(f"Platform: {platform_name}")
                platform_dir = os.path.join(sdk_dir, platform_name.lower())
                os.makedirs(platform_dir, exist_ok=True)
                Downloader.download_platform(element, platform_dir, platform_name.lower(), status)

    @staticmethod
    def get_sdk_url():
        return Util.get_token_download_url(Constants.DESCRIPTOR_FILE)

    @staticmethod
    def download_archives(archive_root, status):
        # The descriptor is always fetched (small; it provides the current file URLs) and is not
        # extracted, so it is not tracked in the download status.
        oesis_file_path = os.path.join(archive_root, Constants.DESCRIPTOR_FILE)
        HttpClientUtils.download_file_synchronous(Downloader.get_sdk_url(), oesis_file_path)
        if os.path.exists(oesis_file_path):
            with open(oesis_file_path, "r", encoding="utf-8") as f:
                xml_string = f.read()
            Downloader.download_releases(archive_root, xml_string, status)

    @staticmethod
    def download_dynamic_file(archive_root, file_name, status):
        print(f"Dynamic File: {file_name}")
        file_path = os.path.join(archive_root, file_name)
        file_url = Util.get_token_download_url(file_name)
        Downloader._download_if_changed(file_name, file_url, file_path, status)

    @staticmethod
    def download(sdk_root):
        archive_path = Util.get_archives_path(sdk_root)
        print(f"Downloading SDK to {archive_path}")
        Util.create_clean_dir(archive_path)

        # Load the persistent status so we can skip files whose size hasn't changed.
        status = DownloadStatus(sdk_root).load()

        Downloader.download_archives(archive_path, status)

        # Now the dynamic files
        Downloader.download_dynamic_file(archive_path, Constants.COMPLIANCE_FILE, status)
        Downloader.download_dynamic_file(archive_path, Constants.CATALOG_FILE, status)

        status.save()

        n = len(status.downloaded_this_run)
        if n:
            print(f"Downloaded {n} new/changed file(s): {', '.join(sorted(status.downloaded_this_run))}")
        else:
            print("All files are up-to-date - nothing downloaded (nothing to extract).")
        return status.downloaded_this_run
