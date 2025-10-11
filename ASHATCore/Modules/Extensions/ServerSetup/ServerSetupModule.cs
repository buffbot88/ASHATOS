using System.Text;
using System.Text.Json;
using Abstractions;

namespace ASHATCore.Modules.Extensions.ServerSetup;

/// <summary>
/// Server Setup Module - Manages PHP, Database folders, FTP, and per-admin instance Configurations.
/// Creates discoveASHATble folder structure for ASHATAI to enhance on-the-go.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class ServerSetupModule : ModuleBase, IServerSetupModule
{
    private readonly string _baseDirectory;
    private readonly string _databasesFolder;
    private readonly string _phpFolder;

    /// <summary>
    /// Checks if a path is absolute, does not contain tASHATversal, and is outside allowed folder root.
    /// For this project, restrict to folders within _baseDirectory or a predefined safe area.
    /// </summary>
    private bool IsSafeAbsolutePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;
        // Must be absolute
        if (!System.IO.Path.IsPathFullyQualified(path))
            return false;
        // No tASHATversal
        if (path.Contains("..") || path.Contains(";") || path.Contains("&") || path.Contains("|") || path.Contains("$") || path.IndexOfAny(new [] {'\'','"','`'}) >= 0)
            return false;
        // Within base directory (if meaningful)
        try
        {
            var normalizedBase = System.IO.Path.GetFullPath(_baseDirectory ?? "/");
            var normalizedPath = System.IO.Path.GetFullPath(path);
            if (!normalizedPath.StartsWith(normalizedBase))
                return false;
        }
        catch
        {
            return false;
        }
        return true;
    }
    private readonly string _adminsFolder;
    private readonly string _ftpFolder;

    public override string Name => "ServerSetup";

    public ServerSetupModule()
    {
        // Use GetCurrentDirectory() for server root directory (where ASHATCore.exe runs)
        _baseDirectory = Directory.GetCurrentDirectory();
        _databasesFolder = Path.Combine(_baseDirectory, "Databases");
        _phpFolder = Path.Combine(_baseDirectory, "php");
        _adminsFolder = Path.Combine(_baseDirectory, "Admins");
        _ftpFolder = Path.Combine(_baseDirectory, "ftp");
    }

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        
        // Automatically discover and create server folders on initialization
        var result = DiscoverServerFoldersAsync().GetAwaiter().GetResult();
        
        if (result.CreatedFolders.Any())
        {
            LogInfo($"Created server folders: {string.Join(", ", result.CreatedFolders)}");
        }
        
        LogInfo("ServerSetup module initialized");
        LogInfo($"  Databases folder: {_databasesFolder}");
        LogInfo($"  PHP folder: {_phpFolder}");
        LogInfo($"  Admins folder: {_adminsFolder}");
        LogInfo($"  FTP folder: {_ftpFolder}");
        
        // Check FTP status on Linux systems
        if (OperatingSystem.IsLinux())
        {
            var ftpStatus = GetFtpStatusAsync().GetAwaiter().GetResult();
            if (ftpStatus.IsInstalled)
            {
                LogInfo($"  FTP Server: {(ftpStatus.IsRunning ? "✓ Running" : "✗ Stopped")}");
            }
            else
            {
                LogInfo("  FTP Server: Not installed (optional)");
            }
        }
    }

    public override string Process(string input)
    {
        var result = ProcessAsync(input).GetAwaiter().GetResult();
        return result.Text ?? string.Empty;
    }

    private async Task<ModuleResponse> ProcessAsync(string input)
    {
        var lower = input.ToLowerInvariant().Trim();

        if (lower.StartsWith("serversetup discover"))
        {
            var result = await DiscoverServerFoldersAsync();
            return new ModuleResponse
            {
                Text = FormatDiscoveryResult(result),
                Status = "success"
            };
        }

        if (lower.StartsWith("serversetup admin create"))
        {
            // Extract license and username
            // Format: serversetup admin create license=12345 username=admin1
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string? license = null, username = null;
            
            foreach (var part in parts)
            {
                if (part.StartsWith("license="))
                    license = part.Substring("license=".Length);
                if (part.StartsWith("username="))
                    username = part.Substring("username=".Length);
            }

            if (string.IsNullOrEmpty(license) || string.IsNullOrEmpty(username))
            {
                return new ModuleResponse
                {
                    Text = "Usage: serversetup admin create license=<number> username=<name>",
                    Status = "error"
                };
            }

            var result = await CreateAdminFolderStructureAsync(license, username);
            return new ModuleResponse
            {
                Text = result.Message,
                Status = result.Success ? "success" : "error"
            };
        }

        if (lower.StartsWith("serversetup php setup"))
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string? license = null, username = null;
            
            foreach (var part in parts)
            {
                if (part.StartsWith("license="))
                    license = part.Substring("license=".Length);
                if (part.StartsWith("username="))
                    username = part.Substring("username=".Length);
            }

            if (string.IsNullOrEmpty(license) || string.IsNullOrEmpty(username))
            {
                return new ModuleResponse
                {
                    Text = "Usage: serversetup php setup license=<number> username=<name>",
                    Status = "error"
                };
            }

            var result = await SetupPhpConfigAsync(license, username);
            return new ModuleResponse
            {
                Text = result.Message,
                Status = result.Success ? "success" : "error"
            };
        }

        if (lower.StartsWith("serversetup ftp status"))
        {
            var result = await GetFtpStatusAsync();
            return new ModuleResponse
            {
                Text = FormatFtpStatusResult(result),
                Status = result.IsInstalled ? "success" : "info"
            };
        }

        if (lower.StartsWith("serversetup ftp setup"))
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string? license = null, username = null;
            
            foreach (var part in parts)
            {
                if (part.StartsWith("license="))
                    license = part.Substring("license=".Length);
                if (part.StartsWith("username="))
                    username = part.Substring("username=".Length);
            }

            if (string.IsNullOrEmpty(license) || string.IsNullOrEmpty(username))
            {
                return new ModuleResponse
                {
                    Text = "Usage: serversetup ftp setup license=<number> username=<name>",
                    Status = "error"
                };
            }

            var result = await SetupFtpAccessAsync(license, username);
            return new ModuleResponse
            {
                Text = result.Message,
                Status = result.Success ? "success" : "error"
            };
        }

        if (lower.StartsWith("serversetup ftp info"))
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string? license = null, username = null;
            
            foreach (var part in parts)
            {
                if (part.StartsWith("license="))
                    license = part.Substring("license=".Length);
                if (part.StartsWith("username="))
                    username = part.Substring("username=".Length);
            }

            if (string.IsNullOrEmpty(license) || string.IsNullOrEmpty(username))
            {
                return new ModuleResponse
                {
                    Text = "Usage: serversetup ftp info license=<number> username=<name>",
                    Status = "error"
                };
            }

            var result = await GetFtpConnectionInfoAsync(license, username);
            return new ModuleResponse
            {
                Text = result.Message,
                Status = result.Success ? "success" : "error"
            };
        }

        if (lower.StartsWith("serversetup ftp createuser"))
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string? username = null, path = null;
            
            foreach (var part in parts)
            {
                if (part.StartsWith("username="))
                    username = part.Substring("username=".Length);
                if (part.StartsWith("path="))
                    path = part.Substring("path=".Length);
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(path))
            {
                return new ModuleResponse
                {
                    Text = "Usage: serversetup ftp createuser username=<name> path=<restricted_path>\n" +
                          "Example: serversetup ftp createuser username=ASHATOS_ftp path=/home/ASHATCore/TheASHATProject/ASHATCore",
                    Status = "error"
                };
            }

            var result = await CreateRestrictedFtpUserAsync(username, path);
            return new ModuleResponse
            {
                Text = result.Message,
                Status = result.Success ? "success" : "error"
            };
        }

        if (lower.StartsWith("serversetup server health") || lower.StartsWith("serversetup health"))
        {
            var result = await CheckLiveServerHealthAsync();
            return new ModuleResponse
            {
                Text = result.Message,
                Status = result.IsOperational ? "success" : "warning"
            };
        }

        return new ModuleResponse
        {
            Text = @"ServerSetup Module Commands:
  serversetup discover - Discover and validate server folders
  serversetup admin create license=<number> username=<name> - Create admin instance
  serversetup php setup license=<number> username=<name> - Setup PHP config
  serversetup health - Check if live server is Operational
  serversetup ftp status - Check FTP server status
  serversetup ftp setup license=<number> username=<name> - Setup FTP access for admin
  serversetup ftp info license=<number> username=<name> - Get FTP connection info
  serversetup ftp createuser username=<name> path=<path> - Create restricted FTP user",
            Status = "success"
        };
    }

    public async Task<DiscoveryResult> DiscoverServerFoldersAsync()
    {
        var result = new DiscoveryResult();

        // Check and create Databases folder
        result.DatabasesFolderExists = Directory.Exists(_databasesFolder);
        result.DatabasesFolderPath = _databasesFolder;
        if (!result.DatabasesFolderExists)
        {
            Directory.CreateDirectory(_databasesFolder);
            result.CreatedFolders.Add("Databases");
            result.DatabasesFolderExists = true;
        }

        // Check and create php folder
        result.PhpFolderExists = Directory.Exists(_phpFolder);
        result.PhpFolderPath = _phpFolder;
        if (!result.PhpFolderExists)
        {
            Directory.CreateDirectory(_phpFolder);
            result.CreatedFolders.Add("php");
            result.PhpFolderExists = true;
            
            // Create a readme in php folder
            await File.WriteAllTextAsync(
                Path.Combine(_phpFolder, "README.md"),
                "# PHP Configuration Directory\n\nPlace PHP binaries and Configuration files here.\nASHATCore AI can modify Configurations in this folder for dynamic enhancements.\n"
            );
        }

        // Check and create Admins folder
        result.AdminsFolderExists = Directory.Exists(_adminsFolder);
        result.AdminsFolderPath = _adminsFolder;
        if (!result.AdminsFolderExists)
        {
            Directory.CreateDirectory(_adminsFolder);
            result.CreatedFolders.Add("Admins");
            result.AdminsFolderExists = true;
        }

        return await Task.FromResult(result);
    }

    public async Task<SetupResult> CreateAdminFolderStructureAsync(string licenseNumber, string username)
    {
        // Validate licenseNumber and username for safe path usage
        if (!IsSafePathComponent(licenseNumber) || !IsSafePathComponent(username))
        {
            return new SetupResult
            {
                Success = false,
                Message = "Invalid license number or username: possible path tASHATversal or illegal characters detected.",
                Path = null
            };
        }
        try
        {
            // Ensure base folders exist
            await DiscoverServerFoldersAsync();

            var adminPath = GetAdminInstancePath(licenseNumber, username);
            
            if (Directory.Exists(adminPath))
            {
                return new SetupResult
                {
                    Success = false,
                    Message = $"Admin instance already exists: {licenseNumber}.{username}",
                    Path = adminPath
                };
            }

            // Create admin folder structure
            Directory.CreateDirectory(adminPath);
            
            var databasesPath = Path.Combine(adminPath, "Databases");
            var wwwrootPath = Path.Combine(adminPath, "wwwroot");
            var documentsPath = Path.Combine(adminPath, "documents");
            
            Directory.CreateDirectory(databasesPath);
            Directory.CreateDirectory(wwwrootPath);
            Directory.CreateDirectory(documentsPath);

            // Create php.ini template
            var phpIniPath = Path.Combine(adminPath, "php.ini");
            await File.WriteAllTextAsync(phpIniPath, GeneratePhpIniTemplate(licenseNumber, username));

            // Create README for admin
            var readmePath = Path.Combine(adminPath, "README.md");
            await File.WriteAllTextAsync(readmePath, GenerateAdminReadme(licenseNumber, username));

            // Create admin.json metadata
            var adminJsonPath = Path.Combine(adminPath, "admin.json");
            var metadata = new
            {
                license_number = licenseNumber,
                username = username,
                created_at = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                folders = new
                {
                    databases = databasesPath,
                    wwwroot = wwwrootPath,
                    documents = documentsPath
                },
                configs = new
                {
                    php_ini = phpIniPath
                }
            };
            await File.WriteAllTextAsync(adminJsonPath, JsonSerializer.Serialize(metadata, options: new JsonSerializerOptions { WriteIndented = true }));

            return new SetupResult
            {
                Success = true,
                Message = $"✅ Admin instance created: {licenseNumber}.{username}\n" +
                         $"Path: {adminPath}\n" +
                         $"Folders: Databases, wwwroot, documents\n" +
                         $"Configs: php.ini",
                Path = adminPath,
                Details = new Dictionary<string, string>
                {
                    ["databases"] = databasesPath,
                    ["wwwroot"] = wwwrootPath,
                    ["documents"] = documentsPath,
                    ["php_ini"] = phpIniPath
                }
            };
        }
        catch (Exception ex)
        {
            return new SetupResult
            {
                Success = false,
                Message = $"Error creating admin instance: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Validates that a string is suitable as a single filesystem path component.
    /// Allows only alphanumerics, underscores, hyphens, and dots. No path Separators or tASHATversal.
    /// </summary>
    private static bool IsSafePathComponent(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;
        // Reject path Separators or tASHATversal sequences
        if (input.Contains("/") || input.Contains("\\") || input.Contains(".."))
            return false;
        // Allow only [A-Za-z0-9_.-]
        foreach (var c in input)
        {
            if (!(char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.'))
                return false;
        }
        return true;
    }

    public async Task<SetupResult> SetupPhpConfigAsync(string licenseNumber, string username)
    {
        try
        {
            var adminPath = GetAdminInstancePath(licenseNumber, username);
            
            if (!Directory.Exists(adminPath))
            {
                return new SetupResult
                {
                    Success = false,
                    Message = $"Admin instance does not exist: {licenseNumber}.{username}. Create it first."
                };
            }

            var phpIniPath = Path.Combine(adminPath, "php.ini");
            var config = GeneratePhpIniTemplate(licenseNumber, username);
            await File.WriteAllTextAsync(phpIniPath, config);

            return new SetupResult
            {
                Success = true,
                Message = $"✅ PHP Configuration created for {licenseNumber}.{username}\nPath: {phpIniPath}",
                Path = phpIniPath,
                Details = new Dictionary<string, string>
                {
                    ["config_path"] = phpIniPath
                }
            };
        }
        catch (Exception ex)
        {
            return new SetupResult
            {
                Success = false,
                Message = $"Error setting up PHP config: {ex.Message}"
            };
        }
    }

    public string GetAdminInstancePath(string licenseNumber, string username)
    {
        return Path.Combine(_adminsFolder, $"{licenseNumber}.{username}");
    }

    private string GeneratePhpIniTemplate(string licenseNumber, string username)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"; PHP Configuration for Admin Instance: {licenseNumber}.{username}");
        sb.AppendLine($"; Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"; ASHATCore AI can modify this file for on-the-go enhancements");
        sb.AppendLine();
        sb.AppendLine("[PHP]");
        sb.AppendLine("engine = On");
        sb.AppendLine("short_open_tag = Off");
        sb.AppendLine("precision = 14");
        sb.AppendLine("output_buffering = 4096");
        sb.AppendLine("zlib.output_compression = Off");
        sb.AppendLine("implicit_flush = Off");
        sb.AppendLine("serialize_precision = -1");
        sb.AppendLine("disable_functions =");
        sb.AppendLine("disable_classes =");
        sb.AppendLine("max_execution_time = 30");
        sb.AppendLine("max_input_time = 60");
        sb.AppendLine("memory_limit = 128M");
        sb.AppendLine("error_reporting = E_ALL & ~E_DEPRECATED & ~E_STRICT");
        sb.AppendLine("display_errors = Off");
        sb.AppendLine("display_startup_errors = Off");
        sb.AppendLine("log_errors = On");
        sb.AppendLine($"error_log = {Path.Combine(GetAdminInstancePath(licenseNumber, username), "php_errors.log")}");
        sb.AppendLine("post_max_size = 8M");
        sb.AppendLine("file_uploads = On");
        sb.AppendLine("upload_max_filesize = 2M");
        sb.AppendLine("max_file_uploads = 20");
        sb.AppendLine("default_socket_timeout = 60");
        sb.AppendLine();
        sb.AppendLine("[Date]");
        sb.AppendLine("date.timezone = UTC");
        sb.AppendLine();
        sb.AppendLine("[SQLite3]");
        sb.AppendLine("sqlite3.extension_dir =");
        sb.AppendLine();
        sb.AppendLine("; Custom settings for ASHATCore AI");
        sb.AppendLine($"; Admin: {licenseNumber}.{username}");
        sb.AppendLine("; ASHATAI: Modify settings below as needed");
        sb.AppendLine();

        return sb.ToString();
    }

    private string GenerateAdminReadme(string licenseNumber, string username)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Admin Instance: {licenseNumber}.{username}");
        sb.AppendLine();
        sb.AppendLine($"**License Number:** {licenseNumber}");
        sb.AppendLine($"**Username:** {username}");
        sb.AppendLine($"**Created:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();
        sb.AppendLine("## Folder Structure");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine($"{licenseNumber}.{username}/");
        sb.AppendLine("├── Databases/          # SQLite databases for CMS/Forums/GameServer");
        sb.AppendLine("├── wwwroot/            # Web root for Apache web server");
        sb.AppendLine("├── documents/          # Admin documents and notes");
        sb.AppendLine("├── php.ini             # PHP Configuration (AI-modifiable)");
        sb.AppendLine("├── admin.json          # Instance metadata");
        sb.AppendLine("└── README.md           # This file");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## Configuration Files");
        sb.AppendLine();
        sb.AppendLine("### php.ini");
        sb.AppendLine("PHP Configuration file for this admin instance. ASHATCore AI can modify this file dynamically for on-the-go enhancements.");
        sb.AppendLine();
        sb.AppendLine("## Usage");
        sb.AppendLine();
        sb.AppendLine("1. Place your web files in `wwwroot/`");
        sb.AppendLine("2. SQLite databases go in `Databases/`");
        sb.AppendLine("3. Store admin-specific documents in `documents/`");
        sb.AppendLine("4. ASHATCore AI can automatically configure Apache and PHP");
        sb.AppendLine();
        sb.AppendLine("## ASHATCore AI integration");
        sb.AppendLine();
        sb.AppendLine("ASHATCore AI has full access to:");
        sb.AppendLine("- Modify `php.ini` for PHP Configuration");
        sb.AppendLine("- Modify `httpd.conf` for Apache Configuration");
        sb.AppendLine("- Create/manage databases in `Databases/`");
        sb.AppendLine("- Deploy web files to `wwwroot/`");
        sb.AppendLine("- Manage documents in `documents/`");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("*Generated by ASHATCore ServerSetup Module*");

        return sb.ToString();
    }

    private string FormatDiscoveryResult(DiscoveryResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Server Folder Discovery Results:");
        sb.AppendLine();
        sb.AppendLine($"✓ Databases folder: {result.DatabasesFolderPath}");
        sb.AppendLine($"✓ PHP folder: {result.PhpFolderPath}");
        sb.AppendLine($"✓ Admins folder: {result.AdminsFolderPath}");
        sb.AppendLine();
        
        if (result.CreatedFolders.Count > 0)
        {
            sb.AppendLine("Created folders:");
            foreach (var folder in result.CreatedFolders)
            {
                sb.AppendLine($"  - {folder}");
            }
        }
        else
        {
            sb.AppendLine("All required folders already exist.");
        }

        return sb.ToString();
    }

    public async Task<FtpStatusResult> GetFtpStatusAsync()
    {
        var result = new FtpStatusResult
        {
            IsLinux = OperatingSystem.IsLinux()
        };

        if (!result.IsLinux)
        {
            result.Message = "FTP management is only available on Linux systems with vsftpd.";
            return result;
        }

        try
        {
            // Check if vsftpd is installed
            var checkInstalled = await ExecuteCommandAsync("which", "vsftpd");
            result.IsInstalled = checkInstalled.ExitCode == 0 && !string.IsNullOrWhiteSpace(checkInstalled.Output);

            if (result.IsInstalled)
            {
                result.Details["vsftpd_path"] = checkInstalled.Output.Trim();

                // Check if vsftpd is running
                var checkRunning = await ExecuteCommandAsync("systemctl", "is-active vsftpd");
                result.IsRunning = checkRunning.ExitCode == 0 && checkRunning.Output.Trim() == "active";

                // Get version
                var versionCheck = await ExecuteCommandAsync("vsftpd", "-v 0>&1");
                if (!string.IsNullOrWhiteSpace(versionCheck.Output))
                {
                    result.Version = versionCheck.Output.Trim();
                }

                // Check config file
                var configPath = "/etc/vsftpd.conf";
                if (File.Exists(configPath))
                {
                    result.ConfigPath = configPath;
                    result.Details["config_path"] = configPath;
                }

                result.Message = $"vsftpd is {(result.IsRunning ? "running" : "installed but not running")}";
            }
            else
            {
                result.Message = "vsftpd is not installed. Install with: sudo apt install vsftpd";
            }
        }
        catch (Exception ex)
        {
            result.Message = $"Error checking FTP status: {ex.Message}";
        }

        return result;
    }

    public async Task<SetupResult> SetupFtpAccessAsync(string licenseNumber, string username)
    {
        try
        {
            // Check if Linux
            if (!OperatingSystem.IsLinux())
            {
                return new SetupResult
                {
                    Success = false,
                    Message = "FTP setup is only available on Linux systems."
                };
            }

            // Check live server health first
            var healthCheck = await CheckLiveServerHealthAsync();
            if (!healthCheck.IsOperational)
            {
                return new SetupResult
                {
                    Success = false,
                    Message = $"❌ Live server must be Operational before FTP setup.\n\n{healthCheck.Message}\n\n" +
                             "Please resolve server health issues and try again."
                };
            }

            // Check if admin instance exists
            var adminPath = GetAdminInstancePath(licenseNumber, username);
            if (!Directory.Exists(adminPath))
            {
                return new SetupResult
                {
                    Success = false,
                    Message = $"Admin instance does not exist: {licenseNumber}.{username}. Create it first with 'serversetup admin create'."
                };
            }

            // Check FTP status
            var ftpStatus = await GetFtpStatusAsync();
            if (!ftpStatus.IsInstalled)
            {
                return new SetupResult
                {
                    Success = false,
                    Message = "vsftpd is not installed. Please install it first: sudo apt install vsftpd"
                };
            }

            // Create FTP directory structure
            var ftpPath = Path.Combine(_ftpFolder, $"{licenseNumber}.{username}");
            if (!Directory.Exists(ftpPath))
            {
                Directory.CreateDirectory(ftpPath);
            }

            var ftpFilesPath = Path.Combine(ftpPath, "files");
            if (!Directory.Exists(ftpFilesPath))
            {
                Directory.CreateDirectory(ftpFilesPath);
            }

            // Create symlink to admin instance (if not exists)
            var symlinkPath = Path.Combine(ftpFilesPath, "admin");
            if (!Directory.Exists(symlinkPath) && !File.Exists(symlinkPath))
            {
                var symlinkResult = await ExecuteCommandAsync("ln", $"-s \"{adminPath}\" \"{symlinkPath}\"");
                if (symlinkResult.ExitCode != 0)
                {
                    LogInfo($"Note: Could not create symlink: {symlinkResult.Error}");
                }
            }

            // Create FTP Configuration file for this admin
            var ftpConfigPath = Path.Combine(ftpPath, "ftp-config.txt");
            var ftpConfig = new StringBuilder();
            ftpConfig.AppendLine($"# FTP Configuration for Admin: {licenseNumber}.{username}");
            ftpConfig.AppendLine($"# Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            ftpConfig.AppendLine();
            ftpConfig.AppendLine($"FTP Root Path: {ftpPath}");
            ftpConfig.AppendLine($"Files Directory: {ftpFilesPath}");
            ftpConfig.AppendLine($"Admin Instance Link: {symlinkPath} -> {adminPath}");
            ftpConfig.AppendLine();
            ftpConfig.AppendLine("To connect via FTP:");
            ftpConfig.AppendLine($"  Host: localhost (or server IP)");
            ftpConfig.AppendLine($"  Port: 21");
            ftpConfig.AppendLine($"  Username: ASHATCore (or your Linux username)");
            ftpConfig.AppendLine($"  Path: /ftp/{licenseNumber}.{username}/files");
            ftpConfig.AppendLine();
            ftpConfig.AppendLine("Note: FTP access uses Linux system users.");
            ftpConfig.AppendLine("The Super Admin can connect using their Linux credentials.");
            ftpConfig.AppendLine();
            ftpConfig.AppendLine("For enhanced security, consider creating a restricted FTP user:");
            ftpConfig.AppendLine($"  serversetup ftp createuser username=ASHATOS_ftp path={_baseDirectory}");
            await File.WriteAllTextAsync(ftpConfigPath, ftpConfig.ToString());

            return new SetupResult
            {
                Success = true,
                Message = $"✅ FTP access configured for {licenseNumber}.{username}\n" +
                         $"FTP Path: {ftpPath}\n" +
                         $"Files Directory: {ftpFilesPath}\n" +
                         $"Admin Instance: {symlinkPath} -> {adminPath}\n" +
                         $"Use 'serversetup ftp info license={licenseNumber} username={username}' for connection details.",
                Path = ftpPath,
                Details = new Dictionary<string, string>
                {
                    ["ftp_path"] = ftpPath,
                    ["files_path"] = ftpFilesPath,
                    ["admin_link"] = symlinkPath,
                    ["config_file"] = ftpConfigPath
                }
            };
        }
        catch (Exception ex)
        {
            return new SetupResult
            {
                Success = false,
                Message = $"Error setting up FTP access: {ex.Message}"
            };
        }
    }

    public async Task<FtpConnectionInfo> GetFtpConnectionInfoAsync(string licenseNumber, string username)
    {
        try
        {
            // Check if Linux
            if (!OperatingSystem.IsLinux())
            {
                return new FtpConnectionInfo
                {
                    Success = false,
                    Message = "FTP is only available on Linux systems."
                };
            }

            // Check if admin instance exists
            var adminPath = GetAdminInstancePath(licenseNumber, username);
            if (!Directory.Exists(adminPath))
            {
                return new FtpConnectionInfo
                {
                    Success = false,
                    Message = $"Admin instance does not exist: {licenseNumber}.{username}."
                };
            }

            // Check if FTP is set up
            var ftpPath = Path.Combine(_ftpFolder, $"{licenseNumber}.{username}");
            if (!Directory.Exists(ftpPath))
            {
                return new FtpConnectionInfo
                {
                    Success = false,
                    Message = $"FTP access not configured. Use 'serversetup ftp setup license={licenseNumber} username={username}' first."
                };
            }

            // Get hostname
            var hostnameResult = await ExecuteCommandAsync("hostname", "-I");
            var hostname = hostnameResult.ExitCode == 0 && !string.IsNullOrWhiteSpace(hostnameResult.Output)
                ? hostnameResult.Output.Split(' ')[0].Trim()
                : "localhost";

            // Get current user
            var userResult = await ExecuteCommandAsync("whoami", "");
            var currentUser = userResult.ExitCode == 0 && !string.IsNullOrWhiteSpace(userResult.Output)
                ? userResult.Output.Trim()
                : "ASHATCore";

            var result = new FtpConnectionInfo
            {
                Success = true,
                Host = hostname,
                Port = 21,
                Username = currentUser,
                FtpPath = $"/ftp/{licenseNumber}.{username}/files",
                Message = $"FTP Connection Info for {licenseNumber}.{username}:\n\n" +
                         $"Host: {hostname}\n" +
                         $"Port: 21\n" +
                         $"Username: {currentUser} (Linux system user)\n" +
                         $"Password: (use Linux system password)\n" +
                         $"Remote Path: /ftp/{licenseNumber}.{username}/files\n\n" +
                         $"The 'admin' directory in FTP links to: {adminPath}\n" +
                         $"Super Admins can directly manage files through FTP.",
                Details = new Dictionary<string, string>
                {
                    ["local_ftp_path"] = ftpPath,
                    ["admin_instance_path"] = adminPath,
                    ["ftp_root"] = $"/ftp/{licenseNumber}.{username}"
                }
            };

            return result;
        }
        catch (Exception ex)
        {
            return new FtpConnectionInfo
            {
                Success = false,
                Message = $"Error getting FTP connection info: {ex.Message}"
            };
        }
    }

    private string FormatFtpStatusResult(FtpStatusResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("FTP Server Status:");
        sb.AppendLine();
        
        if (!result.IsLinux)
        {
            sb.AppendLine("❌ FTP management is only available on Linux systems.");
            return sb.ToString();
        }

        if (!result.IsInstalled)
        {
            sb.AppendLine("❌ vsftpd is not installed");
            sb.AppendLine();
            sb.AppendLine("To install vsftpd:");
            sb.AppendLine("  sudo apt install vsftpd");
            sb.AppendLine("  sudo systemctl enable vsftpd");
            sb.AppendLine("  sudo systemctl start vsftpd");
        }
        else
        {
            sb.AppendLine($"✅ vsftpd is installed");
            if (!string.IsNullOrWhiteSpace(result.Version))
            {
                sb.AppendLine($"   Version: {result.Version}");
            }
            sb.AppendLine($"   Status: {(result.IsRunning ? "✅ Running" : "❌ Stopped")}");
            
            if (!string.IsNullOrWhiteSpace(result.ConfigPath))
            {
                sb.AppendLine($"   Config: {result.ConfigPath}");
            }

            if (!result.IsRunning)
            {
                sb.AppendLine();
                sb.AppendLine("To start vsftpd:");
                sb.AppendLine("  sudo systemctl start vsftpd");
            }
        }

        return sb.ToString();
    }

    private async Task<CommandResult> ExecuteCommandAsync(string command, string arguments)
    {
        try
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = command;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            // Basic argument escaping: split args by whitespace and pass as ArgumentList if possible (.NET 6+)
#if NET6_0_OR_GREATER
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                foreach (var arg in System.Text.RegularExpressions.Regex.Matches(arguments, @"[\""].+?[\""]|[^ ]+")
                                                         .Select(m => m.ToString()))
                {
                    process.StartInfo.ArgumentList.Add(arg); // Each argument as a sepaRate item
                }
            }
#else
            // If not using .NET 6+, fall back to joining, but do NOT allow suspicious chars (see path whitelist)
            // For extASHAT safety, require all arguments to pass the safe regex (alphanumeric, underscore)
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                if (!IsAllArgumentsSafe(arguments))
                {
                    return new CommandResult
                    {
                        ExitCode = -1,
                        Error = "Unsafe arguments detected; command not executed."
                    };
                }
                process.StartInfo.Arguments = EscapeArguments(arguments);
            }
#endif

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return new CommandResult
            {
                ExitCode = process.ExitCode,
                Output = output,
                Error = error
            };
        }
        catch (Exception ex)
        {
            return new CommandResult
            {
                ExitCode = -1,
                Error = ex.Message
            };
        }
    }

    private class CommandResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    /// <summary>
    /// Create a restricted FTP user for ASHATOS folder access (Linux only)
    /// </summary>
    public async Task<SetupResult> CreateRestrictedFtpUserAsync(string username, string restrictedPath)
    {
        try
        {
            // Check if Linux
            if (!OperatingSystem.IsLinux())
            {
                return new SetupResult
                {
                    Success = false,
                    Message = "FTP user creation is only available on Linux systems."
                };
            }

            // Validate username (alphanumeric and underscore only for security)
            if (!System.Text.RegularExpressions.Regex.IsMatch(username, "^[a-zA-Z0-9_]+$"))
            {
                return new SetupResult
                {
                    Success = false,
                    Message = "Invalid username. Use only alphanumeric characters and underscores."
                };
            }

            // Sanitize and validate the restricted path
            if (!IsSafeAbsolutePath(restrictedPath))
            {
                return new SetupResult
                {
                    Success = false,
                    Message = $"Restricted path is invalid or unsafe: {restrictedPath}"
                };
            }
            if (!Directory.Exists(restrictedPath))
            {
                return new SetupResult
                {
                    Success = false,
                    Message = $"Restricted path does not exist: {restrictedPath}"
                };
            }

            // Check if user already exists
            var checkUserResult = await ExecuteCommandAsync("id", username);
            if (checkUserResult.ExitCode == 0)
            {
                return new SetupResult
                {
                    Success = false,
                    Message = $"User '{username}' already exists. Use existing user or choose a different username."
                };
            }

            // Check if vsftpd is installed
            var ftpStatus = await GetFtpStatusAsync();
            if (!ftpStatus.IsInstalled)
            {
                return new SetupResult
                {
                    Success = false,
                    Message = "vsftpd is not installed. Please install it first: sudo apt install vsftpd"
                };
            }

            var sb = new StringBuilder();
            sb.AppendLine($"⚠️  Creating FTP user '{username}' restricted to: {restrictedPath}");
            sb.AppendLine();
            sb.AppendLine("This requires root/sudo privileges. Please run the following commands:");
            sb.AppendLine();
            sb.AppendLine($"1. Create the FTP user (no shell access):");
            sb.AppendLine($"   sudo useASHATdd -m -d {restrictedPath} -s /usr/sbin/nologin {username}");
            sb.AppendLine();
            sb.AppendLine($"2. Set password for the FTP user:");
            sb.AppendLine($"   sudo passwd {username}");
            sb.AppendLine();
            sb.AppendLine($"3. Set ownership of the restricted directory:");
            sb.AppendLine($"   sudo chown {username}:{username} {restrictedPath}");
            sb.AppendLine($"   sudo chmod 755 {restrictedPath}");
            sb.AppendLine();
            sb.AppendLine($"4. Configure vsftpd to restrict user (edit /etc/vsftpd.conf):");
            sb.AppendLine($"   chroot_local_user=YES");
            sb.AppendLine($"   allow_writeable_chroot=YES");
            sb.AppendLine($"   user_sub_token=$USER");
            sb.AppendLine($"   local_root={restrictedPath}");
            sb.AppendLine();
            sb.AppendLine($"5. Restart vsftpd:");
            sb.AppendLine($"   sudo systemctl restart vsftpd");
            sb.AppendLine();
            sb.AppendLine("⚠️  SECURITY NOTE: The FTP user will be restricted to the specified directory.");
            sb.AppendLine("    This user will NOT have shell access and cannot navigate outside the restricted path.");

            return new SetupResult
            {
                Success = true,
                Message = sb.ToString(),
                Path = restrictedPath,
                Details = new Dictionary<string, string>
                {
                    ["username"] = username,
                    ["restricted_path"] = restrictedPath,
                    ["requires_sudo"] = "true"
                }
            };
        }
        catch (Exception ex)
        {
            return new SetupResult
            {
                Success = false,
                Message = $"Error creating FTP user: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Check if the live server is Operational and ready for FTP setup
    /// </summary>
    public async Task<ServerHealthResult> CheckLiveServerHealthAsync()
    {
        var result = new ServerHealthResult();
        var issues = new List<string>();

        try
        {
            // Check if essential folders are accessible
            result.DatabasesAccessible = Directory.Exists(_databasesFolder);
            if (!result.DatabasesAccessible)
            {
                issues.Add("Databases folder is not accessible");
            }
            else
            {
                result.Details["databases_path"] = _databasesFolder;
            }

            result.PhpFoldeASHATccessible = Directory.Exists(_phpFolder);
            if (!result.PhpFoldeASHATccessible)
            {
                issues.Add("PHP folder is not accessible");
            }
            else
            {
                result.Details["php_path"] = _phpFolder;
            }

            result.AdminsFoldeASHATccessible = Directory.Exists(_adminsFolder);
            if (!result.AdminsFoldeASHATccessible)
            {
                issues.Add("Admins folder is not accessible");
            }
            else
            {
                result.Details["admins_path"] = _adminsFolder;
            }

            result.FtpFoldeASHATccessible = Directory.Exists(_ftpFolder);
            if (!result.FtpFoldeASHATccessible)
            {
                // FTP folder may not exist yet - create it
                try
                {
                    Directory.CreateDirectory(_ftpFolder);
                    result.FtpFoldeASHATccessible = true;
                    result.Details["ftp_path"] = _ftpFolder;
                }
                catch
                {
                    issues.Add("FTP folder could not be created");
                }
            }
            else
            {
                result.Details["ftp_path"] = _ftpFolder;
            }

            // On Linux, check if FTP server is running
            if (OperatingSystem.IsLinux())
            {
                var ftpStatus = await GetFtpStatusAsync();
                if (ftpStatus.IsInstalled)
                {
                    result.Details["ftp_server_installed"] = "true";
                    result.Details["ftp_server_running"] = ftpStatus.IsRunning.ToString().ToLower();
                    
                    if (!ftpStatus.IsRunning)
                    {
                        issues.Add("FTP server (vsftpd) is installed but not running");
                    }
                }
                else
                {
                    result.Details["ftp_server_installed"] = "false";
                    issues.Add("FTP server (vsftpd) is not installed");
                }
            }

            result.Issues = issues;
            result.IsOperational = issues.Count == 0;

            if (result.IsOperational)
            {
                result.Message = "✅ Live server is Operational and ready for FTP setup.\n" +
                               "All essential folders are accessible and FTP server is running.";
            }
            else
            {
                result.Message = $"⚠️  Server has {issues.Count} issue(s) that should be resolved:\n" +
                               string.Join("\n", issues.Select(i => $"  - {i}"));
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            result.IsOperational = false;
            result.Message = $"Error checking server health: {ex.Message}";
        }

        return result;
    }
}
