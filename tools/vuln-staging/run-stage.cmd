@echo off
rem ############################################################################################
rem ##  Vuln Staging - run-stage.cmd
rem ##
rem ##  One command for the test box. Plain batch, so PowerShell's execution policy does not
rem ##  apply. Works in two places:
rem ##
rem ##  From a repo clone (tools\vuln-staging):  does the whole pipeline on this machine --
rem ##      1. checks eval-license\ (license.cfg, pass_key.txt, download_token.txt)
rem ##      2. portable Python + the 'requests' wheels the SDK downloader needs (pinned SHA-256)
rem ##      3. sdk-downloader: OESIS SDK + Analog catalog into OPSWAT-SDK\ (skipped when present;
rem ##         --refresh-sdk forces it)
rem ##      4. helloworld\python\copy_sdk_files.py
rem ##      5. plan.py + fetch.py -> the bundle (default C:\vuln-staging, or %%VULN_STAGING_BUNDLE%%)
rem ##      6. hands over to the bundle's run-stage.cmd, which elevates and runs stage.py
rem ##
rem ##  At the end it launches AcmeScanner (tools\vapm-scanner) from the bundle's scanner\
rem ##  folder, after installing the .NET 9 Desktop runtime it needs (pinned to the staged
rem ##  .NET Runtime 9.0 version). --no-scanner skips both.
rem ##
rem ##  From a bundle (C:\vuln-staging, copied to an offline box): elevates, makes sure the
rem ##      portable Python is there, runs stage.py, logs to stage.log.
rem ##
rem ##  Python: the portable embeddable Python 3.12.10 (no installer, nothing registered with
rem ##  Windows, so it cannot collide with the Python 3.13 / 3.14 being staged or be detected
rem ##  as an installed product). Downloaded with curl.exe, checked with certutil, unpacked with
rem ##  tar.exe -- all built into Windows 10 1803+ / Server 2019+.
rem ##
rem ##  Usage:
rem ##      run-stage.cmd                          everything, install, pin, verify
rem ##      run-stage.cmd --apps firefox,7zip      just these
rem ##      run-stage.cmd --verify-only            re-check later; no elevation needed
rem ##      run-stage.cmd --refresh-sdk            (repo clone only) re-download SDK + catalog
rem ##      run-stage.cmd --no-scanner             do not install the Desktop runtime / launch AcmeScanner
rem ##
rem ##  Created by Chris Seiler - OPSWAT OEM Field CTO
rem ############################################################################################
setlocal EnableExtensions

set "HERE=%~dp0"
set "HERE=%HERE:~0,-1%"
set "PYVER=3.12.10"
set "PYZIP=python-%PYVER%-embed-amd64.zip"
set "PYURL=https://www.python.org/ftp/python/%PYVER%/%PYZIP%"
set "PYSHA=4acbed6dd1c744b0376e3b1cf57ce906f9dc9e95e68824584c8099a63025a3c3"
set "PYTHONIOENCODING=utf-8"

set "VERIFY="
echo.%* | find /i "--verify-only" >nul && set "VERIFY=1"

if "%HERE:~0,2%"=="\\" goto :network
net use %~d0 >nul 2>&1 && goto :network

if exist "%HERE%\fetch.py" goto :source_mode


rem ============================================================================================
rem  BUNDLE MODE
rem ============================================================================================
set "BUNDLE=%HERE%"
set "PYEXE=%BUNDLE%\python\python.exe"
set "LOG=%BUNDLE%\stage.log"

set "MISSING=0"
set "NEED_COPY="
set "NEED_FETCH="
set "NEED_SDK="
call :need stage.py            COPY  "the staging script"
call :need _staging.py         COPY  "app list and auto-update pin helpers"
call :need sdk_wrapper.py      COPY  "OESIS SDK Python wrapper"
call :need platform_utils.py   COPY  "OESIS SDK helper"
call :need staging.json        FETCH "what to install - written by fetch.py"
call :need installers          FETCH "the downloaded installers"
call :need sdk\libwaapi.dll    SDK   "OESIS SDK library"
call :need sdk\license.cfg     SDK   "SDK license"
call :need sdk\pass_key.txt    SDK   "SDK pass key"
call :need sdk\patchv2.dat     SDK   "patch database - GetPackages / InstallPackage"
call :need sdk\v2mod.dat       SDK   "vulnerability database - CVE check"
if not "%MISSING%"=="0" (
    echo.
    echo ERROR: %MISSING% item^(s^) missing from the bundle at
    echo          %BUNDLE%
    echo.
    echo How to fix:
    echo   - Easiest: run run-stage.cmd from tools\vuln-staging in a repo clone instead;
    echo     it builds a complete bundle itself.
    if defined NEED_COPY  echo   - Script files: copy the entire bundle folder, not individual files.
    if defined NEED_FETCH echo   - staging.json / installers: run  python fetch.py  ^(after plan.py^).
    if defined NEED_SDK   echo   - sdk\ files: run helloworld\python\copy_sdk_files.py, then fetch.py again.
    goto :fail
)
if not exist "%PYEXE%" if not exist "%BUNDLE%\%PYZIP%" (
    echo NOTE: portable Python is not in the bundle; it will be downloaded from python.org.
    echo       On an offline box this fails - re-run fetch.py to add python\ to the bundle.
)

