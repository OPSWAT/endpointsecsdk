import os
import shutil

from Constants import Constants
import Util

from Util import Util
from typing import Optional


class ClientFiles:

    @staticmethod
    def _copy_extracted_folder(dest_path, root_path, subfolder):
        source_file_folder = os.path.join(root_path, subfolder)
        Util.copy_directory(source_file_folder, dest_path, True)

    @staticmethod
    def _copy_extracted_file(dest_path, root_path, subfolder, filename):
        source_file_folder = os.path.join(root_path, subfolder)
        source_file_path = os.path.join(source_file_folder, filename)
        dest_file = os.path.join(dest_path, filename)

        if not os.path.exists(dest_path):
            os.makedirs(dest_path)

        shutil.copy2(source_file_path, dest_file)


    @staticmethod
    def find_file(root_path: str, file_name: str) -> Optional[str]:
        """
        Recursively searches for a file starting from a given root directory.

        Args:
            root_path: The root directory to start searching from.
            file_name: The name of the file to find (e.g., "wuov2_delta.dat").

        Returns:
            The full absolute path to the first matching file (case-insensitive),
            or None if not found.
        """
        if not os.path.isdir(root_path):
            print(f"Root path not found: {root_path}")
            return None

        target = file_name.lower()
        try:
            for dirpath, dirnames, filenames in os.walk(root_path, topdown=True, onerror=lambda e: None):
                # Case-insensitive filename comparison
                for fname in filenames:
                    if fname.lower() == target:
                        return os.path.abspath(os.path.join(dirpath, fname))
        except Exception as ex:
            print(f"Error searching for {file_name}: {ex}")

        return None

    @staticmethod
    def copy_windows_offline_dat_files(dest_path: str, catalog_path: str):
       
        """
        Python equivalent of the C# CopyWindowsOfflineDatFiles.

        Copies:
          - wuov2.dat from catalog root
          - wuov2_delta.dat from its located subdirectory under catalog_path
        """
        # Copy wuov2.dat from the catalog root
        ClientFiles._copy_extracted_file(dest_path, catalog_path, "", "wuov2.dat")

        # Find wuov2_delta.dat somewhere under catalog_path
        wuov2_delta_path = ClientFiles.find_file(catalog_path, "wuov2_delta.dat")
        if wuov2_delta_path is None:
            print("wuov2_delta.dat not found under catalog path.")
            return

        # Copy wuov2_delta.dat from its discovered subdirectory
        subdir = os.path.dirname(os.path.relpath(wuov2_delta_path, start=catalog_path))
        ClientFiles._copy_extracted_file(dest_path, catalog_path, subdir, "wuov2_delta.dat")


    @staticmethod
    def _copy_windows_architecture(extract_path, client_path, arch):
        print(f"Copying Windows Client Files with Architecture: {arch}")

        dest_path = os.path.join(client_path, f"windows/{arch}")
        source_path = os.path.join(extract_path, "windows")
        ClientFiles._copy_extracted_file(dest_path, source_path, f"bin/detection/{arch}/release", "libwaaddon.dll")
        ClientFiles._copy_extracted_file(dest_path, source_path, f"bin/detection/{arch}/release", "libwaapi.dll")
        ClientFiles._copy_extracted_file(dest_path, source_path, f"bin/detection/{arch}/release", "libwaheap.dll")
        ClientFiles._copy_extracted_file(dest_path, source_path, f"bin/detection/{arch}/release", "libwautils.dll")
        ClientFiles._copy_extracted_file(dest_path, source_path, f"bin/manageability/{arch}/release", "libwalocal.dll")
        ClientFiles._copy_extracted_file(dest_path, source_path, f"bin/vulnerability/{arch}/release", "libwavmodapi.dll")
        ClientFiles._copy_extracted_file(dest_path, source_path, f"bin/deviceinfo/{arch}/release", "libwadeviceinfo.dll")

        if arch == "arm64":
            ClientFiles._copy_extracted_file(dest_path, source_path, f"bin/manageability/{arch}/release", "wa_3rd_party_host_ARM64.exe")
        elif arch == "x64":
            ClientFiles._copy_extracted_file(dest_path, source_path, f"bin/manageability/{arch}/release", "wa_3rd_party_host_32.exe")
            ClientFiles._copy_extracted_file(dest_path, source_path, f"bin/manageability/{arch}/release", "wa_3rd_party_host_64.exe")
        elif arch == "win32":
            ClientFiles._copy_extracted_file(dest_path, source_path, f"bin/manageability/{arch}/release", "wa_3rd_party_host_32.exe")

        resource_path = os.path.join(extract_path, "compliance/windows/bin")
        ClientFiles._copy_extracted_file(dest_path, resource_path, "", "libwaresource.dll")

        patch_catalog_path = os.path.join(extract_path, Constants.CATALOG_EXTRACT_PATH)
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "ap_checksum.dat")
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "patch.dat")
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "v2mod.dat")
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "wiv-lite.dat")

        ClientFiles.copy_windows_offline_dat_files(dest_path, patch_catalog_path)

    @staticmethod
    def _copy_windows_files(extract_dir, client_path):
        ClientFiles._copy_windows_architecture(extract_dir, client_path, "x64")
        ClientFiles._copy_windows_architecture(extract_dir, client_path, "win32")
        ClientFiles._copy_windows_architecture(extract_dir, client_path, "arm64")

    @staticmethod
    def _copy_mac_files(extract_path, client_path):
        print("Copying Mac Client Files")

        dest_path = os.path.join(client_path, "mac")
        source_path = os.path.join(extract_path, "mac")
        ClientFiles._copy_extracted_file(dest_path, source_path, "bin/detection/release", "libwaaddon.dylib")
        ClientFiles._copy_extracted_file(dest_path, source_path, "bin/detection/release", "libwaapi.dylib")
        ClientFiles._copy_extracted_file(dest_path, source_path, "bin/detection/release", "libwautils.dylib")
        ClientFiles._copy_extracted_file(dest_path, source_path, "bin/manageability/release", "libwalocal.dylib")
        ClientFiles._copy_extracted_file(dest_path, source_path, "bin/vulnerability/release", "libwavmodapi.dylib")
        ClientFiles._copy_extracted_file(dest_path, source_path, "bin/deviceinfo/release", "libwadeviceinfo.dylib")

        resource_path = os.path.join(extract_path, "compliance/mac/bin")
        ClientFiles._copy_extracted_file(dest_path, resource_path, "", "libwaresource.dylib")

        patch_catalog_path = os.path.join(extract_path, Constants.CATALOG_EXTRACT_PATH)
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "ap_checksum_mac.dat")
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "patch_mac.dat")
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "v2mod.dat")
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "mav.dat")

    @staticmethod
    def _copy_linux_architecture(extract_path, client_path, arch):
        print(f"Copying Linux Client Files with Architecture: {arch}")

        dest_path = os.path.join(client_path, f"linux/{arch}")
        source_path = os.path.join(extract_path, "linux")
        ClientFiles._copy_extracted_folder(dest_path, source_path, f"bin/detection/{arch}/release")
        ClientFiles._copy_extracted_folder(dest_path, source_path, f"bin/deviceinfo/{arch}/release")
        ClientFiles._copy_extracted_folder(dest_path, source_path, f"bin/infection/{arch}/release")
        ClientFiles._copy_extracted_folder(dest_path, source_path, f"bin/manageability/{arch}/release")

        if arch != "arm64":
            ClientFiles._copy_extracted_folder(dest_path, source_path, f"bin/vulnerability/{arch}/release")

        resource_path = os.path.join(extract_path, f"compliance/linux/bin/{arch}")
        ClientFiles._copy_extracted_file(dest_path, resource_path, "", "libwaresource.so")

        patch_catalog_path = os.path.join(extract_path, Constants.CATALOG_EXTRACT_PATH)
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "ap_checksum.dat")
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "patch_linux.dat")
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "v2mod.dat")
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "liv.dat")

    @staticmethod
    def _copy_linux_files(extract_dir, client_path):
        ClientFiles._copy_linux_architecture(extract_dir, client_path, "x64")
        ClientFiles._copy_linux_architecture(extract_dir, client_path, "x86")
        ClientFiles._copy_linux_architecture(extract_dir, client_path, "arm64")

    @staticmethod
    def prepare_files(sdk_root):
        client_path = Util.get_client_path(sdk_root)
        extract_path = Util.get_extract_path(sdk_root)

        ClientFiles._copy_windows_files(extract_path, client_path)
        ClientFiles._copy_mac_files(extract_path, client_path)
        ClientFiles._copy_linux_files(extract_path, client_path)
