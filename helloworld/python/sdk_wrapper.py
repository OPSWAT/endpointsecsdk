import ctypes
import json
import logging
import os
import sys

from platform_utils import get_os_type, OS_TYPE_WINDOWS, OS_TYPE_MACOS, OS_TYPE_LINUX

logger = logging.getLogger("endpoint_vuln_scanner")


class SDKError(Exception):
    """Raised when an SDK call returns a negative error code."""
    def __init__(self, func_name, rc, detail=None):
        self.func_name = func_name
        self.rc = rc
        self.detail = detail
        msg = f"{func_name} failed with code {rc}"
        if detail:
            msg += f": {detail}"
        super().__init__(msg)


class OESISWrapper:
    """Python ctypes wrapper around the OESIS libwaapi native library."""

    def __init__(self, lib_path):
        self.lib_path = lib_path
        self._lib = None
        self._loaded = False

    def load(self):
        """Load the native library and configure function signatures."""
        lib_dir = os.path.dirname(os.path.abspath(self.lib_path))
        logger.info(f"Loading SDK library from directory: {lib_dir}")

        os_type = get_os_type()

        if os_type == OS_TYPE_WINDOWS:
            # On Windows, dependent DLLs (libwautils, libwaheap, etc.) must be
            # findable. Add the directory to both PATH and via add_dll_directory.
            existing_path = os.environ.get("PATH", "")
            if lib_dir not in existing_path:
                os.environ["PATH"] = lib_dir + ";" + existing_path
                logger.info(f"Added {lib_dir} to PATH")

            if hasattr(os, "add_dll_directory"):
                os.add_dll_directory(lib_dir)
                logger.info(f"Added DLL directory via os.add_dll_directory")

            # Windows SDK uses __stdcall calling convention
            self._lib = ctypes.WinDLL(self.lib_path)
        else:
            # On macOS/Linux, add library directory to search path so dependent
            # libraries (libwautils, libwalocal, etc.) can be found.
            env_var = "DYLD_LIBRARY_PATH" if os_type == OS_TYPE_MACOS else "LD_LIBRARY_PATH"
            existing = os.environ.get(env_var, "")
            if lib_dir not in existing:
                os.environ[env_var] = lib_dir + (":" + existing if existing else "")
            self._lib = ctypes.CDLL(self.lib_path)

        # wa_api_setup(const wchar_t* json_config, wchar_t** json_out) -> int
        self._lib.wa_api_setup.argtypes = [
            ctypes.c_wchar_p,
            ctypes.POINTER(ctypes.c_wchar_p)
        ]
        self._lib.wa_api_setup.restype = ctypes.c_int

        # wa_api_invoke(const wchar_t* json_in, wchar_t** json_out) -> int
        self._lib.wa_api_invoke.argtypes = [
            ctypes.c_wchar_p,
            ctypes.POINTER(ctypes.c_wchar_p)
        ]
        self._lib.wa_api_invoke.restype = ctypes.c_int

        # wa_api_free(wchar_t* json_data) -> int
        self._lib.wa_api_free.argtypes = [ctypes.c_wchar_p]
        self._lib.wa_api_free.restype = ctypes.c_int

        # wa_api_teardown() -> int
        self._lib.wa_api_teardown.argtypes = []
        self._lib.wa_api_teardown.restype = ctypes.c_int

        self._loaded = True

    def _call(self, func_name, json_in):
        """Call an SDK function that takes json_in and returns json_out via pointer."""
        if not self._loaded:
            raise RuntimeError("SDK not loaded. Call load() first.")

        logger.debug(f"{func_name} input: {json_in[:200]}")
        func = getattr(self._lib, func_name)
        json_out = ctypes.c_wchar_p()
        rc = func(json_in, ctypes.byref(json_out))

        result_str = None
        if json_out.value is not None:
            result_str = json_out.value
            self._lib.wa_api_free(json_out)

        return rc, result_str

    def setup(self, license_cfg_path, pass_key_path, debug_log_path=None):
        """Initialize the SDK with license configuration.

        On Windows, only passkey_string is needed.
        On Linux/macOS, license_bytes and license_key_bytes are also required.
        """
        # Read pass_key.txt
        with open(pass_key_path, "r", encoding="utf-8") as f:
            passkey = f.read().strip().replace("\r", "")

        config = {
            "passkey_string": passkey,
            "enable_pretty_print": True,
            "silent_mode": True,
        }

        os_type = get_os_type()

        # Linux and macOS require the full license
        if os_type in (OS_TYPE_LINUX, OS_TYPE_MACOS):
            with open(license_cfg_path, "r", encoding="utf-8") as f:
                license_data = json.load(f)
            config["license_key_bytes"] = license_data["license_key"]
            config["license_bytes"] = license_data["license"]
            if os_type == OS_TYPE_MACOS:
                config["restrict_bundle_search"] = (
                    "user_home|shared_folder|removable_media|"
                    "reminders|photos|calendars|contacts|music"
                )
        else:
            # Windows: also works with just passkey, but include license if available
            if os.path.isfile(license_cfg_path):
                with open(license_cfg_path, "r", encoding="utf-8") as f:
                    license_data = json.load(f)
                config["license_key_bytes"] = license_data["license_key"]
                config["license_bytes"] = license_data["license"]
            config["online_mode"] = True

        setup_json = {"config": config}
        if debug_log_path:
            setup_json["config_debug"] = {
                "debug_log_level": "ALL",
                "debug_log_output_path": debug_log_path,
            }

        json_str = json.dumps(setup_json)
        rc, result_str = self._call("wa_api_setup", json_str)

        if rc < 0:
            raise SDKError("wa_api_setup", rc, result_str)

        return json.loads(result_str) if result_str else {}

    def invoke(self, method_id, **params):
        """Call wa_api_invoke with the given method ID and parameters.

        Returns (rc, parsed_json_response).
        """
        invoke_input = {"method": method_id}
        # Normalize file paths to forward slashes for the SDK
        for key, value in params.items():
            if isinstance(value, str) and ("dat_input" in key or "path" in key):
                params[key] = value.replace("\\", "/")
        invoke_input.update(params)
        json_str = json.dumps({"input": invoke_input})

        rc, result_str = self._call("wa_api_invoke", json_str)

        parsed = {}
        if result_str:
            try:
                parsed = json.loads(result_str)
            except json.JSONDecodeError:
                parsed = {"raw": result_str}

        return rc, parsed

    def teardown(self):
        """Deinitialize the SDK."""
        if not self._loaded:
            return
        rc = self._lib.wa_api_teardown()
        if rc < 0:
            raise SDKError("wa_api_teardown", rc)
