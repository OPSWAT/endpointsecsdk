import os
import sys
import platform
import socket
import logging
import shutil


logger = logging.getLogger("endpoint_vuln_scanner")


# OESIS OS type constants
OS_TYPE_WINDOWS = 1
OS_TYPE_LINUX = 2
OS_TYPE_MACOS = 4



def get_os_type():
    p = sys.platform
    if p == "win32":
        return OS_TYPE_WINDOWS
    elif p == "darwin":
        return OS_TYPE_MACOS
    elif p.startswith("linux"):
        return OS_TYPE_LINUX
    else:
        raise RuntimeError(f"Unsupported platform: {p}")


def get_lib_filename():
    os_type = get_os_type()
    if os_type == OS_TYPE_WINDOWS:
        return "libwaapi.dll"
    elif os_type == OS_TYPE_MACOS:
        return "libwaapi.dylib"
    elif os_type == OS_TYPE_LINUX:
        return "libwaapi.so"


def get_dat_files(dat_path):
    """Return dict of DAT file paths based on current platform."""
    os_type = get_os_type()
    files = {
        "v2mod": os.path.join(dat_path, "v2mod.dat"),
    }

    if os_type == OS_TYPE_WINDOWS:
        files["patch"] = os.path.join(dat_path, "patch.dat")
        files["ap_checksum"] = os.path.join(dat_path, "ap_checksum.dat")
        files["wuov2"] = os.path.join(dat_path, "wuov2.dat")
        files["wiv_lite"] = os.path.join(dat_path, "wiv-lite.dat")
    elif os_type == OS_TYPE_MACOS:
        files["patch"] = os.path.join(dat_path, "patch_mac.dat")
        files["ap_checksum"] = os.path.join(dat_path, "ap_checksum_mac.dat")
        files["mav"] = os.path.join(dat_path, "mav.dat")
    elif os_type == OS_TYPE_LINUX:
        files["patch"] = os.path.join(dat_path, "patch_linux.dat")
        files["ap_checksum"] = os.path.join(dat_path, "ap_checksum.dat")
        files["liv"] = os.path.join(dat_path, "liv.dat")

    return files


def get_architecture():
    machine = platform.machine().lower()
    if machine in ("x86_64", "amd64"):
        return "64-bit"
    elif machine in ("i386", "i686", "x86"):
        return "32-bit"
    elif machine in ("arm64", "aarch64"):
        return "arm64"
    return machine


def get_hostname():
    return socket.gethostname()


def find_sdk_in_repo(script_path):
    """Search 2 directories up from script_path for the OPSWAT-SDK client directory.

    Expected repo structure:
        <repo_root>/
          OPSWAT-SDK/
            client/
              linux/
                arm64/   x64/   x86/
              mac/                      <-- no arch subfolder on macOS
              windows/
                arm64/   win32/   x64/

    Returns the path to the correct architecture-specific client directory,
    or None if the structure is not found.
    """
    ARCH_MAP = {
        OS_TYPE_LINUX:   {"64-bit": "x64", "32-bit": "x86", "arm64": "arm64"},
        OS_TYPE_WINDOWS: {"64-bit": "x64", "32-bit": "win32", "arm64": "arm64"},
        OS_TYPE_MACOS:   {},
    }

    OS_DIR_MAP = {
        OS_TYPE_LINUX:   "linux",
        OS_TYPE_WINDOWS: "windows",
        OS_TYPE_MACOS:   "mac",
    }

    os_type = get_os_type()
    arch = get_architecture()

    search_root = os.path.abspath(script_path)
    for _ in range(2):
        search_root = os.path.dirname(search_root)
        opswat_sdk_dir = os.path.join(search_root, "OPSWAT-SDK")

        # If OPSWAT-SDK doesn't exist at this level, keep walking up.
        # After exhausting all levels, the caller will get a clear message.
        if not os.path.isdir(opswat_sdk_dir):
            continue

        # OPSWAT-SDK exists but client directory is missing
        candidate_client = os.path.join(opswat_sdk_dir, "client")
        if not os.path.isdir(candidate_client):
            logger.error(
                f"Found OPSWAT-SDK at {opswat_sdk_dir} but the 'client' directory "
                f"is missing. Please run SDK_Downloader to populate it."
            )
            return None

        os_dir = os.path.join(candidate_client, OS_DIR_MAP[os_type])
        if not os.path.isdir(os_dir):
            logger.warning(f"Found OPSWAT-SDK/client but no OS dir for {OS_DIR_MAP[os_type]}")
            return None

        if os_type == OS_TYPE_MACOS:
            return os_dir

        arch_subdir = ARCH_MAP[os_type].get(arch)
        if not arch_subdir:
            logger.warning(f"No SDK arch mapping for architecture: {arch}")
            return None

        arch_dir = os.path.join(os_dir, arch_subdir)
        if os.path.isdir(arch_dir):
            return arch_dir
        else:
            logger.warning(
                f"Found OPSWAT-SDK/client/{OS_DIR_MAP[os_type]} "
                f"but no arch dir: {arch_subdir}"
            )
            return None

    # Exhausted all levels without finding OPSWAT-SDK at all
    logger.error(
        f"OPSWAT-SDK directory not found in the 2 directories above {script_path}. "
        f"Please run SDK_Downloader to set up the SDK before running this tool."
    )
    return None

def resolve_sdk_lib_path(sdk_path):
    """Find the libwaapi library file in the given SDK path.

    If not found at sdk_path, searches 2 levels up for the OPSWAT-SDK repo
    client directory matching the current OS and architecture, and copies
    all files from that directory into sdk_path before returning the lib path.
    """

    lib_name = get_lib_filename()

    # Check directly in sdk_path
    direct = os.path.join(sdk_path, lib_name)
    if os.path.isfile(direct):
        return direct

    # Check in architecture subdirectory (e.g. sdk/x64/libwaapi.so)
    arch = get_architecture()
    arch_map = {
        "64-bit": "x64",
        "32-bit": ("win32" if get_os_type() == OS_TYPE_WINDOWS else "x86"),
        "arm64": "arm64",
    }
    subdir = arch_map.get(arch, "x64")
    in_subdir = os.path.join(sdk_path, subdir, lib_name)
    if os.path.isfile(in_subdir):
        return in_subdir

    # Not found locally — search the repo structure 2 levels up
    repo_arch_dir = find_sdk_in_repo(sdk_path)
    if repo_arch_dir:
        repo_lib = os.path.join(repo_arch_dir, lib_name)
        if os.path.isfile(repo_lib):
            os.makedirs(sdk_path, exist_ok=True)
            logger.info(f"SDK not found in {sdk_path}, copying from repo: {repo_arch_dir}")
            for filename in os.listdir(repo_arch_dir):
                src = os.path.join(repo_arch_dir, filename)
                dst = os.path.join(sdk_path, filename)
                if os.path.isfile(src):
                    shutil.copy2(src, dst)
                    logger.debug(f"  Copied {filename}")
            logger.info(f"SDK files copied to {sdk_path}")
            return os.path.join(sdk_path, lib_name)

    raise FileNotFoundError(
        f"Could not find {lib_name} in {sdk_path}, {sdk_path}/{subdir}, "
        f"or in the OPSWAT-SDK repo client directory 2 levels up."
    )