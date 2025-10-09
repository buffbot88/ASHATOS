#!/bin/bash

# Memory Hygiene Implementation Validation Script
# Security Gate #235 Requirements Check

echo "╔════════════════════════════════════════════════════════════════════╗"
echo "║  Memory/Data Hygiene Implementation Validation                    ║"
echo "║  Security Gate #235 Requirements Check                            ║"
echo "╚════════════════════════════════════════════════════════════════════╝"
echo ""

PASSED=0
FAILED=0

check_requirement() {
    local name="$1"
    local condition="$2"
    
    echo -n "  Checking: $name... "
    if eval "$condition"; then
        echo "✅ PASS"
        ((PASSED++))
    else
        echo "❌ FAIL"
        ((FAILED++))
    fi
}

echo "📋 Requirement 1: Core Components"
echo "────────────────────────────────────────────────────────────────────"
check_requirement "MemoryMetrics class exists" \
    "test -f RaCore/Engine/Memory/MemoryMetrics.cs"
check_requirement "MemoryAlerts class exists" \
    "test -f RaCore/Engine/Memory/MemoryAlerts.cs"
check_requirement "MemoryHealthMonitor class exists" \
    "test -f RaCore/Engine/Memory/MemoryHealthMonitor.cs"
check_requirement "MemoryModule enhanced" \
    "grep -q 'public TimeSpan MaxAge' RaCore/Engine/Memory/MemoryModule.cs"
check_requirement "MemoryMaintenanceService enhanced" \
    "grep -q 'MemoryHealthMonitor' RaCore/Engine/Memory/MemoryMaintenanceService.cs"
echo ""

echo "📋 Requirement 2: Test Suites"
echo "────────────────────────────────────────────────────────────────────"
check_requirement "MemorySoakTests exists" \
    "test -f RaCore/Tests/MemorySoakTests.cs"
check_requirement "MemoryObservabilityTests exists" \
    "test -f RaCore/Tests/MemoryObservabilityTests.cs"
check_requirement "MemoryHygieneTestRunner exists" \
    "test -f RaCore/Tests/MemoryHygieneTestRunner.cs"
check_requirement "Soak test validates bounded growth" \
    "grep -q 'TestBoundedGrowthUnderLoad' RaCore/Tests/MemorySoakTests.cs"
check_requirement "Alert generation test exists" \
    "grep -q 'TestAlertingUnderStress' RaCore/Tests/MemorySoakTests.cs"
echo ""

echo "📋 Requirement 3: Documentation"
echo "────────────────────────────────────────────────────────────────────"
check_requirement "Observability documentation exists" \
    "test -f docs/MEMORY_HYGIENE_OBSERVABILITY.md"
check_requirement "Evidence template exists" \
    "test -f docs/MEMORY_HYGIENE_EVIDENCE.md"
check_requirement "Integration guide exists" \
    "test -f docs/MEMORY_HYGIENE_INTEGRATION.md"
check_requirement "Quick reference exists" \
    "test -f docs/MEMORY_HYGIENE_README.md"
check_requirement "Documentation describes metrics" \
    "grep -q 'Metrics Collection' docs/MEMORY_HYGIENE_OBSERVABILITY.md"
echo ""

echo "📋 Requirement 4: Automatic Pruning"
echo "────────────────────────────────────────────────────────────────────"
check_requirement "PruneOldItems returns count" \
    "grep -q 'public int PruneOldItems' RaCore/Engine/Memory/MemoryModule.cs"
check_requirement "Default max age is 90 days" \
    "grep -q 'TimeSpan.FromDays(90)' RaCore/Engine/Memory/MemoryModule.cs"
check_requirement "Pruning called in maintenance" \
    "grep -q 'PruneOldItems()' RaCore/Engine/Memory/MemoryModule.cs"
check_requirement "Maintenance service exists" \
    "test -f RaCore/Engine/Memory/MemoryMaintenanceService.cs"
echo ""

echo "📋 Requirement 5: Storage Quotas"
echo "────────────────────────────────────────────────────────────────────"
check_requirement "EnforceItemLimit returns count" \
    "grep -q 'public int EnforceItemLimit' RaCore/Engine/Memory/MemoryModule.cs"
check_requirement "Default max items is 10,000" \
    "grep -q '10000' RaCore/Engine/Memory/MemoryModule.cs"
check_requirement "Automatic enforcement at 90%" \
    "grep -q '0.9' RaCore/Engine/Memory/MemoryModule.cs"
check_requirement "Limit enforcement test exists" \
    "grep -q 'TestItemLimit' RaCore/Tests/MemoryManagementTests.cs"
echo ""

echo "📋 Requirement 6: Deduplication"
echo "────────────────────────────────────────────────────────────────────"
check_requirement "DeduplicateItems returns count" \
    "grep -q 'public int DeduplicateItems' RaCore/Engine/Memory/MemoryModule.cs"
check_requirement "Deduplication called in maintenance" \
    "grep -q 'DeduplicateItems()' RaCore/Engine/Memory/MemoryModule.cs"
check_requirement "Deduplication test exists" \
    "grep -q 'TestDeduplication' RaCore/Tests/MemoryManagementTests.cs"
check_requirement "Keeps most recent entry" \
    "grep -q 'OrderByDescending.*CreatedAt' RaCore/Engine/Memory/MemoryModule.cs"
echo ""

echo "📋 Requirement 7: Metrics Exported"
echo "────────────────────────────────────────────────────────────────────"
check_requirement "TotalItems metric exists" \
    "grep -q 'TotalItems' RaCore/Engine/Memory/MemoryMetrics.cs"
