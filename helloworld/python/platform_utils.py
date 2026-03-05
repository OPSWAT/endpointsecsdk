import logging
import os
import platform
import shutil
import socket
import sys

logger = logging.getLogger("endpoint_vuln_scanner")

# OESIS OS type constants — these integer values match the OESIS SDK API contract
OS_TYPE_WINDOWS = 1
OS_TYPE_LINUX   = 2
OS_TYPE_MACOS   = 4


# ---------------------------------------------------------------------------
# Platform detection
# ---------------------------------------------------------------------------

def get_os_type():
    """Return the OESIS OS type constant for the current platform.

    Raises RuntimeError for unsupported platforms.
    """
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
    """Return the libwaapi filename for the current OS."""
    os_type = get_os_type()
    if os_type == OS_TYPE_WINDOWS:
        return "libwaapi.dll"
    elif os_type == OS_TYPE_MACOS:
        return "libwaapi.dylib"
    elif os_type == OS_TYPE_LINUX:
        return "libwaapi.so"
    else:
        raise RuntimeError(f"Cannot determine library filename for platform: {sys.platform}")


def get_architecture():
    """Return a normalised architecture string for the current CPU.

    Return values map directly to the subdirectory names used in the
    OPSWAT-SDK client directory structure.
    """
    machine = platform.machine().lower()
    if machine in ("x86_64", "amd64"):
        return "64-bit"
    elif machine in ("i386", "i686", "x86"):
        return "32-bit"
    elif machine in ("arm64", "aarch64"):
        return "arm64"
    return machine


def get_hostname():
    """Return the hostname of the current machine."""
    return socket.gethostname()


# ---------------------------------------------------------------------------
# DAT file paths
# ---------------------------------------------------------------------------

def get_dat_files(dat_path):
    """Return a dict of DAT file paths for the current platform.

    Each key is a logical name used by the scanner; each value is the
    absolute path to that DAT file under dat_path. Files are platform-specific
    because OPSWAT ships separate DATs for Windows, macOS, and Linux.
    """
    os_type = get_os_type()

    # v2mod.dat is cross-platform — covers 3rd-party application vulnerabilities
    files = {
        "v2mod": os.path.join(dat_path, "v2mod.dat"),
    }

    if os_type == OS_TYPE_WINDOWS:
        files["patch"]       = os.path.join(dat_path, "patch.dat")
        files["ap_checksum"] = os.path.join(dat_path, "ap_checksum.dat")
        files["wuov2"]       = os.path.join(dat_path, "wuov2.dat")
        files["wiv_lite"]    = os.path.join(dat_path, "wiv-lite.dat")
    elif os_type == OS_TYPE_MACOS:
        files["patch"]       = os.path.join(dat_path, "patch_mac.dat")
        files["ap_checksum"] = os.path.join(dat_path, "ap_checksum_mac.dat")
        files["mav"]         = os.path.join(dat_path, "mav.dat")
    elif os_type == OS_TYPE_LINUX:
        files["patch"]       = os.path.join(dat_path, "patch_linux.dat")
        files["ap_checksum"] = os.path.join(dat_path, "ap_checksum.dat")
        files["liv"]         = os.path.join(dat_path, "liv.dat")

    return files


# ---------------------------------------------------------------------------
# SDK repo discovery
# ---------------------------------------------------------------------------

def find_sdk_in_repo(script_path):
    """Search up to 2 directories above script_path for the OPSWAT-SDK client directory.

    Expected repo structure:
        <repo_root>/
          OPSWAT-SDK/
            client/
              linux/
                arm64/   x64/   x86/
              mac/                       <-- no arch subfolder on macOS
              windows/
                arm64/   win32/   x64/

    If OPSWAT-SDK is not found at either level the user is told to run
    SDK_Downloader. If the directory exists but the client folder is missing,
    the user is told to run SDK_Downloader to populate it.

    Returns the path to the correct OS/architecture client directory, or None.
    """
    # Maps OS type -> architecture string -> SDK subdirectory name
    ARCH_MAP = {
        OS_TYPE_LINUX:   {"64-bit": "x64", "32-bit": "x86",   "arm64": "arm64"},
        OS_TYPE_WINDOWS: {"64-bit": "x64", "32-bit": "win32", "arm64": "arm64"},
        OS_TYPE_MACOS:   {},  # macOS client directory is flat — no arch subfolder
    }

    # Maps OS type -> client OS subdirectory name
    OS_DIR_MAP = {
        OS_TYPE_LINUX:   "linux",
        OS_TYPE_WINDOWS: "windows",
        OS_TYPE_MACOS:   "mac",
    }

    os_type = get_os_type()
    arch    = get_architecture()

    search_root = os.path.abspath(script_path)
    for _ in range(2):
        search_root   = os.path.dirname(search_root)
        opswat_sdk_dir = os.path.join(search_root, "OPSWAT-SDK")

        # OPSWAT-SDK not present at this level — keep walking up
        if not os.path.isdir(opswat_sdk_dir):
            continue

        # OPSWAT-SDK found but client directory is absent — incomplete download
        candidate_client = os.path.join(opswat_sdk_dir, "client")
        if not os.path.isdir(candidate_client):
            logger.error(
                f"Found OPSWAT-SDK at {opswat_sdk_dir} but the 'client' "
                f"directory is missing. Please run SDK_Downloader to populate it."
            )
            return None

        # client found — check for the OS subdirectory
        os_dir = os.path.join(candidate_client, OS_DIR_MAP[os_type])
        if not os.path.isdir(os_dir):
            logger.warning(
                f"Found OPSWAT-SDK/client but no OS directory for "
                f"'{OS_DIR_MAP[os_type]}'. Please run SDK_Downloader."
            )
            return None

        # macOS client directory is flat — return it directly
        if os_type == OS_TYPE_MACOS:
            return os_dir

        # Linux and Windows have an architecture subdirectory
        arch_subdir = ARCH_MAP[os_type].get(arch)
        if not arch_subdir:
            logger.warning(f"No SDK architecture mapping for: {arch}")
            return None

        arch_dir = os.path.join(os_dir, arch_subdir)
        if os.path.isdir(arch_dir):
            return arch_dir

        logger.warning(
            f"Found OPSWAT-SDK/client/{OS_DIR_MAP[os_type]} "
            f"but no architecture directory: {arch_subdir}"
        )
        return None

    # Exhausted both levels without finding OPSWAT-SDK at all
    logger.error(
        f"OPSWAT-SDK directory not found in the 2 directories above "
        f"{script_path}. Please run SDK_Downloader to set up the SDK "
        f"before running this tool."
    )
    return None