net session >nul 2>&1
if errorlevel 1 if not defined VERIFY goto :elevate

rem --no-scanner is ours; everything else goes to stage.py
set "NOSCANNER="
set "STAGEARGS="
:bargloop
if "%~1"=="" goto :bargdone
if /i "%~1"=="--no-scanner" (set "NOSCANNER=1") else (set STAGEARGS=%STAGEARGS% %1)
shift
goto :bargloop
:bargdone

if not exist "%PYEXE%" call :provision_python || goto :fail

> "%LOG%" echo === run-stage %DATE% %TIME%  args: %STAGEARGS% ===

rem AcmeScanner needs the .NET 9 Windows Desktop runtime. Installed before staging (pinned to
rem the staged .NET version by fetch.py) so the manifest's final detection includes it.
if not defined NOSCANNER if not defined VERIFY call :desktop_runtime

"%PYEXE%" -u "%BUNDLE%\stage.py" %STAGEARGS% 2>&1 | "%PYEXE%" -c "import sys;f=open(sys.argv[1],'a',encoding='utf-8',errors='replace');[(sys.stdout.write(l),sys.stdout.flush(),f.write(l),f.flush()) for l in sys.stdin]" "%LOG%"

echo.
echo Log      : %LOG%
if exist "%BUNDLE%\results\manifest.md" echo Manifest : %BUNDLE%\results\manifest.md

if defined NOSCANNER goto :done
if not exist "%BUNDLE%\scanner\AcmeScanner.exe" (
    echo NOTE: no scanner\AcmeScanner.exe in the bundle - re-run fetch.py to add it.
    goto :done
)
echo.
echo Launching AcmeScanner from %BUNDLE%\scanner ...
start "" /D "%BUNDLE%\scanner" "%BUNDLE%\scanner\AcmeScanner.exe"
goto :done


rem ============================================================================================
rem  SOURCE MODE - running from tools\vuln-staging in a repo clone
rem ============================================================================================
:source_mode
for %%I in ("%HERE%\..\..") do set "REPO=%%~fI"
if not exist "%REPO%\sdkroot" (
    echo ERROR: expected the repo root two levels up ^(%REPO%^) with an 'sdkroot' file.
    echo        Run run-stage.cmd from tools\vuln-staging inside a full clone of the repo.
    goto :fail
)
if defined VULN_STAGING_BUNDLE (set "BUNDLE=%VULN_STAGING_BUNDLE%") else (set "BUNDLE=C:\vuln-staging")
set "PYEXE=%BUNDLE%\python\python.exe"
set "RUN="%PYEXE%" "%HERE%\_run.py""

rem split our own options from the ones meant for stage.py
set "REFRESH="
set "APPS="
set "STAGEARGS="
:argloop
if "%~1"=="" goto :argdone
if /i "%~1"=="--refresh-sdk" (set "REFRESH=1" & shift & goto :argloop)
if /i "%~1"=="--apps" set "APPS=%~2"
set STAGEARGS=%STAGEARGS% %1
shift
goto :argloop
:argdone

echo Vuln staging from the repo clone at %REPO%
echo Bundle folder: %BUNDLE%
echo.

rem --- 1. license ---------------------------------------------------------------------------
set "MISSING=0"
call :need_lic license.cfg        "SDK license"
call :need_lic pass_key.txt       "SDK pass key"
call :need_lic download_token.txt "token for the SDK / catalog download"
if not "%MISSING%"=="0" (
    echo.
    echo ERROR: the evaluation license is not in %REPO%\eval-license.
    echo        These files are personal and never in git. Copy your eval-license folder
    echo        ^(license.cfg, pass_key.txt, download_token.txt^) into
    echo          %REPO%\eval-license\
    echo        then run run-stage.cmd again. See eval-license\README.md.
    goto :fail
)
echo [1/6] License files found in eval-license\

rem --- 2. Python ----------------------------------------------------------------------------
if not exist "%BUNDLE%" mkdir "%BUNDLE%"
if not exist "%PYEXE%" call :provision_python || goto :fail
call :provision_requests || goto :fail
echo [2/6] Portable Python %PYVER% with requests ready

