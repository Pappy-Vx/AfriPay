#!/bin/bash

# AfriPay Setup Script
# This script sets up the entire AfriPay development environment

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored messages
print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_header() {
    echo ""
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  $1${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo ""
}

# Function to check if a command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Function to wait for user confirmation
wait_for_confirmation() {
    read -p "Press Enter to continue..."
}

# Main script starts here
clear
print_header "AfriPay API Setup Script"
echo "This script will set up your AfriPay development environment."
echo "It will:"
echo "  1. Check prerequisites (Docker, .NET SDK)"
echo "  2. Build the Docker images"
echo "  3. Start the database and API containers"
echo "  4. Run database migrations"
echo "  5. Display access information"
echo ""
wait_for_confirmation

# Step 1: Check Prerequisites
print_header "Step 1: Checking Prerequisites"

print_info "Checking for Docker..."
if command_exists docker; then
    DOCKER_VERSION=$(docker --version)
    print_success "Docker is installed: $DOCKER_VERSION"
else
    print_error "Docker is not installed!"
    echo "Please install Docker from: https://docs.docker.com/get-docker/"
    exit 1
fi

print_info "Checking if Docker daemon is running..."
if docker info >/dev/null 2>&1; then
    print_success "Docker daemon is running"
else
    print_error "Docker daemon is not running!"
    echo "Please start Docker Desktop or the Docker daemon and try again."
    exit 1
fi

print_info "Checking for Docker Compose..."
if command_exists docker-compose || docker compose version >/dev/null 2>&1; then
    if command_exists docker-compose; then
        COMPOSE_VERSION=$(docker-compose --version)
    else
        COMPOSE_VERSION=$(docker compose version)
    fi
    print_success "Docker Compose is available: $COMPOSE_VERSION"
else
    print_error "Docker Compose is not available!"
    exit 1
fi

print_info "Checking for .NET SDK..."
if command_exists dotnet; then
    DOTNET_VERSION=$(dotnet --version)
    print_success ".NET SDK is installed: $DOTNET_VERSION"
else
    print_warning ".NET SDK is not installed (optional for migrations)"
    echo "If you need to run migrations manually, install from: https://dotnet.microsoft.com/download"
fi

echo ""
print_success "All prerequisites checked!"
sleep 2

# Step 2: Clean up existing containers (if any)
print_header "Step 2: Cleaning Up Existing Containers"

print_info "Stopping any running AfriPay containers..."
if docker compose version >/dev/null 2>&1; then
    docker compose down >/dev/null 2>&1 || true
else
    docker-compose down >/dev/null 2>&1 || true
fi
print_success "Cleanup complete"
sleep 1

# Step 3: Build Docker Images
print_header "Step 3: Building Docker Images"

print_info "Building AfriPay API image..."
echo "This may take a few minutes on first run..."
if docker compose version >/dev/null 2>&1; then
    docker compose build --no-cache
else
    docker-compose build --no-cache
fi
print_success "Docker images built successfully!"
sleep 1

# Step 4: Start Containers
print_header "Step 4: Starting Containers"

print_info "Starting SQL Server database..."
if docker compose version >/dev/null 2>&1; then
    docker compose up -d sqlserver
else
    docker-compose up -d sqlserver
fi

print_info "Waiting for SQL Server to be ready..."
echo "This may take 20-30 seconds on first startup..."

# Wait for SQL Server to be healthy
MAX_WAIT=60
WAITED=0
while [ $WAITED -lt $MAX_WAIT ]; do
    if docker inspect afripay-sqlserver --format='{{.State.Health.Status}}' 2>/dev/null | grep -q "healthy"; then
        break
    fi
    echo -n "."
    sleep 2
    WAITED=$((WAITED + 2))
done

echo ""
if [ $WAITED -ge $MAX_WAIT ]; then
    print_error "SQL Server failed to start within $MAX_WAIT seconds"
    print_info "Checking logs..."
    docker logs afripay-sqlserver
    exit 1
fi

print_success "SQL Server is ready!"

print_info "Starting AfriPay API..."
if docker compose version >/dev/null 2>&1; then
    docker compose up -d afripay-api
else
    docker-compose up -d afripay-api
fi

print_info "Waiting for API to start..."
sleep 5
print_success "API container started!"

# Step 5: Database Setup
print_header "Step 5: Setting Up Database"

print_info "The API will automatically apply database migrations on startup..."
print_info "Waiting for migrations to complete (15 seconds)..."
sleep 15
print_success "Database should be initialized!"
print_info "The API applies migrations automatically using EF Core's Database.MigrateAsync()"

# Step 6: Display Status and Access Information
print_header "Setup Complete!"

print_success "AfriPay development environment is ready!"
echo ""
echo "📊 Service Status:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
if docker compose version >/dev/null 2>&1; then
    docker compose ps
else
    docker-compose ps
fi
echo ""

echo "🌐 Access Information:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  API Base URL:        http://localhost:8080"
echo "  Swagger UI:          http://localhost:8080/swagger"
echo "  Database Server:     localhost:1433"
echo "  Database Name:       AfriPay"
echo "  Database User:       sa"
echo "  Database Password:   YourStrong@Passw0rd"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

echo "📝 Useful Commands:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  View logs:           docker compose logs -f"
echo "  View API logs:       docker compose logs -f afripay-api"
echo "  View DB logs:        docker compose logs -f sqlserver"
echo "  Stop services:       docker compose down"
echo "  Restart services:    docker compose restart"
echo "  Rebuild & restart:   docker compose up -d --build"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

print_info "Checking API health..."
sleep 3

# Try to check if API is responding
if curl -s http://localhost:8080/swagger >/dev/null 2>&1; then
    print_success "API is responding! You can now access it at http://localhost:8080"
else
    print_warning "API might still be starting up..."
    print_info "Give it a few more seconds and check: http://localhost:8080/swagger"
fi

echo ""
print_success "🎉 Setup complete! Happy coding!"
echo ""
