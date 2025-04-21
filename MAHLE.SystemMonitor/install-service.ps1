# Requires -RunAsAdministrator

$serviceName = "MAHLESystemMonitor"
$serviceDisplayName = "MAHLE System Monitor"
$serviceDescription = "Monitors system resources, services, backups and network connectivity"
$exePath = Join-Path $PSScriptRoot "MAHLE.SystemMonitor.exe"

# Check if .NET Runtime is installed
$dotnetVersion = dotnet --version
if (-not $?) {
    Write-Host "ERROR: .NET Runtime is not installed. Please install .NET 8.0 or later." -ForegroundColor Red
    Exit 1
}

# Build the project in Release mode
Write-Host "Building project..." -ForegroundColor Yellow
dotnet publish -c Release -o "$PSScriptRoot\publish"
if (-not $?) {
    Write-Host "ERROR: Failed to build the project." -ForegroundColor Red
    Exit 1
}

# Stop and remove existing service if it exists
if (Get-Service $serviceName -ErrorAction SilentlyContinue) {
    Write-Host "Stopping and removing existing service..." -ForegroundColor Yellow
    Stop-Service $serviceName
    $existing = Get-WmiObject -Class Win32_Service -Filter "Name='$serviceName'"
    if ($existing) {
        $existing.delete()
    }
}

# Create new service using SC command
Write-Host "Creating new service..." -ForegroundColor Yellow
$binPath = "`"$PSScriptRoot\publish\MAHLE.SystemMonitor.exe`""
$result = sc.exe create $serviceName binpath= $binPath start= auto displayname= $serviceDisplayName
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create service. Exit code: $LASTEXITCODE" -ForegroundColor Red
    Exit 1
}

# Set service description
$result = sc.exe description $serviceName $serviceDescription
if ($LASTEXITCODE -ne 0) {
    Write-Host "WARNING: Failed to set service description." -ForegroundColor Yellow
}

# Configure service recovery options (restart on failure)
$result = sc.exe failure $serviceName reset= 86400 actions= restart/60000/restart/60000/restart/60000
if ($LASTEXITCODE -ne 0) {
    Write-Host "WARNING: Failed to set service recovery options." -ForegroundColor Yellow
}

# Start the service
Write-Host "Starting service..." -ForegroundColor Yellow
Start-Service $serviceName
if (-not $?) {
    Write-Host "ERROR: Failed to start the service." -ForegroundColor Red
    Exit 1
}

# Verify service status
$service = Get-Service $serviceName
Write-Host "`nService installation completed." -ForegroundColor Green
Write-Host "Service Name: $serviceName" -ForegroundColor White
Write-Host "Display Name: $serviceDisplayName" -ForegroundColor White
Write-Host "Status: $($service.Status)" -ForegroundColor White
Write-Host "Startup Type: $($service.StartType)" -ForegroundColor White

# Add note about configuration
Write-Host "`nIMPORTANT:" -ForegroundColor Yellow
Write-Host "1. Make sure to update the connection string in appsettings.json" -ForegroundColor White
Write-Host "2. Configure monitoring parameters in the database" -ForegroundColor White
Write-Host "3. Service logs can be found in the Windows Event Viewer" -ForegroundColor White