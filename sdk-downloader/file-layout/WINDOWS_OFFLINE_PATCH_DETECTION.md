# Windows Offline OS Patch Detection (WUO / WUOv2)

These datasets support **offline detection of Windows OS patches**.

## Files

- `wuo.dat`  
  **Deprecated** legacy Windows Offline OS patch detection dataset.  
  Superseded by `wuov2.dat` and not recommended for new integrations.

- `wuov2.dat`  
  **Primary** Windows Offline OS patch detection dataset containing full Windows Update intelligence.

- `wuov2_baseline.dat`  
  Baseline dataset used to enable delta-based (incremental) updates for Windows Offline OS patch detection.

- `wuov2_delta.dat`  
  Delta update dataset containing incremental changes.  
  Must be applied on top of the corresponding baseline dataset.

## Recommended Guidance

- New integrations should use **WUOv2** (`wuov2.dat`).
- For environments that frequently update datasets or have bandwidth constraints, use **baseline + delta** workflows.
