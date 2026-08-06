# Run this ONCE after cloning the project, before using any of the
# other scripts in this folder. It:
#   1. Creates the Python virtual environment for the Backend and
#      installs its dependencies.
#   2. Loads the database schema and sample data into your existing
#      MySQL server (this asks for your MySQL root password - it's
#      only used locally, right now, to run the two .sql files).
#   3. Restores and builds the Frontend (.NET) project.
#
# Safe to run again later - steps that were already done are skipped.
# Re-running step 2 will reset the database back to the sample data.

. "$PSScriptRoot\_common.ps1"

Write-Host "=== Step 1: Backend Python environment ===" -ForegroundColor Cyan
$venvPython = Join-Path $BackendDir "venv\Scripts\python.exe"
if (-not (Test-Path $venvPython)) {
    Write-Host "Creating virtual environment..."
    py -3.12 -m venv (Join-Path $BackendDir "venv")
    if (-not (Test-Path $venvPython)) {
        Write-Host "Python 3.12 was not found via the 'py' launcher. Install it, or create the venv yourself:" -ForegroundColor Red
        Write-Host "  py -3.12 -m venv Backend\venv" -ForegroundColor Red
        exit 1
    }
}
Write-Host "Installing backend dependencies (this can take a minute the first time)..."
& $venvPython -m pip install --upgrade pip --quiet
& $venvPython -m pip install -r (Join-Path $BackendDir "requirements.txt")

if (-not (Test-Path (Join-Path $BackendDir ".env"))) {
    Write-Host "Creating Backend/.env from the example template..."
    Copy-Item (Join-Path $BackendDir ".env.example") (Join-Path $BackendDir ".env")
}

Write-Host "`n=== Step 2: MySQL database ===" -ForegroundColor Cyan
if (-not (Test-MySqlClientInstalled)) { exit 1 }

if (-not (Test-MySqlRunning)) {
    Write-Host "MySQL isn't running on port $MySqlPort yet." -ForegroundColor Red
    Write-Host "Run scripts\start-database.bat for instructions, then re-run this script." -ForegroundColor Red
    exit 1
}

Write-Host "This project keeps the app's own database credentials separate from your"
Write-Host "MySQL root account (see Database/cafeteria.sql). Your root password is only"
Write-Host "used right now, locally, to load the schema - it is not saved anywhere."
$securePassword = Read-Host "Enter your MySQL root password" -AsSecureString
$rootPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword))

Write-Host "Loading schema (cafeteria.sql)..."
Get-Content (Join-Path $DatabaseDir "cafeteria.sql") | & $MySqlClient -h 127.0.0.1 -P $MySqlPort -u root "-p$rootPassword"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Could not load the schema - double check your root password and that MySQL is running." -ForegroundColor Red
    exit 1
}

Write-Host "Loading sample data (sample_data.sql)..."
Get-Content (Join-Path $DatabaseDir "sample_data.sql") | & $MySqlClient -h 127.0.0.1 -P $MySqlPort -u root "-p$rootPassword" cafeteria_db

$rootPassword = $null

Write-Host "`n=== Step 3: Frontend (.NET) ===" -ForegroundColor Cyan
$dotnetExe = Get-DotnetExe
if (-not $dotnetExe) {
    Write-Host "Could not find dotnet.exe. Install the .NET SDK, or if you just installed it," -ForegroundColor Red
    Write-Host "close and reopen this terminal (it needs a fresh PATH) and re-run this script." -ForegroundColor Red
    exit 1
}
Push-Location $FrontendDir
& $dotnetExe build
Pop-Location

Write-Host "`nAll set! Use scripts\start-backend.bat and start-frontend.bat to run the app." -ForegroundColor Green
