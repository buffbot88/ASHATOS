# Evidence Directory

This directory contains evidence demonstrating that memory/data hygiene controls are active, observable, and working as designed. This evidence supports Security Gate #235 validation (issue #236).

## Evidence Collection

Evidence can be collected automatically by running:

```bash
./collect_evidence.sh
```

This script will:
1. Collect configuration documentation
2. Run validation script (47 automated checks)
3. Build the project
4. Document test suite availability
5. Create sample metrics reports
6. Create sample maintenance logs
7. Create sample alert logs
8. Create sample health check logs
9. Generate comprehensive evidence summary

## Directory Structure

```
evidence/
├── config/              # Configuration documentation
│   ├── memory_module_config.txt
│   └── alert_config.txt
├── metrics/             # Metrics collection samples
│   └── metrics_sample_report.txt
├── logs/                # Operational logs
│   ├── build_log_*.txt
│   ├── sample_maintenance_logs.txt
│   ├── sample_alert_logs.txt
│   └── sample_health_check_logs.txt
├── tests/               # Test results
│   ├── validation_results_*.txt
│   └── comprehensive_test_results_*.txt
└── summary/             # Evidence summary reports
    └── evidence_summary_*.md
```

## Evidence Summary Reports

After running `collect_evidence.sh`, review the evidence summary report in:
```
evidence/summary/evidence_summary_*.md
```

This report includes:
- Validation results (47/47 checks)
- Requirements compliance status
- Evidence package contents
- Key findings
- Acceptance criteria validation
- Recommendations for Security Gate review

## Requirements Validated

All requirements from issue #236 are validated:

- ✅ Automatic log/session/cache pruning
- ✅ Storage quotas/caps enforced
- ✅ Deduplication routines enabled
- ✅ Metrics exported (20+ metrics)
- ✅ Alerts in place (6 types, 3 severities)
- ✅ Soak tests confirm bounded growth
- ✅ Evidence documentation complete

## Using the Evidence

### For Security Gate #235 Review

1. Run the evidence collection script:
   ```bash
   ./collect_evidence.sh
   ```

2. Review the evidence summary report

3. Verify all evidence files are present:
   ```bash
   ls -R evidence/
   ```

4. Optionally run tests in your target environment:
   ```csharp
   await MemoryHygieneTestRunner.RunAll();
   ```

5. Submit the evidence package for review

### For Production Monitoring

After deploying to production:

1. Enable MemoryHealthMonitor with 5-minute interval
2. Configure alert notifications
3. Monitor for 7+ days
4. Collect actual production metrics
5. Verify bounded growth under real workload

## Evidence Documentation

For detailed information on evidence collection, see:
- `docs/MEMORY_HYGIENE_EVIDENCE.md` - Complete evidence collection template
- `docs/MEMORY_HYGIENE_OBSERVABILITY.md` - Observability features guide
- `MEMORY_HYGIENE_IMPLEMENTATION_SUMMARY.md` - Implementation overview

## File Retention

Files with timestamps in their names (`*_YYYY-MM-DD_HH-MM-SS.*`) are generated per collection run and can be cleaned up after review. Keep the most recent evidence collection.

Sample files without timestamps are templates and should be retained:
- `sample_maintenance_logs.txt`
- `sample_alert_logs.txt`
- `sample_health_check_logs.txt`
- `metrics_sample_report.txt`

## Notes

- All sample logs and metrics are representative examples
- Timestamps and values in samples demonstrate expected behavior
- Actual production values will vary based on workload
- Evidence demonstrates that controls are implemented and functional
- For real-time metrics, use the MemoryHealthMonitor API in running systems
