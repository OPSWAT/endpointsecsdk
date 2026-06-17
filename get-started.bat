@echo off
REM ===========================================================================
REM  OPSWAT Endpoint SDK - Getting Started (Windows)
REM
REM  1. Verifies the evaluation license files exist in eval-license\
REM  2. Ensures Python is available (installs it via winget if missing)
REM  3. Ensures the Python 'requests' package is installed
REM  4. Runs the SDK downloader (sdk-downloader\script\src\main.py) to populate OPSWAT-SDK\
REM ===========================================================================
setlocal EnableExtensions
set "ROOT=%~dp0"

echo ============================================================
echo   OPSWAT Endpoint SDK - Getting Started
echo   Repo root: %ROOT%
echo ============================================================

REM ---- 1. License files ---------------------------------------------------
echo.
echo [1/4] Checking eval-license files...
set "LICDIR=%ROOT%eval-license"
set "LICMISSING="
if exist "%LICDIR%\license.cfg"       (echo   found: license.cfg)       else (echo   MISSING: eval-license\license.cfg & set "LICMISSING=1")
if exist "%LICDIR%\pass_key.txt"      (echo   found: pass_key.txt)      else (echo   MISSING: eval-license\pass_key.txt & set "LICMISSING=1")
if exist "%LICDIR%\download_token.txt" (echo   found: download_token.txt) else (echo   MISSING: eval-license\download_token.txt & set "LICMISSING=1")
if defined LICMISSING (
    echo.
    echo ERROR: One or more license files are missing in "%LICDIR%".
    echo Place license.cfg, pass_key.txt and download_token.txt there.
    echo Need an evaluation license? Contact oem@opswat.com
    exit /b 1
)

REM ---- 2. Python ----------------------------------------------------------
echo.
echo [2/4] Checking for Python...
set "PYEXE="
call :detect_python
if not defined PYEXE (
    echo   Python not found. Attempting install via winget...
    where winget >nul 2>&1
    if errorlevel 1 (
        echo ERROR: winget is not available. Install Python 3 from
        echo        https://www.python.org/downloads/ and re-run this script.
        exit /b 1
    )
    winget install -e --id Python.Python.3.12 --accept-source-agreements --accept-package-agreements
    call :detect_python
)
if not defined PYEXE (
    echo.
    echo ERROR: Python is still not on PATH. It may have just been installed --
    echo close this window, open a new terminal, and run this script again.
    exit /b 1
)
echo   Using: %PYEXE%

REM ---- 3. requests --------------------------------------------------------
echo.
echo [3/4] Ensuring the Python 'requests' package...
%PYEXE% -m pip install --upgrade pip >nul 2>&1
%PYEXE% -m pip install requests
if errorlevel 1 (
    echo ERROR: Failed to install the 'requests' package.
    exit /b 1
)

REM ---- 4. Download the SDK ------------------------------------------------
echo.
echo [4/4] Downloading the SDK via sdk-downloader...
pushd "%ROOT%sdk-downloader\script\src"
%PYEXE% main.py
set "RC=%ERRORLEVEL%"
popd
if not "%RC%"=="0" (
    echo ERROR: SDK download failed with exit code %RC%.
    exit /b %RC%
)

echo.
echo ============================================================
echo   Setup complete. The SDK has been downloaded to OPSWAT-SDK\
echo   Next: open a sample (e.g. helloworld\python) and run it.
echo ============================================================
endlocal
exit /b 0

REM ---- subroutine: set PYEXE to a working Python launcher -----------------
:detect_python
py -3 --version >nul 2>&1 && set "PYEXE=py -3"
if not defined PYEXE ( python --version >nul 2>&1 && set "PYEXE=python" )
goto :eof