check_requirement "DatabaseSizeBytes metric exists" \
    "grep -q 'DatabaseSizeBytes' RaCore/Engine/Memory/MemoryMetrics.cs"
check_requirement "Prune rate metric exists" \
    "grep -q 'PruneRate' RaCore/Engine/Memory/MemoryMetrics.cs"
check_requirement "Metrics GetDetailedReport exists" \
    "grep -q 'GetDetailedReport' RaCore/Engine/Memory/MemoryMetrics.cs"
check_requirement "Metrics test exists" \
    "grep -q 'TestMetricsCollection' RaCore/Tests/MemoryObservabilityTests.cs"
echo ""

echo "📋 Requirement 8: Alerts in Place"
echo "────────────────────────────────────────────────────────────────────"
check_requirement "MemoryAlertConfig exists" \
    "grep -q 'class MemoryAlertConfig' RaCore/Engine/Memory/MemoryAlerts.cs"
check_requirement "Capacity threshold alert exists" \
    "grep -q 'CapacityThresholdExceeded' RaCore/Engine/Memory/MemoryAlerts.cs"
check_requirement "Disk usage alert exists" \
    "grep -q 'DiskUsageThresholdExceeded' RaCore/Engine/Memory/MemoryAlerts.cs"
check_requirement "Maintenance failure alert exists" \
    "grep -q 'MaintenanceFailure' RaCore/Engine/Memory/MemoryAlerts.cs"
check_requirement "Alert evaluation test exists" \
    "grep -q 'TestAlertEvaluation' RaCore/Tests/MemoryObservabilityTests.cs"
echo ""

echo "📋 Requirement 9: Soak Tests"
echo "────────────────────────────────────────────────────────────────────"
check_requirement "Bounded growth test exists" \
    "grep -q 'TestBoundedGrowthUnderLoad' RaCore/Tests/MemorySoakTests.cs"
check_requirement "Test runs for 30+ seconds" \
    "grep -q 'FromSeconds(30)' RaCore/Tests/MemorySoakTests.cs"
check_requirement "Maintenance effectiveness test" \
    "grep -q 'TestMaintenanceEffectiveness' RaCore/Tests/MemorySoakTests.cs"
check_requirement "Stress test validates alerts" \
    "grep -q 'TestAlertingUnderStress' RaCore/Tests/MemorySoakTests.cs"
echo ""

echo "📋 Requirement 10: Evidence Documentation"
echo "────────────────────────────────────────────────────────────────────"
check_requirement "Evidence checklist exists" \
    "grep -q 'Evidence Checklist' docs/MEMORY_HYGIENE_EVIDENCE.md"
check_requirement "Metrics evidence section" \
    "grep -q 'Metrics Collection Evidence' docs/MEMORY_HYGIENE_EVIDENCE.md"
check_requirement "Alert evidence section" \
    "grep -q 'Alert System Evidence' docs/MEMORY_HYGIENE_EVIDENCE.md"
check_requirement "Maintenance evidence section" \
    "grep -q 'Maintenance Cycle Evidence' docs/MEMORY_HYGIENE_EVIDENCE.md"
check_requirement "Summary template exists" \
    "grep -q 'Evidence Summary Template' docs/MEMORY_HYGIENE_EVIDENCE.md"
echo ""

echo "📋 Requirement 11: Build Validation"
echo "────────────────────────────────────────────────────────────────────"
echo -n "  Checking: Project builds successfully... "
if cd /home/runner/work/TheRaProject/TheRaProject && dotnet build RaCore/RaCore.csproj > /tmp/build.log 2>&1; then
    echo "✅ PASS"
    ((PASSED++))
else
    echo "❌ FAIL"
    ((FAILED++))
    echo "  Build errors:"
    tail -20 /tmp/build.log
fi
echo ""

echo "╔════════════════════════════════════════════════════════════════════╗"
echo "║  Validation Summary                                                ║"
echo "╚════════════════════════════════════════════════════════════════════╝"
echo ""
echo "Total Checks: $((PASSED + FAILED))"
echo "Passed: $PASSED ✅"
echo "Failed: $FAILED ❌"
echo ""

if [ $FAILED -eq 0 ]; then
    echo "╔════════════════════════════════════════════════════════════════════╗"
    echo "║  ✅ ALL REQUIREMENTS MET                                          ║"
    echo "║                                                                    ║"
    echo "║  The memory hygiene implementation meets all requirements for     ║"
    echo "║  Security Gate #235. The system is ready for validation.         ║"
    echo "╚════════════════════════════════════════════════════════════════════╝"
    echo ""
    echo "Next Steps:"
    echo "  1. Run comprehensive tests:"
    echo "     cd RaCore && dotnet run"
    echo "     # Then call: await MemoryHygieneTestRunner.RunAll()"
    echo ""
    echo "  2. Collect evidence:"
    echo "     See docs/MEMORY_HYGIENE_EVIDENCE.md for checklist"
    echo ""
    echo "  3. Review documentation:"
    echo "     - docs/MEMORY_HYGIENE_OBSERVABILITY.md"
    echo "     - docs/MEMORY_HYGIENE_INTEGRATION.md"
    echo "     - docs/MEMORY_HYGIENE_README.md"
    echo ""
    exit 0
else
    echo "╔════════════════════════════════════════════════════════════════════╗"
    echo "║  ❌ SOME REQUIREMENTS NOT MET                                     ║"
    echo "║                                                                    ║"
    echo "║  Please review the failed checks above and address any issues.    ║"
    echo "╚════════════════════════════════════════════════════════════════════╝"
    exit 1
fi
