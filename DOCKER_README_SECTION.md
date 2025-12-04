# Docker Quick Start Section

Add this section to your main README.md file:

---

## 🐳 Docker Quick Start

The easiest way to run AfriPay locally is using Docker. No .NET SDK or SQL Server installation required!

### Prerequisites
- [Docker Desktop](https://docs.docker.com/get-docker/) installed and running

### One-Command Setup

**macOS/Linux:**
```bash
./setup.sh
```

**Windows:**
```powershell
.\setup.ps1
```

That's it! The script will:
- ✅ Build the Docker images
- ✅ Start SQL Server database
- ✅ Start the API
- ✅ Apply database migrations automatically
- ✅ Open Swagger UI in your browser

### Access the API

Once setup completes:
- **Swagger UI:** http://localhost:8080/swagger
- **API Base URL:** http://localhost:8080

### Quick Commands

```bash
# Start services
docker compose up -d

# View logs
docker compose logs -f

# Stop services
docker compose down

# Rebuild after code changes
docker compose up -d --build
```

### 📖 Documentation

- **[SETUP_GUIDE.md](SETUP_GUIDE.md)** - Detailed guide for non-developers
- **[DOCKER_SETUP_SUMMARY.md](DOCKER_SETUP_SUMMARY.md)** - Technical documentation
- **Need help?** Check the [Troubleshooting section](SETUP_GUIDE.md#troubleshooting)

---
