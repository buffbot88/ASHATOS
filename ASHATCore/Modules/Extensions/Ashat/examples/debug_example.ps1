# Example: Debug Session with Ashat
# PowerShell script for Windows users

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Ashat Example: Debugging with AI" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

$userId = Read-Host "Enter your user ID (or press Enter for 'debug_user')"
if ([string]::IsNullOrWhiteSpace($userId)) {
    $userId = "debug_user"
}

Write-Host ""
Write-Host "Common debugging scenarios:" -ForegroundColor Yellow
Write-Host "1. Fix null reference exception"
Write-Host "2. Performance optimization"
Write-Host "3. integration issue"
Write-Host ""

$scenario = Read-Host "Choose a scenario (1-3)"

$goal = switch ($scenario) {
    "1" { "Fix null reference exception in ChatModule when user joins non-existent room" }
    "2" { "Optimize database queries in UserProfile module" }
    "3" { "Fix integration between AICodeGen and ModuleSpawner modules" }
    default { "Debug and fix an issue in the system" }
}

Write-Host ""
Write-Host "Starting Ashat debug session..." -ForegroundColor Green
Write-Host ""
Write-Host "Goal: $goal" -ForegroundColor White
Write-Host ""

Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Run from ASHATCore directory:"
Write-Host "   dotnet run -- 'ashat start session $userId $goal'"
Write-Host ""
Write-Host "2. Continue with context:"
Write-Host "   dotnet run -- 'ashat continue $userId The error occurs in JoinRoomAsync method'"
Write-Host ""
Write-Host "3. Review and approve fix:"
Write-Host "   dotnet run -- 'ashat approve $userId'"
Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
