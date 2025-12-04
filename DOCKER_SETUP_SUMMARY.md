# Docker Setup Summary

## 📦 Files Created

This document summarizes all the Docker-related files created for the AfriPay project.

---

## 1. Dockerfile (Root Level)

**Location:** `/Dockerfile`

**Purpose:** Builds the AfriPay API Docker image with proper multi-project support.

**Key Features:**
- Multi-stage build (build, publish, runtime)
- Properly handles all 4 projects (API, APP, CORE, DAL)
- Optimized layer caching
- Uses .NET 8.0 official images
- Runs as non-root user for security

**Build Command:**
```bash
docker build -t afripay-api:latest .
```

---

## 2. docker-compose.yml (Root Level)

**Location:** `/docker-compose.yml`

**Purpose:** Orchestrates the entire application stack (API + Database).

**Services:**

### SQL Server (`sqlserver`)
- **Image:** `mcr.microsoft.com/mssql/server:2022-latest`
- **Port:** 1433
- **Credentials:** sa / YourStrong@Passw0rd
- **Volume:** Persistent storage for database data
- **Health Check:** Ensures database is ready before API starts

### AfriPay API (`afripay-api`)
- **Build:** From Dockerfile in project root
- **Port:** 8080
- **Environment:** Development mode
- **Connection String:** Pre-configured to connect to SQL Server container
- **Dependency:** Waits for SQL Server to be healthy

**Usage:**
```bash
# Start all services
docker compose up -d

# View logs
docker compose logs -f

# Stop services
docker compose down
```

---

## 3. setup.sh (Root Level)

**Location:** `/setup.sh`

**Purpose:** Automated setup script for macOS/Linux users.

**Features:**
- ✅ Colorful, interactive output
- ✅ Checks all prerequisites (Docker, Docker Compose)
- ✅ Builds images with progress feedback
- ✅ Starts services in correct order
- ✅ Waits for health checks
- ✅ Displays access information
- ✅ Tests API availability
- ✅ Error handling with helpful messages

**Usage:**
```bash
chmod +x setup.sh
./setup.sh
```

**What It Does:**
1. Verifies Docker is installed and running
2. Cleans up old containers
3. Builds Docker images
4. Starts SQL Server and waits for it to be ready
5. Starts AfriPay API
6. Waits for automatic migrations
7. Shows status and access URLs

---

## 4. setup.ps1 (Root Level)

**Location:** `/setup.ps1`

**Purpose:** Automated setup script for Windows PowerShell users.

**Features:**
- ✅ Same functionality as setup.sh
- ✅ Windows-optimized commands
- ✅ Colorful PowerShell output
- ✅ Automatically opens Swagger in browser
- ✅ Handles both docker-compose and docker compose syntax

**Usage:**
```powershell
.\setup.ps1
```

Or right-click → "Run with PowerShell"

---

## 5. SETUP_GUIDE.md (Root Level)

**Location:** `/SETUP_GUIDE.md`

**Purpose:** Comprehensive user guide for non-developers.

**Contents:**
- Step-by-step Docker installation instructions
- How to run the setup scripts
- How to access and test the API
- Swagger UI usage guide
- Troubleshooting common issues
- Useful Docker commands
- Database connection information
- System requirements

**Target Audience:** Frontend developers, QA testers, product managers, designers, etc.

---

## 🚀 Quick Start Guide

For developers familiar with Docker:

```bash
# 1. Ensure Docker is running
docker --version

# 2. Run the setup script
./setup.sh          # macOS/Linux
.\setup.ps1         # Windows

# 3. Access the API
# Open: http://localhost:8080/swagger
```

---

## 🔄 Automatic Database Migrations

