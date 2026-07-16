#Requires -Version 5.1
<#
    build.ps1 - build simple-proxy on Windows and stage the artifacts into bin/.

    Produces (release):
        bin\simple-proxy-demo.exe  demo executable
        bin\simple_proxy.dll       dynamic library (C ABI) for embedding
        bin\simple_proxy.dll.lib   import library for linking against the DLL
        bin\simple_proxy.h         C header (copied from include/)

    Requires the Rust toolchain (https://rustup.rs) and the MSVC build tools.

    Created by Chris Seiler - OPSWAT OEM Field CTO
#>
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $root
try {
    Write-Host "Building simple-proxy (release)..." -ForegroundColor Cyan
    # cargo writes progress to stderr; under ErrorActionPreference=Stop that would abort the
    # script, so relax it just for the native call and gate on the exit code instead.
    $prev = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    cargo build --release
    $code = $LASTEXITCODE
    $ErrorActionPreference = $prev
    if ($code -ne 0) { throw "cargo build failed ($code)" }

    $bin = Join-Path $root "bin"
    New-Item -ItemType Directory -Force -Path $bin | Out-Null
    $rel = Join-Path $root "target\release"

    $artifacts = @("simple-proxy-demo.exe", "simple_proxy.dll", "simple_proxy.dll.lib")
    foreach ($f in $artifacts) {
        $src = Join-Path $rel $f
        if (Test-Path $src) {
            Copy-Item $src -Destination $bin -Force
            Write-Host "  copied $f"
        } else {
            Write-Warning "  missing $f"
        }
    }

    $hdr = Join-Path $root "include\simple_proxy.h"
    if (Test-Path $hdr) { Copy-Item $hdr -Destination $bin -Force; Write-Host "  copied simple_proxy.h" }

    Write-Host "Done. Artifacts in $bin" -ForegroundColor Green
} finally {
    Pop-Location
}
