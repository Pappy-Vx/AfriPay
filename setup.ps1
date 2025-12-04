# AfriPay Setup Script for Windows PowerShell
# This script sets up the entire AfriPay development environment

# Requires -RunAsAdministrator is optional, but recommended for Docker operations

# Stop on errors
$ErrorActionPreference = "Stop"

# Function to print colored messages
function Print-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Cyan
}

function Print-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Print-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Print-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Print-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  $Message" -ForegroundColor Cyan
    Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
}

# Function to check if a command exists
function Test-CommandExists {
    param([string]$Command)
    $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
}

# Function to wait for user confirmation
function Wait-ForConfirmation {
    Write-Host "Press Enter to continue..." -ForegroundColor Gray
    $null = Read-Host
}

# Main script starts here
Clear-Host
Print-Header "AfriPay API Setup Script"
Write-Host "This script will set up your AfriPay development environment."
Write-Host "It will:"
Write-Host "  1. Check prerequisites (Docker, .NET SDK)"
Write-Host "  2. Build the Docker images"
Write-Host "  3. Start the database and API containers"
Write-Host "  4. Run database migrations"
Write-Host "  5. Display access information"
Write-Host ""
Wait-ForConfirmation

# Step 1: Check Prerequisites
Print-Header "Step 1: Checking Prerequisites"

Print-Info "Checking for Docker..."
if (Test-CommandExists docker) {
    $dockerVersion = docker --version
    Print-Success "Docker is installed: $dockerVersion"
} else {
    Print-Error "Docker is not installed!"
    Write-Host "Please install Docker Desktop from: https://docs.docker.com/desktop/install/windows/"
    exit 1
}

Print-Info "Checking if Docker daemon is running..."
try {
    docker info | Out-Null
    Print-Success "Docker daemon is running"
} catch {
    Print-Error "Docker daemon is not running!"
    Write-Host "Please start Docker Desktop and try again."
    exit 1
}

Print-Info "Checking for Docker Compose..."
try {
    if (Test-CommandExists docker-compose) {
        $composeVersion = docker-compose --version
        $composeCommand = "docker-compose"
    } else {
        $composeVersion = docker compose version
        $composeCommand = "docker compose"
    }
    Print-Success "Docker Compose is available: $composeVersion"
} catch {
    Print-Error "Docker Compose is not available!"
    exit 1
}

Print-Info "Checking for .NET SDK..."
if (Test-CommandExists dotnet) {
    $dotnetVersion = dotnet --version
    Print-Success ".NET SDK is installed: $dotnetVersion"
} else {
    Print-Warning ".NET SDK is not installed (optional for migrations)"
    Write-Host "If you need to run migrations manually, install from: https://dotnet.microsoft.com/download"
}

Write-Host ""
Print-Success "All prerequisites checked!"
Start-Sleep -Seconds 2

# Step 2: Clean up existing containers (if any)
Print-Header "Step 2: Cleaning Up Existing Containers"

Print-Info "Stopping any running AfriPay containers..."
try {
    if ($composeCommand -eq "docker-compose") {
        docker-compose down 2>$null
    } else {
        docker compose down 2>$null
    }
} catch {
    # Ignore errors if containers don't exist
}
Print-Success "Cleanup complete"
Start-Sleep -Seconds 1

# Step 3: Build Docker Images
Print-Header "Step 3: Building Docker Images"

Print-Info "Building AfriPay API image..."
Write-Host "This may take a few minutes on first run..." -ForegroundColor Gray

try {
    if ($composeCommand -eq "docker-compose") {
        docker-compose build --no-cache
    } else {
        docker compose build --no-cache
    }
    Print-Success "Docker images built successfully!"
} catch {
    Print-Error "Failed to build Docker images"
    Write-Host $_.Exception.Message
    exit 1
}
Start-Sleep -Seconds 1

# Step 4: Start Containers
Print-Header "Step 4: Starting Containers"

Print-Info "Starting SQL Server database..."
try {
    if ($composeCommand -eq "docker-compose") {
        docker-compose up -d sqlserver
    } else {
        docker compose up -d sqlserver
    }
} catch {
    Print-Error "Failed to start SQL Server container"
    exit 1
}

Print-Info "Waiting for SQL Server to be ready..."
Write-Host "This may take 20-30 seconds on first startup..." -ForegroundColor Gray

# Wait for SQL Server to be healthy
$maxWait = 60
$waited = 0
$healthy = $false

while ($waited -lt $maxWait) {
    try {
        $healthStatus = docker inspect afripay-sqlserver --format='{{.State.Health.Status}}' 2>$null
        if ($healthStatus -eq "healthy") {
            $healthy = $true
            break
        }
    } catch {
        # Container might not be ready yet
    }
    Write-Host "." -NoNewline
    Start-Sleep -Seconds 2
    $waited += 2
}

Write-Host ""
if (-not $healthy) {
    Print-Error "SQL Server failed to start within $maxWait seconds"
    Print-Info "Checking logs..."
    docker logs afripay-sqlserver
    exit 1
}

Print-Success "SQL Server is ready!"

Print-Info "Starting AfriPay API..."
try {
    if ($composeCommand -eq "docker-compose") {
        docker-compose up -d afripay-api
    } else {
        docker compose up -d afripay-api
    }
} catch {
    Print-Error "Failed to start API container"
    exit 1
}

Print-Info "Waiting for API to start..."
Start-Sleep -Seconds 5
Print-Success "API container started!"

# Step 5: Database Setup
Print-Header "Step 5: Setting Up Database"

Print-Info "The API will automatically apply database migrations on startup..."
Print-Info "Waiting for migrations to complete (15 seconds)..."
Start-Sleep -Seconds 15
Print-Success "Database should be initialized!"
Print-Info "The API applies migrations automatically using EF Core's Database.MigrateAsync()"

# Step 6: Display Status and Access Information
Print-Header "Setup Complete!"

Print-Success "AfriPay development environment is ready!"
Write-Host ""
Write-Host "📊 Service Status:" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray

if ($composeCommand -eq "docker-compose") {
    docker-compose ps
} else {
    docker compose ps
}

Write-Host ""
Write-Host "🌐 Access Information:" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "  API Base URL:        http://localhost:8080"
Write-Host "  Swagger UI:          http://localhost:8080/swagger"
Write-Host "  Database Server:     localhost:1433"
Write-Host "  Database Name:       AfriPay"
Write-Host "  Database User:       sa"
Write-Host "  Database Password:   YourStrong@Passw0rd"
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host ""

Write-Host "📝 Useful Commands:" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "  View logs:           docker compose logs -f"
Write-Host "  View API logs:       docker compose logs -f afripay-api"
Write-Host "  View DB logs:        docker compose logs -f sqlserver"
Write-Host "  Stop services:       docker compose down"
Write-Host "  Restart services:    docker compose restart"
Write-Host "  Rebuild & restart:   docker compose up -d --build"
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host ""

Print-Info "Checking API health..."
Start-Sleep -Seconds 3

# Try to check if API is responding
try {
    $response = Invoke-WebRequest -Uri "http://localhost:8080/swagger" -UseBasicParsing -TimeoutSec 5
    Print-Success "API is responding! You can now access it at http://localhost:8080"

    # Open browser
    Print-Info "Opening Swagger UI in your default browser..."
    Start-Process "http://localhost:8080/swagger"
} catch {
    Print-Warning "API might still be starting up..."
    Print-Info "Give it a few more seconds and check: http://localhost:8080/swagger"
}

Write-Host ""
Print-Success "🎉 Setup complete! Happy coding!"
Write-Host ""
