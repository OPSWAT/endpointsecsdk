# Top-Level Structure

This documentation describes the directory structure and key files produced when extracting the **OPSWAT Endpoint Security SDK** package.

```
OPSWAT-SDK/
├── client/
├── extract/
└── output.txt
```

## Directories

- **client/**  
  Runtime binaries, shared libraries, and intelligence datasets used directly by the SDK on endpoints.

- **extract/**  
  Expanded, human-readable datasets, documentation, schemas, and compliance artifacts.
  Commonly used for offline/air-gapped, server-side ingestion, or compliance workflows.

- **output.txt**  
  Diagnostic output generated during extraction or packaging workflows.
