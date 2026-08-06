# Makes sure your MySQL service is running. This project uses your
# machine's normal MySQL install (the "MySQL80" Windows service) on
# the default port 3306, so unlike the backend/frontend there's
# usually nothing to "start" here - Windows services are typically
# already running or set to start automatically.

. "$PSScriptRoot\_common.ps1"

if (Test-MySqlRunning) {
    Write-Host "MySQL is already running on port $MySqlPort. Nothing to do." -ForegroundColor Green
    exit 0
}

Write-Host "MySQL does not appear to be running on port $MySqlPort." -ForegroundColor Yellow
$service = Get-Service -Name $MySqlServiceName -ErrorAction SilentlyContinue
if ($service) {
    Write-Host "Found the '$MySqlServiceName' Windows service (status: $($service.Status))."
    Write-Host "Starting a Windows service requires an elevated (Administrator) PowerShell window."
    Write-Host "Open one and run:" -ForegroundColor Cyan
    Write-Host "    net start $MySqlServiceName" -ForegroundColor Cyan
    Write-Host "Or: open the Services app, find '$MySqlServiceName', right-click -> Start."
}
else {
    Write-Host "No '$MySqlServiceName' Windows service was found either." -ForegroundColor Red
    Write-Host "Install MySQL Server, or edit scripts/_common.ps1 if it's installed under a different service name/port."
}
exit 1
