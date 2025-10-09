# ASHAT Server Setup Helper Guide

**Version:** 1.0  
**Module:** AshatServerSetupHelper Extension  
**Purpose:** Interactive guidance for RaOS public server setup with FTP

## Overview

The ASHAT Server Setup Helper provides interactive guidance for server administrators setting up RaOS public servers with FTP access. It offers:

- ✅ **Interactive Step-by-Step Guidance** - Walk through server setup with ASHAT's help
- ✅ **Training Course Library** - Access reference materials anytime
- ✅ **Troubleshooting Assistance** - Get help with common setup issues
- ✅ **Server Health Monitoring** - Check readiness before FTP setup
- ✅ **Security Best Practices** - Learn how to secure FTP access
- ✅ **Guided Launch Workflow** - Complete public server launch assistance

---

## Quick Start

### Basic Command Structure

All ASHAT Server Setup Helper commands start with:
```
ashat setup <command>
```

### Get Help

```bash
ashat setup help
```

### Launch Public Server (Guided)

For a complete guided workflow:
```bash
ashat setup launch
```

---

## Commands Reference

### Interactive Guidance

#### Get Server Setup Guide
```
ashat setup guide
```

Provides a comprehensive overview of the entire server setup process, including:
- Prerequisites checklist
- Step-by-step instructions
- Command examples
- Pro tips

#### Check Server Health
```
ashat setup health
```

Checks if your server is operational and provides ASHAT recommendations for any issues found.

#### Launch Public Server with FTP
```
ashat setup launch
```

Initiates a guided workflow that:
1. Checks server health
2. Verifies FTP server status
3. Provides step-by-step instructions for:
   - Admin instance creation
   - FTP setup
   - Security configuration
   - Connection verification

#### Get Setup Checklist
```
ashat setup checklist
```

Displays a comprehensive checklist of all tasks needed for server setup. Use this to track your progress.

---

### FTP Setup Guidance

ASHAT provides detailed guidance for each phase of FTP setup:

#### Step 1: Check Server Status
```
ashat setup ftp check
```

Learn how to verify:
- Server health
- FTP server installation status
- System readiness for FTP

#### Step 2: Install FTP Server
```
ashat setup ftp install
```

Get instructions for:
- Installing vsftpd
- Starting the service
- Configuring firewall rules

#### Step 3: Configure FTP Access
```
ashat setup ftp configure
```

Learn how to:
- Create admin instances
- Setup FTP directories
- Configure FTP access
- Get connection information

#### Step 4: Secure FTP Access
```
ashat setup ftp secure
```

Advanced security guidance:
- Creating restricted FTP users
- Implementing chroot jails
- Enabling SSL/TLS
- Monitoring and logging

---

### Training Courses

ASHAT maintains a library of training courses you can revisit anytime.

#### List Available Courses
```
ashat setup courses
```

Shows all available training courses with descriptions.

#### View Course Content
```
ashat setup course <course-id>
```

Available Course IDs:
- `ftp-basics` - FTP Server Setup Basics
- `ftp-security` - Secure FTP Configuration
- `server-health` - Server Health Monitoring
- `public-server-launch` - Public Server Launch Guide

**Example:**
```bash
ashat setup course ftp-security
```

---

### Troubleshooting

Get help with specific issues:

```
ashat setup troubleshoot <issue>
```

**Examples:**
```bash
ashat setup troubleshoot ftp
ashat setup troubleshoot connection
ashat setup troubleshoot server
```

---

## Workflow Examples

### Example 1: First-Time Server Setup

```bash
# Step 1: Get the comprehensive guide
ashat setup guide

# Step 2: Check server health
ashat setup health

# Step 3: Get FTP installation help
ashat setup ftp install

# Step 4: Use the guided launch workflow
ashat setup launch

# Step 5: Review security best practices
ashat setup course ftp-security
```

### Example 2: Troubleshooting FTP Issues

```bash
# Check server health with recommendations
ashat setup health

# Get specific FTP troubleshooting help
ashat setup troubleshoot ftp

# Review FTP basics course
ashat setup course ftp-basics

# Get step-by-step FTP configuration help
ashat setup ftp configure
```

### Example 3: Learning and Reference

```bash
# List all available training courses
ashat setup courses

# Review specific courses
ashat setup course ftp-basics
ashat setup course ftp-security
ashat setup course server-health
ashat setup course public-server-launch

# Get practical guidance
ashat setup guide
ashat setup checklist
```

---

## Integration with ServerSetup Module

ASHAT Server Setup Helper works seamlessly with the ServerSetup module:

### ASHAT Guidance → ServerSetup Commands

| ASHAT Guidance | ServerSetup Command |
|---------------|---------------------|
| `ashat setup health` | `serversetup health` |
| `ashat setup ftp check` | `serversetup ftp status` |
| `ashat setup ftp configure` | `serversetup ftp setup` |
| `ashat setup ftp secure` | `serversetup ftp createuser` |

ASHAT provides the **guidance and explanation**, while ServerSetup executes the **actual configuration**.

---

## Features

### Interactive Guidance

- **Context-Aware Help**: ASHAT provides recommendations based on your server's current state
- **Step-by-Step Instructions**: Clear, actionable steps for each task
- **Command Examples**: Practical examples you can copy and paste
- **Pro Tips**: Best practices and recommendations

### Training Course Library

- **Revisit Anytime**: Access training materials whenever you need a refresher
- **Structured Learning**: Lessons organized by topic
- **Practical Focus**: Related commands provided with each course
- **Comprehensive Coverage**: From basics to advanced security

