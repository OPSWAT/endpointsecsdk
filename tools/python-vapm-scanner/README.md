# Python VAPM Scanner

A Python Vulnerability And Patch Management (VAPM) scanner built on the OESIS Framework SDK. It follows the same self-contained, copy-the-SDK-locally format as the [helloworld/python](../../helloworld/python/README.md) samples.

> **Status:** initial scaffolding — the scan logic in both modes is currently a **stub**.

## Assessment modes

| Folder | Description |
|--------|-------------|
| [`endpoint-assessment/`](endpoint-assessment/README.md) | Scans **this endpoint** (the local machine) directly via the SDK for vulnerabilities and missing patches. |
| [`centralized-assessment/`](centralized-assessment/README.md) | Assesses **externally-collected inventory** against the OESIS offline catalogs (centralized / server-side model), rather than scanning the local machine live. |

Each mode is self-contained: it has its own `copysdk.py` (which stages the SDK binaries and license into a local `sdk/` directory) plus copies of the shared `sdk_wrapper.py` and `platform_utils.py` helpers.

## Prerequisites

1. **Run the SDK downloader first** so the `OPSWAT-SDK/` directory is populated — see [sdk-downloader/README.md](../../sdk-downloader/README.md).
2. Place your license files (`license.cfg`, `pass_key.txt`, `download_token.txt`) in [`eval-license/`](../../eval-license/README.md) at the repository root.
3. Python 3.7+ (the samples use only the standard library).

## Usage

From either assessment folder:

```bash
cd endpoint-assessment        # or: cd centralized-assessment
python copysdk.py             # stage SDK binaries + license into ./sdk
python vapm_scanner.py        # run the scanner (stub for now)
```

## Layout

```
python-vapm-scanner/
├── endpoint-assessment/
│   ├── copysdk.py            # stage the SDK into ./sdk
│   ├── vapm_scanner.py       # endpoint patch + vulnerability scan (stub)
│   ├── sdk_wrapper.py        # ctypes wrapper around libwaapi
│   └── platform_utils.py     # platform/arch + SDK path helpers
└── centralized-assessment/
    ├── copysdk.py
    ├── vapm_scanner.py       # centralized assessment (stub)
    ├── sdk_wrapper.py
    └── platform_utils.py
```
