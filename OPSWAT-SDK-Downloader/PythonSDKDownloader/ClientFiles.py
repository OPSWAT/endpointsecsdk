import os
import shutil

import Util

from Util import Util


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

        patch_catalog_path = os.path.join(extract_path, "analog/client")
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "ap_checksum.dat")
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "patch.dat")
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "v2mod.dat")
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "wuo.dat")
        ClientFiles._copy_extracted_file(dest_path, patch_catalog_path, "", "wiv-lite.dat")

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

        patch_catalog_path = os.path.join(extract_path, "analog/client")
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

        patch_catalog_path = os.path.join(extract_path, "analog/client")
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