### Server Health Monitoring

- **Operational Readiness**: Verify server is ready before FTP setup
- **Issue Detection**: Identify and diagnose problems
- **Smart Recommendations**: Get specific advice for each issue
- **Proactive Prevention**: Catch problems before they cause failures

### Security Best Practices

- **Restricted FTP Users**: Learn to create secure, isolated FTP access
- **Chroot Jails**: Understand and implement directory restrictions
- **SSL/TLS Configuration**: Enable encrypted FTP connections
- **Access Monitoring**: Learn to track and audit FTP activity

---

## Training Course Details

### FTP Basics Course

**Course ID:** `ftp-basics`

**Lessons:**
1. Understanding FTP and vsftpd
2. Installing vsftpd on Linux
3. Basic vsftpd configuration
4. Testing FTP connectivity

**Related Commands:**
- `ashat setup ftp install`
- `serversetup ftp status`
- `serversetup ftp setup`

---

### FTP Security Course

**Course ID:** `ftp-security`

**Lessons:**
1. Creating restricted FTP users
2. Implementing chroot jails
3. Setting up SSL/TLS for FTP
4. Monitoring FTP access logs

**Related Commands:**
- `ashat setup ftp secure`
- `serversetup ftp createuser`

---

### Server Health Course

**Course ID:** `server-health`

**Lessons:**
1. Server health check basics
2. Troubleshooting common issues
3. Monitoring essential folders
4. Ensuring operational readiness

**Related Commands:**
- `ashat setup health`
- `serversetup health`

---

### Public Server Launch Course

**Course ID:** `public-server-launch`

**Lessons:**
1. Pre-launch server health check
2. FTP setup and configuration
3. Admin instance creation
4. Security best practices
5. Post-launch verification

**Related Commands:**
- `ashat setup launch`
- `ashat setup guide`

---

## Best Practices

### Before Starting Setup

1. ✅ Review the comprehensive guide: `ashat setup guide`
2. ✅ Check server health: `ashat setup health`
3. ✅ Review relevant training courses
4. ✅ Ensure you have root/sudo access
5. ✅ Have your license and username information ready

### During Setup

1. ✅ Follow steps in order
2. ✅ Verify each step completes successfully
3. ✅ Use the checklist to track progress: `ashat setup checklist`
4. ✅ Implement security recommendations
5. ✅ Test FTP connection before moving on

### After Setup

1. ✅ Verify all services are running
2. ✅ Test FTP access with a client
3. ✅ Review security settings
4. ✅ Monitor logs for any issues
5. ✅ Keep training materials available for reference

---

## Troubleshooting Guide

### Server Health Issues

**Problem:** Server health check shows issues

**Solution:**
```bash
# Get detailed recommendations
ashat setup health

# Review server health course
ashat setup course server-health

# Get troubleshooting help
ashat setup troubleshoot server
```

---

### FTP Connection Issues

**Problem:** Cannot connect to FTP server

**Solution:**
```bash
# Get FTP troubleshooting help
ashat setup troubleshoot ftp

# Review connection guide
ashat setup ftp check

# Review FTP basics
ashat setup course ftp-basics
```

---

### Security Configuration

**Problem:** Need to secure FTP access

**Solution:**
```bash
# Get security guidance
ashat setup ftp secure

# Review security course
ashat setup course ftp-security

# Create restricted FTP user
serversetup ftp createuser username=raos_ftp path=/path/to/raos
```

---

## FAQ

### Q: How is ASHAT Server Setup Helper different from ServerSetup module?

**A:** ASHAT provides **guidance, training, and explanations**, while ServerSetup performs the **actual configuration**. Think of ASHAT as your interactive instructor and ServerSetup as the tools you use.

### Q: Can I revisit training courses after completing them?

**A:** Yes! All training courses are available anytime using `ashat setup course <course-id>`. This is one of ASHAT's key features - you can always go back and review materials.

### Q: What if I encounter an issue not covered in the documentation?

**A:** Use `ashat setup troubleshoot <issue>` to get help with common problems. For specific issues, the troubleshooting command provides targeted guidance.

### Q: Do I need to follow the guided workflow, or can I use individual commands?

**A:** You can use either approach! The guided workflow (`ashat setup launch`) is great for first-time setup, while individual commands let you focus on specific tasks.

### Q: How does ASHAT know my server's status?

**A:** ASHAT integrates with the ServerSetup module to check real-time server status, including health checks and FTP server status.

---

## Related Documentation

- [FTP_MANAGEMENT.md](FTP_MANAGEMENT.md) - Detailed FTP setup documentation
- [LINUX_HOSTING_SETUP.md](LINUX_HOSTING_SETUP.md) - Complete Linux server setup guide
- [ASHAT_DEPLOYMENT_WORKFLOW.md](ASHAT_DEPLOYMENT_WORKFLOW.md) - ASHAT deployment workflow

---

## Support

For additional help:

1. **Interactive Guidance**: `ashat setup help`
2. **Training Courses**: `ashat setup courses`
3. **Troubleshooting**: `ashat setup troubleshoot <issue>`
4. **Documentation**: Review markdown files in repository
5. **GitHub Issues**: Report issues on GitHub

---

**Remember:** ASHAT is here to guide you through server setup. Use the training courses and interactive guidance to learn at your own pace!

**Generated by ASHAT Server Setup Helper Module**  
**Documentation Version:** 1.0  
**Last Updated:** 2025
