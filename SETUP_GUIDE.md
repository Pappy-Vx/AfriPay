# AfriPay API - Quick Setup Guide

Welcome! This guide will help you get the AfriPay API up and running on your machine in just a few minutes, even if you're not a backend developer.

## 📋 Table of Contents

- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Accessing the API](#accessing-the-api)
- [Testing the API](#testing-the-api)
- [Troubleshooting](#troubleshooting)
- [Useful Commands](#useful-commands)
- [Stopping the Application](#stopping-the-application)

---

## 🎯 Prerequisites

Before you begin, you need to install **Docker Desktop**. That's it! The setup script will handle everything else.

### Installing Docker Desktop

#### **Windows:**
1. Download Docker Desktop from [https://docs.docker.com/desktop/install/windows/](https://docs.docker.com/desktop/install/windows/)
2. Run the installer
3. Restart your computer if prompted
4. Open Docker Desktop and wait for it to start (you'll see a green "Running" status)

#### **macOS:**
1. Download Docker Desktop from [https://docs.docker.com/desktop/install/mac-install/](https://docs.docker.com/desktop/install/mac-install/)
2. Drag Docker to your Applications folder
3. Open Docker from Applications
4. Wait for Docker to start (you'll see a green indicator in the menu bar)

#### **Linux:**
1. Follow the installation guide for your distribution: [https://docs.docker.com/desktop/install/linux-install/](https://docs.docker.com/desktop/install/linux-install/)
2. Start Docker Desktop

### Verifying Docker Installation

Open a terminal (Command Prompt/PowerShell on Windows, Terminal on macOS/Linux) and run:

```bash
docker --version
```

You should see something like: `Docker version 24.x.x`

---

## 🚀 Quick Start

Once Docker is installed and running, follow these simple steps:

### **For Windows Users:**

1. **Open PowerShell**
   - Right-click on the Start menu
   - Select "Windows PowerShell" or "Terminal"

2. **Navigate to the project folder**
   ```powershell
   cd path\to\AfriPay
   ```

3. **Run the setup script**
   ```powershell
   .\setup.ps1
   ```

4. **Follow the prompts**
   - The script will guide you through each step
   - Press Enter when prompted to continue
   - Wait for the setup to complete (usually 2-5 minutes)

### **For macOS/Linux Users:**

1. **Open Terminal**
   - Press `Cmd + Space`, type "Terminal", and press Enter (macOS)
   - Or find Terminal in your applications (Linux)

2. **Navigate to the project folder**
   ```bash
   cd path/to/AfriPay
   ```

3. **Run the setup script**
   ```bash
   ./setup.sh
   ```

4. **Follow the prompts**
   - The script will guide you through each step
   - Press Enter when prompted to continue
   - Wait for the setup to complete (usually 2-5 minutes)

### What Happens During Setup?

The setup script will:
- ✅ Check that Docker is installed and running
- ✅ Build the application (first time may take a few minutes)
- ✅ Start the database
- ✅ Start the API
- ✅ Wait for automatic database migrations to complete
- ✅ Verify everything is working

You'll see colorful messages showing progress at each step!

**Note:** The API automatically applies database migrations on startup, so you don't need to run any manual database setup commands.

---

## 🌐 Accessing the API

Once setup is complete, you can access the API at:

### **Swagger UI (Interactive API Documentation)**
Open your web browser and go to:
```
http://localhost:8080/swagger
```

This is an interactive interface where you can:
- See all available API endpoints
- Read documentation for each endpoint
- Test API calls directly from your browser
- See example requests and responses

### **API Base URL**
If you're testing with other tools (like Postman), use:
```
http://localhost:8080
```

---

## 🧪 Testing the API

### Using Swagger UI (Easiest Method)

1. **Open Swagger** at [http://localhost:8080/swagger](http://localhost:8080/swagger)

2. **Expand an endpoint** by clicking on it
   - For example, click on `GET /api/v1/users` or similar

3. **Click "Try it out"**

4. **Fill in any required parameters** (if needed)

5. **Click "Execute"**

6. **View the response** below
   - You'll see the status code (200 = success)
   - Response body with data
   - Response headers

### Example: Testing Authentication

Most APIs require authentication. Here's a typical flow:

1. **Register/Login**
   - Find the authentication endpoint (usually `/api/auth/login` or `/api/auth/register`)
   - Click "Try it out"
   - Fill in the required credentials
   - Click "Execute"
   - Copy the token from the response

2. **Authorize Future Requests**
   - Click the "Authorize" button at the top of Swagger
   - Paste your token in the format: `Bearer YOUR_TOKEN_HERE`
   - Click "Authorize"
   - Now all your requests will include authentication

3. **Test Protected Endpoints**
   - Try any endpoint that requires authentication
   - It should now work!

---

## 🔧 Troubleshooting

### Problem: "Docker is not running"

**Solution:**
- Open Docker Desktop application
- Wait for it to fully start (green indicator)
- Try running the setup script again

### Problem: "Port 8080 is already in use"

**Solution:**
Stop any other applications using port 8080, or modify the port in `docker-compose.yml`:
```yaml
ports:
  - "8081:8080"  # Change 8080 to 8081 or another free port
```

### Problem: "Cannot connect to database"

**Solution:**
1. Check if SQL Server container is running:
   ```bash
   docker ps
   ```
   You should see `afripay-sqlserver` in the list

2. Restart the database:
   ```bash
   docker compose restart sqlserver
   ```

3. Wait 20-30 seconds for it to fully start

### Problem: Setup script fails

**Solution:**
1. Check Docker is running
2. Clean up and try again:
   ```bash
   # Stop everything
   docker compose down

   # Remove old images
   docker compose down --rmi all

   # Run setup again
   ./setup.sh  # or .\setup.ps1 on Windows
   ```

### Problem: "API returns 500 errors"

**Solution:**
Check the logs to see what's wrong:
```bash
docker compose logs -f afripay-api
```

Look for error messages and contact the development team with the log details.

### Problem: Database migrations not applied

**Solution:**
The API automatically runs migrations on startup. If you see migration errors:
1. Check the API logs for migration errors:
   ```bash
   docker compose logs afripay-api | grep -i migration
   ```
2. Restart the API to retry migrations:
   ```bash
   docker compose restart afripay-api
   ```
3. If issues persist, check that the database is healthy:
   ```bash
   docker compose ps
   ```

---

## 📝 Useful Commands

### View Logs

**See all logs:**
```bash
docker compose logs -f
```

**See only API logs:**
```bash
docker compose logs -f afripay-api
```

**See only database logs:**
```bash
docker compose logs -f sqlserver
```

Press `Ctrl+C` to stop viewing logs.

### Check Status

**See what's running:**
```bash
docker compose ps
```

### Restart Services

**Restart everything:**
```bash
docker compose restart
```

**Restart only the API:**
```bash
docker compose restart afripay-api
```

### Rebuild After Code Changes

If developers make changes to the code:
```bash
docker compose up -d --build
```

---

## 🛑 Stopping the Application

### Stop All Services

**Keep data (recommended):**
```bash
docker compose down
```

This stops the containers but keeps your database data.

**Remove everything including data:**
```bash
docker compose down -v
```

⚠️ **Warning:** This deletes all database data! Only use if you want a fresh start.

### Restart Later

To start the application again after stopping:
```bash
docker compose up -d
```

This is much faster than running the setup script again.

---

## 🗄️ Database Access

If you need to connect to the database directly (for viewing data, running queries, etc.):

### Connection Details:
- **Server:** `localhost,1433` or `localhost:1433`
- **Database:** `AfriPay`
- **Username:** `sa`
- **Password:** `YourStrong@Passw0rd`
- **Authentication:** SQL Server Authentication

### Recommended Tools:
- **Azure Data Studio** (Free, cross-platform): [Download](https://docs.microsoft.com/en-us/sql/azure-data-studio/download)
- **SQL Server Management Studio** (Windows only): [Download](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- **DBeaver** (Free, cross-platform): [Download](https://dbeaver.io/)

---

## 🆘 Getting Help

If you encounter issues not covered in this guide:

1. **Check the logs** (see [Useful Commands](#useful-commands))
2. **Try restarting** Docker Desktop
3. **Contact the development team** with:
   - What you were trying to do
   - The error message you received
   - Relevant logs from `docker compose logs`

---

## 📊 System Requirements

- **RAM:** At least 4GB available (8GB recommended)
- **Disk Space:** At least 5GB free
- **Operating System:**
  - Windows 10/11 (64-bit)
  - macOS 10.15 or newer
  - Modern Linux distribution

---

## ✅ Verification Checklist

After running the setup script, verify everything works:

- [ ] Docker Desktop is running
- [ ] Setup script completed without errors
- [ ] Can access [http://localhost:8080/swagger](http://localhost:8080/swagger)
- [ ] Swagger UI loads and shows API endpoints
- [ ] Can execute a test API call from Swagger
- [ ] API returns a valid response

If all boxes are checked, you're ready to test the API! 🎉

---

## 📖 Additional Resources

- **Docker Documentation:** [https://docs.docker.com/](https://docs.docker.com/)
- **What is Swagger/OpenAPI?** [https://swagger.io/docs/](https://swagger.io/docs/)
- **HTTP Status Codes:** [https://httpstatuses.com/](https://httpstatuses.com/)

---

**Happy Testing!** 🚀

If you have suggestions for improving this guide, please share them with the development team.
