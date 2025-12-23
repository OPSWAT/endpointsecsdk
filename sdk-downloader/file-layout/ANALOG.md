# extract/analog/ – Analog Dataset

The `extract/analog/` directory contains structured product, patch, and vulnerability intelligence used by both SDK and server-side consumers.

## Key Files

- `header.json` – Dataset metadata and versioning information
- `How_to_use_Analog_files.pdf` – Official documentation for consuming Analog datasets

## Subdirectories

- **client/** – Client-consumable intelligence datasets (typically `.dat`)
- **server/** – Server-scale JSON datasets for backend ingestion
- **quality_analog/** – Schema-validated datasets for testing and validation
- **changes/** – Historical schema and dataset changes organized by change ID and date
