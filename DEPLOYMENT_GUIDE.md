# 🚀 Deployment Guide

**Version:** 9.3.2  
**Last Updated:** January 2025  
**Status:** Production Ready

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [Pre-Deployment Checklist](#pre-deployment-checklist)
3. [Environment Setup](#environment-setup)
4. [Build and Package](#build-and-package)
5. [Deployment Strategies](#deployment-strategies)
6. [Platform-Specific Deployment](#platform-specific-deployment)
7. [Configuration Management](#configuration-management)
8. [Database Migration](#database-migration)
9. [Monitoring and Logging](#monitoring-and-logging)
10. [Rollback Procedures](#rollback-procedures)
11. [Post-Deployment Verification](#post-deployment-verification)
12. [Troubleshooting](#troubleshooting)

---

## Overview

This guide covers deployment procedures for RaOS across different environments and platforms. Follow these procedures to ensure safe, reliable deployments.

### Deployment Goals

- 🎯 **Zero Downtime**: Minimize or eliminate service interruption
- 🔒 **Security**: Deploy securely without exposing sensitive data
- 🔄 **Repeatability**: Automated, consistent deployments
- 📊 **Observability**: Monitor deployment health and metrics
- ⏪ **Recoverability**: Quick rollback if issues occur

---

## Pre-Deployment Checklist

### Code Quality

- [ ] All tests pass locally
- [ ] Code review completed and approved
- [ ] No critical security vulnerabilities
- [ ] Performance testing completed
- [ ] Code coverage meets requirements (70%+)

### Documentation

- [ ] CHANGELOG.md updated
- [ ] Version numbers incremented
- [ ] README.md reflects changes
- [ ] API documentation updated
- [ ] Migration guide created (if breaking changes)

### Testing

- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] End-to-end tests pass
- [ ] Manual testing completed
- [ ] Load testing passed (for high-traffic changes)

### Configuration

- [ ] Environment variables configured
- [ ] Secrets stored securely
- [ ] Configuration files reviewed
- [ ] Database connection strings verified
- [ ] External service credentials validated

### Backup

- [ ] Database backup created
- [ ] Configuration backup created
- [ ] Previous version tagged
- [ ] Rollback plan documented
- [ ] Recovery procedures tested

---

## Environment Setup

### Development Environment

```bash
# Local development setup
export RACORE_ENVIRONMENT=Development
export RACORE_PORT=5000
export LOG_LEVEL=Debug

cd RaCore
dotnet run
```

### Staging Environment

```bash
# Staging environment (for testing)
export RACORE_ENVIRONMENT=Staging
export RACORE_PORT=80
export LOG_LEVEL=Information

# Use staging database
export DATABASE_CONNECTION="Server=staging-db;Database=raos_staging;"
```

### Production Environment

```bash
# Production environment
export RACORE_ENVIRONMENT=Production
export RACORE_PORT=80
export LOG_LEVEL=Warning

# Use production database
export DATABASE_CONNECTION="Server=prod-db;Database=raos_production;"
```

---

## Build and Package

### Development Build

```bash
# Standard development build
cd TheRaProject
dotnet build

# Or build specific projects
dotnet build RaCore/RaCore.csproj
dotnet build LegendaryCMS/LegendaryCMS.csproj
dotnet build LegendaryGameEngine/LegendaryGameEngine.csproj
```

### Release Build

```bash
# Build in Release configuration
dotnet build -c Release

# Verify build output
ls -la RaCore/bin/Release/net9.0/
```

### Self-Contained Deployment

```bash
# Create self-contained deployment (includes .NET runtime)
# Linux 64-bit
dotnet publish -c Release --self-contained true -r linux-x64 \
  -o ./publish/linux-x64

# Windows 64-bit
dotnet publish -c Release --self-contained true -r win-x64 \
  -o ./publish/win-x64

# macOS 64-bit (ARM)
dotnet publish -c Release --self-contained true -r osx-arm64 \
  -o ./publish/osx-arm64
```

### Framework-Dependent Deployment

```bash
# Smaller deployment (requires .NET runtime on target)
dotnet publish -c Release --self-contained false \
  -o ./publish/framework-dependent
```

### Using Build Scripts

```bash
# Linux production build (recommended)
chmod +x build-linux-production.sh
./build-linux-production.sh

# Standard Linux build
chmod +x build-linux.sh
./build-linux.sh

# Verify build
chmod +x verify-phase8.sh
./verify-phase8.sh
```

---

## Deployment Strategies

### 1. Blue-Green Deployment

**Best for**: Zero-downtime deployments

```bash
# Deploy to "green" environment while "blue" serves traffic
# 1. Deploy new version to green
ssh user@green-server "cd /opt/raos-green && ./deploy.sh"

# 2. Test green environment
curl http://green-server/api/health

# 3. Switch traffic to green
sudo systemctl stop raos-blue
sudo systemctl start raos-green
sudo nginx -s reload  # Update proxy config

# 4. Keep blue as backup for quick rollback
```

### 2. Rolling Deployment

**Best for**: Multi-server setups

```bash
# Update servers one at a time
for server in server1 server2 server3; do
    echo "Deploying to $server..."
    ssh user@$server "systemctl stop raos"
    scp -r publish/* user@$server:/opt/raos/
    ssh user@$server "systemctl start raos"
    
    # Wait and verify
    sleep 30
    curl http://$server/api/health || exit 1
done
```

### 3. Canary Deployment

**Best for**: High-risk changes

```bash
# Deploy to 10% of traffic first
# 1. Deploy to canary server
ssh user@canary-server "cd /opt/raos && ./deploy.sh"

# 2. Route 10% of traffic to canary
# (Configure in load balancer)

# 3. Monitor for issues
# If successful, gradually increase to 100%
# If issues, rollback immediately
```

---

## Platform-Specific Deployment

### Linux Deployment (Ubuntu 22.04 LTS)

See [LINUX_HOSTING_SETUP.md](LINUX_HOSTING_SETUP.md) for complete guide.

#### Quick Linux Deployment

```bash
# 1. Build production package
./build-linux-production.sh

# 2. Create deployment directory
sudo mkdir -p /opt/raos
sudo chown $USER:$USER /opt/raos

# 3. Copy files
cp -r publish/linux-x64/* /opt/raos/

# 4. Create systemd service
sudo nano /etc/systemd/system/raos.service
```

**raos.service** content:
```ini
[Unit]
Description=RaOS Mainframe
After=network.target

[Service]
Type=simple
User=raos
WorkingDirectory=/opt/raos
ExecStart=/opt/raos/RaCore
Restart=on-failure
RestartSec=10
Environment="RACORE_ENVIRONMENT=Production"
Environment="RACORE_PORT=80"

[Install]
WantedBy=multi-user.target
```

```bash
# 5. Enable and start service
sudo systemctl daemon-reload
sudo systemctl enable raos
sudo systemctl start raos

# 6. Verify
sudo systemctl status raos
curl http://localhost/api/health
```

#### Nginx Configuration

```bash
# Install Nginx
sudo apt-get install nginx

# Configure reverse proxy
sudo nano /etc/nginx/sites-available/raos
```

**Nginx config**:
```nginx
server {
    listen 80;
    server_name your-domain.com;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
# Enable site
sudo ln -s /etc/nginx/sites-available/raos /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### Windows Deployment

```powershell
# 1. Build for Windows
dotnet publish -c Release -r win-x64 --self-contained true

# 2. Copy to deployment directory
Copy-Item -Path "publish\win-x64\*" -Destination "C:\RaOS\" -Recurse

# 3. Install as Windows Service (using NSSM or similar)
nssm install RaOS "C:\RaOS\RaCore.exe"
nssm set RaOS AppDirectory "C:\RaOS"
nssm set RaOS AppEnvironmentExtra "RACORE_ENVIRONMENT=Production"

# 4. Start service
nssm start RaOS

# 5. Verify
Invoke-WebRequest -Uri "http://localhost/api/health"
```

### Docker Deployment

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["RaCore/RaCore.csproj", "RaCore/"]
RUN dotnet restore "RaCore/RaCore.csproj"
COPY . .
WORKDIR "/src/RaCore"
RUN dotnet build "RaCore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RaCore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaCore.dll"]
```

```bash
# Build and run with Docker
docker build -t raos:latest .
docker run -d -p 80:80 \
  -e RACORE_ENVIRONMENT=Production \
  --name raos \
  raos:latest

# Or use Docker Compose
docker-compose up -d
```

**docker-compose.yml**:
```yaml
version: '3.8'
services:
  raos:
    build: .
    ports:
      - "80:80"
    environment:
      - RACORE_ENVIRONMENT=Production
      - RACORE_PORT=80
    volumes:
      - ./data:/app/data
    restart: unless-stopped
```

---

## Configuration Management

### Environment Variables

```bash
# Development
export RACORE_ENVIRONMENT=Development
export RACORE_PORT=5000
export LOG_LEVEL=Debug

# Staging
export RACORE_ENVIRONMENT=Staging
export RACORE_PORT=80
export LOG_LEVEL=Information

# Production
export RACORE_ENVIRONMENT=Production
export RACORE_PORT=80
export LOG_LEVEL=Warning
export DATABASE_CONNECTION="encrypted_connection_string"
```

### Configuration Files

```json
// appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "System": "Error"
    }
  },
  "AllowedHosts": "*",
  "RaCore": {
    "Port": 80,
    "Environment": "Production"
  },
  "Database": {
    "ConnectionString": "Server=prod-db;Database=raos;"
  }
}
```

### Secrets Management

```bash
# Use dotnet user-secrets for development
dotnet user-secrets init
dotnet user-secrets set "Database:Password" "your-password"

# Use environment variables in production
export DATABASE_PASSWORD="your-secure-password"

# Or use Azure Key Vault / AWS Secrets Manager
# Configure in appsettings.json
```

---

## Database Migration

### Before Deployment

```bash
# 1. Backup database
pg_dump raos_production > backup_$(date +%Y%m%d_%H%M%S).sql

# Or for SQLite
sqlite3 raos.db ".backup 'backup_$(date +%Y%m%d_%H%M%S).db'"

# 2. Test migration on staging
dotnet ef database update --connection "Server=staging-db"

# 3. Verify staging data integrity
```

### During Deployment

```bash
# Apply migrations
cd RaCore
dotnet ef database update

# Or use custom migration script
./scripts/migrate-database.sh
```

### Rollback Migration

```bash
# Rollback to previous migration
dotnet ef database update PreviousMigrationName

# Or restore from backup
psql raos_production < backup_20250107_120000.sql
```

---

## Monitoring and Logging

### Application Monitoring

```bash
# Check application status
systemctl status raos

# View logs
journalctl -u raos -f

# Or check log files
tail -f /var/log/raos/application.log
```

### Health Checks

```bash
# Health check endpoint
curl http://localhost/api/health

# Expected response
{
  "status": "Healthy",
  "timestamp": "2025-01-07T12:00:00Z",
  "version": "9.3.2"
}
```

### Performance Monitoring

```bash
# CPU and memory usage
top -p $(pgrep RaCore)

# Or use htop
htop -p $(pgrep RaCore)

# Network monitoring
netstat -tuln | grep :80
```

### Log Aggregation

Configure centralized logging:
- **Serilog** for structured logging
- **ELK Stack** (Elasticsearch, Logstash, Kibana)
- **Grafana** + **Loki** for visualization
- **Application Insights** (Azure)
- **CloudWatch** (AWS)

---

## Rollback Procedures

### Quick Rollback Steps

```bash
# 1. Stop current version
sudo systemctl stop raos

# 2. Restore previous version
sudo rm -rf /opt/raos/*
sudo cp -r /opt/raos-backup-v9.3.1/* /opt/raos/

# 3. Rollback database (if needed)
psql raos_production < backup_previous.sql

# 4. Restart service
sudo systemctl start raos

# 5. Verify
curl http://localhost/api/health
```

### Blue-Green Rollback

```bash
# Simply switch back to blue environment
sudo systemctl stop raos-green
sudo systemctl start raos-blue
sudo nginx -s reload
```

### Git-Based Rollback

```bash
# Revert to previous commit
git revert HEAD
# Or
git checkout tags/v9.3.1

# Rebuild and redeploy
./build-linux-production.sh
./deploy.sh
```

---

## Post-Deployment Verification

### Smoke Tests

```bash
# 1. Health check
curl http://localhost/api/health

# 2. Module status
curl http://localhost/api/status

# 3. CMS API
curl http://localhost:8080/api/cms/health

# 4. Game Engine API
curl http://localhost/api/engine/stats
```

### Functional Tests

```bash
# Run automated tests against deployed environment
export TEST_BASE_URL=http://production-server
dotnet test Tests/E2E/

# Or run manual test script
./scripts/post-deployment-tests.sh
```

### Checklist

- [ ] Application starts successfully
- [ ] Health endpoints return 200
- [ ] Database connections work
- [ ] Logs show no errors
- [ ] All modules initialized
- [ ] API endpoints respond correctly
- [ ] WebSocket connections work
- [ ] Authentication functions
- [ ] Performance is acceptable
- [ ] No memory leaks detected

---

## Troubleshooting

### Common Issues

#### Service Won't Start

```bash
# Check logs
journalctl -u raos -n 50

# Check file permissions
ls -la /opt/raos/

# Check port availability
netstat -tuln | grep :80

# Test manual start
cd /opt/raos
./RaCore
```

#### Database Connection Errors

```bash
# Verify connection string
echo $DATABASE_CONNECTION

# Test database connectivity
psql -h db-host -U db-user -d raos_production

# Check database logs
tail -f /var/log/postgresql/postgresql.log
```

#### Performance Issues

```bash
# Check resource usage
top -p $(pgrep RaCore)

# Check for memory leaks
dotnet-counters monitor -p $(pgrep RaCore)

# Analyze slow queries
# (Check database query logs)
```

#### Module Loading Failures

```bash
# Check module files exist
ls -la /opt/raos/*.dll

# Verify module compatibility
dotnet --info

# Check module dependencies
ldd /opt/raos/LegendaryCMS.dll
```

---

## Automation Scripts

### Deployment Script Example

```bash
#!/bin/bash
# deploy.sh - Automated deployment script

set -e

DEPLOY_DIR="/opt/raos"
BACKUP_DIR="/opt/raos-backup-$(date +%Y%m%d-%H%M%S)"
LOG_FILE="/var/log/raos-deploy.log"

echo "Starting deployment..." | tee -a $LOG_FILE

# 1. Backup current version
echo "Creating backup..." | tee -a $LOG_FILE
sudo cp -r $DEPLOY_DIR $BACKUP_DIR

# 2. Stop service
echo "Stopping service..." | tee -a $LOG_FILE
sudo systemctl stop raos

# 3. Deploy new version
echo "Deploying new version..." | tee -a $LOG_FILE
sudo cp -r publish/linux-x64/* $DEPLOY_DIR/

# 4. Update database
echo "Updating database..." | tee -a $LOG_FILE
cd $DEPLOY_DIR
dotnet ef database update

# 5. Start service
echo "Starting service..." | tee -a $LOG_FILE
sudo systemctl start raos

# 6. Verify
echo "Verifying deployment..." | tee -a $LOG_FILE
sleep 10
if curl -f http://localhost/api/health > /dev/null 2>&1; then
    echo "Deployment successful!" | tee -a $LOG_FILE
else
    echo "Deployment failed! Rolling back..." | tee -a $LOG_FILE
    sudo systemctl stop raos
    sudo cp -r $BACKUP_DIR/* $DEPLOY_DIR/
    sudo systemctl start raos
    exit 1
fi

echo "Deployment complete." | tee -a $LOG_FILE
```

---

## Security Considerations

### Deployment Security Checklist

- [ ] Use HTTPS in production
- [ ] Secure database connections (SSL/TLS)
- [ ] Store secrets securely (not in code)
- [ ] Limit file permissions (0600 for configs)
- [ ] Run service as non-root user
- [ ] Enable firewall rules
- [ ] Regular security updates
- [ ] Audit logs enabled
- [ ] Rate limiting configured
- [ ] CORS properly configured

---

## Resources

- [LINUX_HOSTING_SETUP.md](LINUX_HOSTING_SETUP.md) - Linux deployment guide
- [WINDOWS_VS_LINUX.md](WINDOWS_VS_LINUX.md) - Platform comparison
- [ARCHITECTURE.md](ARCHITECTURE.md) - System architecture
- [SECURITY_ARCHITECTURE.md](SECURITY_ARCHITECTURE.md) - Security details

---

**Last Updated:** January 2025  
**Version:** 9.3.2  
**Maintained By:** RaOS Development Team

---

**Copyright © 2025 AGP Studios, INC. All rights reserved.**
