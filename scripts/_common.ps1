# Shared settings used by every script in this folder.
# If MySQL is installed somewhere else on your machine, change the
# path below - that is the only thing you should need to edit.

$RepoRoot    = Split-Path -Parent $PSScriptRoot
$DatabaseDir = Join-Path $RepoRoot "Database"
$BackendDir  = Join-Path $RepoRoot "Backend"
$FrontendDir = Join-Path $RepoRoot "Frontend"

# Only used to find mysql.exe (the command-line client) - this app
# connects to whichever MySQL *server* is listening on $MySqlPort
# below, regardless of which version installed this client.
$MySqlBinDir  = "C:\Program Files\MySQL\MySQL Server 8.4\bin"
$MySqlClient  = Join-Path $MySqlBinDir "mysql.exe"

# This project uses your machine's normal MySQL install/service
# (Windows service name "MySQL80"), on the default port.
$MySqlServiceName = "MySQL80"
$MySqlPort         = 3306

# The app itself connects as a dedicated low-privilege user, not root
# (see Database/cafeteria.sql, which creates it). Root is only needed
# once, by hand, to load the schema - see scripts/first-time-setup.ps1.
$AppDbPassword = "CafeteriaApp@123"

function Test-MySqlClientInstalled {
    if (-not (Test-Path $MySqlClient)) {
        Write-Host "Could not find the MySQL command-line client at: $MySqlClient" -ForegroundColor Red
        Write-Host "Install MySQL, or edit scripts/_common.ps1 to point at your install." -ForegroundColor Red
        return $false
    }
    return $true
}

function Test-MySqlRunning {
    $connection = Get-NetTCPConnection -LocalPort $MySqlPort -State Listen -ErrorAction SilentlyContinue
    return $null -ne $connection
}

# Finds dotnet.exe even if it's not on PATH yet. Installing the .NET SDK
# updates PATH, but a PowerShell/terminal window that was already open
# at the time won't see that change until it's reopened - rather than
# make you do that, we just fall back to the well-known install path.
function Get-DotnetExe {
    $onPath = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($onPath) { return $onPath.Source }

    $wellKnownPath = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $wellKnownPath) { return $wellKnownPath }

    return $null
}
