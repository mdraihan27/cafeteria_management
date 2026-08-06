# Starts the FastAPI backend server. Leave this window open while you
# use the app. The API will be at http://127.0.0.1:8000
# (interactive docs at http://127.0.0.1:8000/docs)

. "$PSScriptRoot\_common.ps1"

$venvPython = Join-Path $BackendDir "venv\Scripts\python.exe"
if (-not (Test-Path $venvPython)) {
    Write-Host "The backend hasn't been set up yet." -ForegroundColor Red
    Write-Host "Run scripts\first-time-setup.bat first." -ForegroundColor Red
    exit 1
}

if (-not (Test-MySqlRunning)) {
    Write-Host "Warning: MySQL doesn't seem to be running yet. Start it first with scripts\start-database.bat" -ForegroundColor Yellow
}

Push-Location $BackendDir
& $venvPython -m uvicorn main:app --host 127.0.0.1 --port 8000 --reload
Pop-Location