The AfriPay API automatically applies Entity Framework migrations on startup via `Database.MigrateAsync()` in [Program.cs:273](AfriPay.API/Program.cs#L273).

**This means:**
- ✅ No manual migration commands needed
- ✅ Database schema updates automatically
- ✅ Works seamlessly in Docker containers
- ✅ Migrations run before API accepts requests

**If migrations fail:**
1. Check API logs: `docker compose logs afripay-api`
2. Restart API: `docker compose restart afripay-api`
3. Verify database is healthy: `docker compose ps`

---

## 📊 Service URLs

Once running, access these services:

| Service | URL | Description |
|---------|-----|-------------|
| **API** | http://localhost:8080 | Base API URL |
| **Swagger UI** | http://localhost:8080/swagger | Interactive API documentation |
| **Database** | localhost:1433 | SQL Server (via SSMS, Azure Data Studio, etc.) |

---

## 🗄️ Database Connection

Use these credentials to connect to the database:

```
Server: localhost,1433
Database: AfriPay
User: sa
Password: YourStrong@Passw0rd
Authentication: SQL Server Authentication
```

**⚠️ Note:** Change the password in production! Update it in `docker-compose.yml`.

---

## 🛠️ Useful Commands

### View Logs
```bash
docker compose logs -f                # All services
docker compose logs -f afripay-api    # API only
docker compose logs -f sqlserver      # Database only
```

### Check Status
```bash
docker compose ps                     # Service status
docker ps                             # All containers
```

### Restart Services
```bash
docker compose restart                # Restart all
docker compose restart afripay-api    # Restart API only
```

### Rebuild After Code Changes
```bash
docker compose up -d --build
```

### Stop Everything
```bash
docker compose down                   # Stop (keep data)
docker compose down -v                # Stop and delete data
```

### Clean Slate
```bash
docker compose down -v
docker system prune -a
./setup.sh  # or .\setup.ps1
```

---

## 🔧 Customization

### Change API Port

Edit `docker-compose.yml`:
```yaml
afripay-api:
  ports:
    - "8081:8080"  # Change 8080 to 8081
```

### Change Database Password

Edit `docker-compose.yml` in **two places**:
```yaml
sqlserver:
  environment:
    SA_PASSWORD: "YourNewPassword"  # Update here

afripay-api:
  environment:
    - ConnectionStrings__DefaultConnection=Data Source=sqlserver,1433;Initial Catalog=AfriPay;User ID=sa;Password=YourNewPassword;...  # And here
```

### Add Environment Variables

Edit `docker-compose.yml` under `afripay-api` → `environment`:
```yaml
afripay-api:
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - YourCustomVariable=YourValue
```

---

## 🐛 Troubleshooting

### Docker Not Running
**Symptom:** "Cannot connect to Docker daemon"
**Solution:** Open Docker Desktop and wait for it to start

### Port Already in Use
**Symptom:** "Port 8080 is already allocated"
**Solution:** Change the port in docker-compose.yml or stop the conflicting service

### Database Connection Failed
**Symptom:** "Cannot connect to SQL Server"
**Solution:**
1. Check database health: `docker compose ps`
2. View database logs: `docker compose logs sqlserver`
3. Restart database: `docker compose restart sqlserver`

### API Returns 500 Errors
**Symptom:** API calls fail with 500 status
**Solution:**
1. Check API logs: `docker compose logs afripay-api`
2. Look for migration errors
3. Restart API: `docker compose restart afripay-api`

---

## 📝 Development Workflow

### For Backend Developers

1. **Make code changes** in your IDE
2. **Rebuild and restart:**
   ```bash
   docker compose up -d --build
   ```
3. **View logs to debug:**
   ```bash
   docker compose logs -f afripay-api
   ```

### For Frontend/Mobile Developers

1. **Run setup once:**
   ```bash
   ./setup.sh  # or .\setup.ps1
   ```
2. **Access API at:** http://localhost:8080
3. **View API docs at:** http://localhost:8080/swagger
4. **Test endpoints** using Swagger UI or your app

### For QA/Testers

1. **Start the environment:**
   ```bash
   docker compose up -d
   ```
2. **Test via Swagger UI:** http://localhost:8080/swagger
3. **View logs if issues occur:**
   ```bash
   docker compose logs -f
   ```
4. **Stop when done:**
   ```bash
   docker compose down
   ```

---

## 🎯 Next Steps

1. **For non-developers:** Read [SETUP_GUIDE.md](SETUP_GUIDE.md)
2. **For developers:** Run `./setup.sh` or `.\setup.ps1`
3. **Access Swagger:** http://localhost:8080/swagger
4. **Start testing!**

---

## 📚 Additional Resources

- **Docker Documentation:** https://docs.docker.com/
- **Docker Compose Documentation:** https://docs.docker.com/compose/
- **ASP.NET Core in Docker:** https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/
- **SQL Server in Docker:** https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-deployment

---

## ✅ Success Checklist

After running the setup, verify:

- [ ] Docker Desktop is running
- [ ] `docker compose ps` shows both services as "Up" and healthy
- [ ] http://localhost:8080/swagger loads successfully
- [ ] Can execute a test API call from Swagger UI
- [ ] Database connection works (test via API or direct connection)

If all boxes are checked, your environment is ready! 🎉

---

**Last Updated:** 2025-12-04
**Maintained By:** AfriPay Development Team
