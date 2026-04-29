import importlib
import importlib.util
import sys
import textwrap


class DependencyUtils:
    NON_STANDARD_MODULES = {
        "requests": {
            "package": "requests",
            "purpose": "download SDK files over HTTP/HTTPS",
        },
    }

    @staticmethod
    def import_required_module(module_name: str):
        """Import a required third-party module and show install help if missing."""
        if importlib.util.find_spec(module_name) is not None:
            return importlib.import_module(module_name)

        dependency = DependencyUtils.NON_STANDARD_MODULES.get(
            module_name,
            {"package": module_name, "purpose": "run this script"},
        )
        package_name = dependency["package"]
        purpose = dependency["purpose"]
        python_cmd = "py -m pip" if sys.platform.startswith("win") else "python3 -m pip"
        venv_activate = (
            r".venv\\Scripts\\activate" if sys.platform.startswith("win") else "source .venv/bin/activate"
        )

        raise ModuleNotFoundError(
            textwrap.dedent(
                f"""
                Required Python module '{module_name}' was not found.

                This script needs '{module_name}' to {purpose}. The module is not included with
                the Python standard library, so install the package before running this script.

                Install it with:
                    {python_cmd} install {package_name}

                If you are using a virtual environment, activate it first:
                    {venv_activate}
                    {python_cmd} install {package_name}

                If pip is not available, install/upgrade pip first:
                    {python_cmd} install --upgrade pip
                """
            ).strip()
        ) from None
