# client/ – Runtime Libraries & Datasets

The `client/` directory contains platform-specific runtime components consumed by applications integrating the OPSWAT Endpoint Security SDK.

```
client/
├── linux/
├── mac/
└── windows/
```

## Common File Types

| File Pattern | Description |
|--------------|-------------|
| `libwa*` | Core OPSWAT SDK shared libraries |
| `libwaapi*` | Main posture and SDK API libraries |
| `libwadeviceinfo*` | Device inventory and system information |
| `libwavmodapi*` | Vulnerability and patch module interface |
| `libwaresource*` | Dynamic scripts for detecting application-specific details |
| `wadiagnose*` | Diagnostic and troubleshooting utilities |
| `*.dat` | Intelligence datasets (patches, vulnerabilities, updates) |

## Linux (`client/linux/`)

Architecture-specific folders: `arm64`, `x64`, `x86`.

Typical contents:
- Core SDK libraries (`.so`) such as `libwaapi.so`, `libwadeviceinfo.so`, `libwalocal.so`, `libwautils.so`, etc.
- Vulnerability datasets (e.g., `liv.dat`, `v2mod.dat`)
- Patch metadata (e.g., `patch_linux.dat`)
- Diagnostics (`wadiagnose`, `wadiagnose_legacy`)

## macOS (`client/mac/`)

Typical contents:
- Core SDK libraries (`.dylib`) such as `libwaapi.dylib`, `libwadeviceinfo.dylib`, `libwalocal.dylib`, `libwautils.dylib`
- Patch & vulnerability datasets (e.g., `patch_mac.dat`, `v2mod.dat`, `mav.dat`)

## Windows (`client/windows/`)

Architecture-specific folders: `arm64`, `win32`, `x64`.

Typical contents:
- Core SDK DLLs (e.g., `libwaapi.dll`, `libwadeviceinfo.dll`, `libwalocal.dll`, `libwautils.dll`, `libwavmodapi.dll`)
- Helper executables (`wa_3rd_party_host_*.exe`)
- Update & patch datasets (e.g., `wuo.dat`, `wuov2.dat`, `patch.dat`)
