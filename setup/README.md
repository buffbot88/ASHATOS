# RaOS Setup Scripts

This directory contains setup scripts and configuration files for deploying and configuring RaOS on various platforms.

## Files

### ubuntu-nginx-sudoers
Sudoers configuration template for allowing RaOS to manage Nginx without password prompts on Ubuntu/Debian systems.

**Installation:**
```bash
sudo cp ubuntu-nginx-sudoers /etc/sudoers.d/raos-nginx
sudo chmod 0440 /etc/sudoers.d/raos-nginx
```

**Note:** Remember to replace `racore` with your actual RaOS username if different.

### setup-nginx-permissions.sh
Automated setup script that configures Ubuntu to allow RaOS to manage Nginx without requiring SuperAdmin terminal intervention.

**Usage:**
```bash
# For default 'racore' user:
sudo ./setup-nginx-permissions.sh

# For a different user:
sudo ./setup-nginx-permissions.sh your-username
```

**What it does:**
- Validates prerequisites (user exists, systemd available)
- Creates sudoers configuration file
- Sets correct permissions (0440)
- Validates syntax
- Tests the configuration

**Safety:**
- Only grants specific Nginx management commands
- Does not grant full sudo access
- Uses standard Linux sudoers mechanism
- Can be safely uninstalled by removing `/etc/sudoers.d/raos-nginx`

## Documentation

For detailed information about Nginx management on Ubuntu, see:
- [NGINX_MANAGEMENT_UBUNTU.md](../NGINX_MANAGEMENT_UBUNTU.md) - Complete guide
- [LINUX_HOSTING_SETUP.md](../LINUX_HOSTING_SETUP.md) - Full Ubuntu hosting setup

## Security

These scripts follow security best practices:
- **Principle of Least Privilege:** Only grants minimum required permissions
- **No Root Shell Access:** Cannot escalate to root shell
- **Limited Scope:** Only affects Nginx service management
- **Auditable:** All actions are logged in sudo logs
- **Standard Practice:** Uses the same mechanism as Docker, libvirt, etc.

## Platform Support

Currently supports:
- ✅ Ubuntu 20.04 LTS and newer
- ✅ Debian 10 and newer
- ✅ Any systemd-based Linux distribution

Should work on (untested):
- Fedora, CentOS, RHEL 8+
- Arch Linux
- openSUSE

Does not support:
- ❌ Non-systemd distributions (Alpine, Gentoo, etc.)
- ❌ macOS (uses different service management)
- ❌ Windows (uses different service management)

## Troubleshooting

If you encounter issues:

1. **Check sudoers file syntax:**
   ```bash
   sudo visudo -c -f /etc/sudoers.d/raos-nginx
   ```

2. **Verify permissions:**
   ```bash
   ls -l /etc/sudoers.d/raos-nginx
   # Should be: -r--r----- 1 root root
   ```

3. **Test manually:**
   ```bash
   sudo -l -U racore
   # Should list Nginx management commands
   ```

4. **Check logs:**
   ```bash
   sudo journalctl -u racore -n 50
   ```

## Contributing

When adding new setup scripts:
1. Add clear documentation
2. Include error handling
3. Validate all inputs
4. Test on clean Ubuntu installation
5. Update this README

## License

These scripts are part of the RaOS project and follow the same license as the main project.