# ---------------------------------------------------------------------------
# Environment validation
# ---------------------------------------------------------------------------

def validate_sdk_environment(sdk_dir):
    """Check that the sdk directory is ready to use before running any sample.

    Verifies:
      1. The sdk directory exists
      2. The correct libwaapi library for this platform is present
         (libwaapi.dll on Windows, libwaapi.dylib on macOS, libwaapi.so on Linux)
      3. license.cfg exists in the sdk directory
      4. pass_key.txt exists in the sdk directory

    Prints a clear message for each missing item and returns False if anything
    is wrong, so callers can exit immediately with a prompt to run copy_sdk_files.py.

    Returns True if all checks pass, False otherwise.
    """
    lib_name = get_lib_filename()
    ok = True

    if not os.path.isdir(sdk_dir):
        print(f"  ERROR: sdk directory not found: {sdk_dir}")
        return False

    if not os.path.isfile(os.path.join(sdk_dir, lib_name)):
        print(f"  ERROR: SDK library not found: {lib_name} (expected in {sdk_dir})")
        ok = False

    if not os.path.isfile(os.path.join(sdk_dir, "license.cfg")):
        print(f"  ERROR: license.cfg not found in {sdk_dir}")
        ok = False

    if not os.path.isfile(os.path.join(sdk_dir, "pass_key.txt")):
        print(f"  ERROR: pass_key.txt not found in {sdk_dir}")
        ok = False

    if not ok:
        print()
        print("Run copy_sdk_files.py to prepare the environment before running this sample:")
        print("    python copy_sdk_files.py")

    return ok


# ---------------------------------------------------------------------------
# SDK library resolution
# ---------------------------------------------------------------------------

def resolve_sdk_lib_path(sdk_path):
    """Find the libwaapi library file for the current OS and architecture.

    Search order:
      1. Directly in sdk_path (e.g. sdk/libwaapi.so)
      2. In an architecture subdirectory under sdk_path (e.g. sdk/x64/libwaapi.so)
      3. In the OPSWAT-SDK repo client directory up to 2 levels above sdk_path.
         If found there, all files in that directory are copied into sdk_path
         so subsequent runs find them locally without needing the repo.

    Raises FileNotFoundError if the library cannot be found anywhere and
    the OPSWAT-SDK repo structure is also absent.
    """
    lib_name = get_lib_filename()

    # 1. Direct path
    direct = os.path.join(sdk_path, lib_name)
    if os.path.isfile(direct):
        return direct

    # 2. Architecture subdirectory
    arch = get_architecture()
    arch_map = {
        "64-bit": "x64",
        "32-bit": "win32" if get_os_type() == OS_TYPE_WINDOWS else "x86",
        "arm64":  "arm64",
    }
    subdir   = arch_map.get(arch, "x64")
    in_subdir = os.path.join(sdk_path, subdir, lib_name)
    if os.path.isfile(in_subdir):
        return in_subdir

    # 3. Repo fallback — search OPSWAT-SDK/client/<os>/<arch> up to 2 levels up
    repo_arch_dir = find_sdk_in_repo(sdk_path)
    if repo_arch_dir:
        repo_lib = os.path.join(repo_arch_dir, lib_name)
        if os.path.isfile(repo_lib):
            os.makedirs(sdk_path, exist_ok=True)
            logger.info(
                f"SDK not found in {sdk_path} — copying from repo: {repo_arch_dir}"
            )
            for filename in os.listdir(repo_arch_dir):
                src = os.path.join(repo_arch_dir, filename)
                dst = os.path.join(sdk_path, filename)
                if os.path.isfile(src):
                    shutil.copy2(src, dst)
                    logger.debug(f"  Copied {filename}")
            logger.info(f"SDK files copied to {sdk_path}")
            return os.path.join(sdk_path, lib_name)

    raise FileNotFoundError(
        f"Could not find {lib_name} in '{sdk_path}', '{sdk_path}/{subdir}', "
        f"or in the OPSWAT-SDK repo client directory 2 levels up. "
        f"Please run SDK_Downloader to set up the SDK."
    )