rem --- 3. SDK + catalog ---------------------------------------------------------------------
set "CATALOG=%REPO%\OPSWAT-SDK\extract\analog\server\patch_aggregation_v2.json"
set "PATCHDB=%REPO%\OPSWAT-SDK\extract\analog\client\patchv2.dat"
set "DOSDK="
if defined REFRESH set "DOSDK=1"
if not exist "%CATALOG%" set "DOSDK=1"
if not exist "%PATCHDB%" set "DOSDK=1"
if not defined DOSDK (
    echo [3/6] OESIS SDK and catalog already present ^(--refresh-sdk to re-download^)
    goto :sdk_done
)
echo [3/6] Downloading the OESIS SDK and catalog into %REPO%\OPSWAT-SDK
echo       This is several GB; the first run takes a while.
pushd "%REPO%"
%RUN% "%REPO%\sdk-downloader\script\src\main.py"
popd
rem The downloader can end with a harmless PermissionError on extract\linux, so check the files.
if not exist "%CATALOG%" (
    echo ERROR: the SDK download did not produce the catalog:
    echo          %CATALOG%
    echo        Check the messages above. A rejected or expired download_token.txt is the
    echo        usual cause; the box also needs internet access to vcr.opswat.com.
    goto :fail
)
if not exist "%PATCHDB%" (
    echo ERROR: the SDK download did not produce %PATCHDB%.
    goto :fail
)
:sdk_done

rem --- 4. SDK files into helloworld\python\sdk -----------------------------------------------
%RUN% "%REPO%\helloworld\python\copy_sdk_files.py"
if not exist "%REPO%\helloworld\python\sdk\libwaapi.dll" (
    echo ERROR: copy_sdk_files.py did not put libwaapi.dll in %REPO%\helloworld\python\sdk.
    echo        Check the messages above.
    goto :fail
)
echo [4/6] SDK files copied to helloworld\python\sdk

rem --- 5. plan + fetch ----------------------------------------------------------------------
echo [5/6] Choosing vulnerable versions and downloading installers into %BUNDLE%
set "VULN_STAGING_LAUNCHER=1"
%RUN% "%HERE%\plan.py" --out "%BUNDLE%\plan.json"
if errorlevel 1 (
    echo ERROR: plan.py failed - see the messages above.
    goto :fail
)
if defined APPS (
    %RUN% "%HERE%\fetch.py" --bundle "%BUNDLE%" --apps %APPS%
) else (
    %RUN% "%HERE%\fetch.py" --bundle "%BUNDLE%"
)
if errorlevel 1 (
    echo ERROR: fetch.py failed - see the messages above.
    goto :fail
)

rem --- 6. hand over to the bundle -----------------------------------------------------------
echo.
echo [6/6] Installing from the bundle %BUNDLE%
set "OUTER_NO_PAUSE=%VULN_STAGING_NO_PAUSE%"
set "VULN_STAGING_NO_PAUSE=1"
call "%BUNDLE%\run-stage.cmd" %STAGEARGS%
set "VULN_STAGING_NO_PAUSE=%OUTER_NO_PAUSE%"
goto :done


rem ============================================================================================
rem  subroutines
rem ============================================================================================
:elevate
echo Requesting elevation (accept the UAC prompt)...
if "%~1"=="" (
    powershell.exe -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs -Wait"
) else (
    powershell.exe -NoProfile -Command "Start-Process -FilePath '%~f0' -ArgumentList '%*' -Verb RunAs -Wait"
)
if exist "%LOG%" (
    echo.
    echo --- last lines of %LOG% ---
    powershell.exe -NoProfile -Command "Get-Content -LiteralPath '%LOG%' -Tail 30"
)
goto :done


:desktop_runtime
dir /b /ad "%ProgramFiles%\dotnet\shared\Microsoft.WindowsDesktop.App\9.*" >nul 2>&1 && (
    >> "%LOG%" echo .NET 9 Desktop runtime already present
    exit /b 0
)
set "DTR="
for %%E in ("%BUNDLE%\prereqs\windowsdesktop-runtime-9.*-win-x64.exe") do set "DTR=%%~fE"
if not defined DTR (
    echo NOTE: .NET 9 Desktop runtime is not installed and not in the bundle ^(prereqs\^);
    echo       AcmeScanner will ask for it. Re-run fetch.py to add it.
    >> "%LOG%" echo .NET 9 Desktop runtime missing and not in bundle
    exit /b 0
)
echo Installing %DTR% ^(needed by AcmeScanner^)...
"%DTR%" /install /quiet /norestart
set "RC=%ERRORLEVEL%"
>> "%LOG%" echo .NET Desktop runtime install %DTR% exit code %RC%
if "%RC%"=="0" exit /b 0
if "%RC%"=="3010" exit /b 0
if "%RC%"=="1638" exit /b 0
echo WARNING: .NET Desktop runtime install returned %RC%; AcmeScanner may not start.
exit /b 0


:need
rem  %1 = path inside the bundle, %2 = fix category, %3 = what it is
if exist "%BUNDLE%\%~1" exit /b 0
if "%MISSING%"=="0" echo Checking bundle %BUNDLE%
echo   MISSING  %~1   ^(%~3^)
set /a MISSING+=1
set "NEED_%~2=1"
exit /b 0


:need_lic
if exist "%REPO%\eval-license\%~1" exit /b 0
echo   MISSING  eval-license\%~1   ^(%~2^)
set /a MISSING+=1
exit /b 0


:sha256
rem  %1 = file; sets GOT to its lowercase-insensitive SHA-256 (no spaces)
set "GOT="
for /f "delims=" %%H in ('certutil -hashfile "%~1" SHA256 ^| findstr /v ":"') do if not defined GOT set "GOT=%%H"
if defined GOT set "GOT=%GOT: =%"
exit /b 0


:provision_python
set "ZIP=%BUNDLE%\%PYZIP%"
if not exist "%ZIP%" (
    echo Portable Python not found; downloading %PYURL%
    curl.exe -fsSL -o "%ZIP%" "%PYURL%"
    if errorlevel 1 (
        echo ERROR: could not download Python ^(no internet?^).
        echo        Copy %PYZIP% into %BUNDLE% and run again.
        if exist "%ZIP%" del "%ZIP%"
        exit /b 1
    )
)
call :sha256 "%ZIP%"
if /i not "%GOT%"=="%PYSHA%" (
    echo ERROR: %PYZIP% SHA-256 mismatch ^(got %GOT%^) - deleted, not used.
    del "%ZIP%"
    exit /b 1
)
if not exist "%BUNDLE%\python" mkdir "%BUNDLE%\python"
tar.exe -xf "%ZIP%" -C "%BUNDLE%\python"
if errorlevel 1 (
    echo ERROR: could not extract %PYZIP%.
    exit /b 1
)
echo Portable Python %PYVER% ready in %BUNDLE%\python
exit /b 0


:provision_requests
rem The SDK downloader needs 'requests'. Pure-Python wheels, pinned in wheels.txt, unpacked
rem straight into the portable Python's site-packages (it has no pip).
"%PYEXE%" -c "import requests" >nul 2>&1 && exit /b 0
set "SITE=%BUNDLE%\python\Lib\site-packages"
if not exist "%SITE%" mkdir "%SITE%"
if not exist "%BUNDLE%\wheels" mkdir "%BUNDLE%\wheels"
for /f "usebackq tokens=1-3" %%a in ("%HERE%\wheels.txt") do (
    call :wheel %%a %%b %%c || exit /b 1
)
findstr /x /c:"Lib\site-packages" "%BUNDLE%\python\python312._pth" >nul 2>&1 || (
    >> "%BUNDLE%\python\python312._pth" echo.
    >> "%BUNDLE%\python\python312._pth" echo Lib\site-packages
)
"%PYEXE%" -c "import requests" >nul 2>&1 && exit /b 0
echo ERROR: 'requests' still cannot be imported by the portable Python.
exit /b 1


:wheel
rem  %1 = file name, %2 = sha256, %3 = url
set "WHL=%BUNDLE%\wheels\%~1"
if not exist "%WHL%" (
    curl.exe -fsSL -o "%WHL%" "%~3"
    if errorlevel 1 (
        echo ERROR: could not download %~1 from %~3
        if exist "%WHL%" del "%WHL%"
        exit /b 1
    )
)
call :sha256 "%WHL%"
if /i not "%GOT%"=="%~2" (
    echo ERROR: %~1 SHA-256 mismatch ^(got %GOT%^) - deleted, not used.
    del "%WHL%"
    exit /b 1
)
tar.exe -xf "%WHL%" -C "%SITE%"
if errorlevel 1 (
    echo ERROR: could not unpack %~1
    exit /b 1
)
exit /b 0


:network
echo ERROR: run-stage.cmd is running from a network location:
echo          %HERE%
echo        The elevated installer cannot see mapped drives, and installing from a share is
echo        unreliable. Copy the folder to a local disk first, for example:
echo          robocopy "%HERE%" C:\temp\vuln-staging /E
echo        then run run-stage.cmd from there.
goto :fail


:fail
echo.
echo run-stage stopped before installing anything.
:done
if not defined VULN_STAGING_NO_PAUSE pause
endlocal
