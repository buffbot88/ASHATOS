using System.Text;
using System.Text.Json;
using Abstractions;

namespace RaCore.Modules.Extensions.ServerSetup;

/// <summary>
/// Server Setup Module - Manages Apache, PHP, Database folders, and per-admin instance configurations.
/// Creates discoverable folder structure for RaAI to enhance on-the-go.
/// </summary>
[RaModule(Category = "extensions")]
public sealed class ServerSetupModule : ModuleBase, IServerSetupModule
{
    private readonly string _baseDirectory;
    private readonly string _databasesFolder;
    private readonly string _phpFolder;
    private readonly string _apacheFolder;
    private readonly string _adminsFolder;

    public override string Name => "ServerSetup";

    public ServerSetupModule()
    {
        // Use GetCurrentDirectory() for server root directory (where RaCore.exe runs)
        _baseDirectory = Directory.GetCurrentDirectory();
        _databasesFolder = Path.Combine(_baseDirectory, "Databases");
        _phpFolder = Path.Combine(_baseDirectory, "php");
        _apacheFolder = Path.Combine(_baseDirectory, "Apache");
        _adminsFolder = Path.Combine(_baseDirectory, "Admins");
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
        LogInfo($"  Apache folder: {_apacheFolder}");
        LogInfo($"  Admins folder: {_adminsFolder}");
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

        if (lower.StartsWith("serversetup apache setup"))
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
                    Text = "Usage: serversetup apache setup license=<number> username=<name>",
                    Status = "error"
                };
            }

            var result = await SetupApacheConfigAsync(license, username);
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

        return new ModuleResponse
        {
            Text = @"ServerSetup Module Commands:
  serversetup discover - Discover and validate server folders
  serversetup admin create license=<number> username=<name> - Create admin instance
  serversetup apache setup license=<number> username=<name> - Setup Apache config
  serversetup php setup license=<number> username=<name> - Setup PHP config",
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
                "# PHP Configuration Directory\n\nPlace PHP binaries and configuration files here.\nRaCore AI can modify configurations in this folder for dynamic enhancements.\n"
            );
        }

        // Check and create Apache folder
        result.ApacheFolderExists = Directory.Exists(_apacheFolder);
        result.ApacheFolderPath = _apacheFolder;
        if (!result.ApacheFolderExists)
        {
            Directory.CreateDirectory(_apacheFolder);
            result.CreatedFolders.Add("Apache");
            result.ApacheFolderExists = true;
            
            // Create a readme in Apache folder
            await File.WriteAllTextAsync(
                Path.Combine(_apacheFolder, "README.md"),
                "# Apache HTTP Server Configuration Directory\n\nPlace Apache binaries and configuration files here.\nRaCore AI can modify configurations in this folder for dynamic enhancements.\n"
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

            // Create httpd.conf template
            var httpdConfPath = Path.Combine(adminPath, "httpd.conf");
            await File.WriteAllTextAsync(httpdConfPath, GenerateApacheConfigTemplate(licenseNumber, username, adminPath));

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
                    php_ini = phpIniPath,
                    httpd_conf = httpdConfPath
                }
            };
            await File.WriteAllTextAsync(adminJsonPath, JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));

            return new SetupResult
            {
                Success = true,
                Message = $"✅ Admin instance created: {licenseNumber}.{username}\n" +
                         $"Path: {adminPath}\n" +
                         $"Folders: Databases, wwwroot, documents\n" +
                         $"Configs: php.ini, httpd.conf",
                Path = adminPath,
                Details = new Dictionary<string, string>
                {
                    ["databases"] = databasesPath,
                    ["wwwroot"] = wwwrootPath,
                    ["documents"] = documentsPath,
                    ["php_ini"] = phpIniPath,
                    ["httpd_conf"] = httpdConfPath
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

    public async Task<SetupResult> SetupApacheConfigAsync(string licenseNumber, string username)
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

            var httpdConfPath = Path.Combine(adminPath, "httpd.conf");
            var config = GenerateApacheConfigTemplate(licenseNumber, username, adminPath);
            await File.WriteAllTextAsync(httpdConfPath, config);

            return new SetupResult
            {
                Success = true,
                Message = $"✅ Apache configuration created for {licenseNumber}.{username}\nPath: {httpdConfPath}",
                Path = httpdConfPath,
                Details = new Dictionary<string, string>
                {
                    ["config_path"] = httpdConfPath
                }
            };
        }
        catch (Exception ex)
        {
            return new SetupResult
            {
                Success = false,
                Message = $"Error setting up Apache config: {ex.Message}"
            };
        }
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
                Message = $"✅ PHP configuration created for {licenseNumber}.{username}\nPath: {phpIniPath}",
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
        sb.AppendLine($"; RaCore AI can modify this file for on-the-go enhancements");
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
        sb.AppendLine("; Custom settings for RaCore AI");
        sb.AppendLine($"; Admin: {licenseNumber}.{username}");
        sb.AppendLine("; RaAI: Modify settings below as needed");
        sb.AppendLine();

        return sb.ToString();
    }

    private string GenerateApacheConfigTemplate(string licenseNumber, string username, string adminPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Apache HTTP Server Configuration for Admin Instance: {licenseNumber}.{username}");
        sb.AppendLine($"# Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"# RaCore AI can modify this file for on-the-go enhancements");
        sb.AppendLine();
        sb.AppendLine("<VirtualHost *:8080>");
        sb.AppendLine($"    ServerName {username}.localhost");
        sb.AppendLine($"    ServerAdmin admin@{username}.localhost");
        sb.AppendLine($"    DocumentRoot \"{Path.Combine(adminPath, "wwwroot")}\"");
        sb.AppendLine();
        sb.AppendLine($"    <Directory \"{Path.Combine(adminPath, "wwwroot")}\">");
        sb.AppendLine("        Options Indexes FollowSymLinks");
        sb.AppendLine("        AllowOverride All");
        sb.AppendLine("        Require all granted");
        sb.AppendLine("    </Directory>");
        sb.AppendLine();
        sb.AppendLine($"    ErrorLog \"{Path.Combine(adminPath, "apache_error.log")}\"");
        sb.AppendLine($"    CustomLog \"{Path.Combine(adminPath, "apache_access.log")}\" combined");
        sb.AppendLine();
        sb.AppendLine("    # PHP Configuration");
        sb.AppendLine($"    PHPIniDir \"{adminPath}\"");
        sb.AppendLine();
        sb.AppendLine("    # Custom directives for RaCore AI");
        sb.AppendLine($"    # Admin: {licenseNumber}.{username}");
        sb.AppendLine("    # RaAI: Add custom Apache directives below");
        sb.AppendLine();
        sb.AppendLine("</VirtualHost>");
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
        sb.AppendLine("├── wwwroot/            # Web root for Apache HTTP server");
        sb.AppendLine("├── documents/          # Admin documents and notes");
        sb.AppendLine("├── php.ini             # PHP configuration (AI-modifiable)");
        sb.AppendLine("├── httpd.conf          # Apache configuration (AI-modifiable)");
        sb.AppendLine("├── admin.json          # Instance metadata");
        sb.AppendLine("└── README.md           # This file");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## Configuration Files");
        sb.AppendLine();
        sb.AppendLine("### php.ini");
        sb.AppendLine("PHP configuration file for this admin instance. RaCore AI can modify this file dynamically for on-the-go enhancements.");
        sb.AppendLine();
        sb.AppendLine("### httpd.conf");
        sb.AppendLine("Apache HTTP Server configuration file. RaCore AI can update virtual host settings, document root, and other Apache directives.");
        sb.AppendLine();
        sb.AppendLine("## Usage");
        sb.AppendLine();
        sb.AppendLine("1. Place your web files in `wwwroot/`");
        sb.AppendLine("2. SQLite databases go in `Databases/`");
        sb.AppendLine("3. Store admin-specific documents in `documents/`");
        sb.AppendLine("4. RaCore AI can automatically configure Apache and PHP");
        sb.AppendLine();
        sb.AppendLine("## RaCore AI Integration");
        sb.AppendLine();
        sb.AppendLine("RaCore AI has full access to:");
        sb.AppendLine("- Modify `php.ini` for PHP configuration");
        sb.AppendLine("- Modify `httpd.conf` for Apache configuration");
        sb.AppendLine("- Create/manage databases in `Databases/`");
        sb.AppendLine("- Deploy web files to `wwwroot/`");
        sb.AppendLine("- Manage documents in `documents/`");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("*Generated by RaCore ServerSetup Module*");

        return sb.ToString();
    }

    private string FormatDiscoveryResult(DiscoveryResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Server Folder Discovery Results:");
        sb.AppendLine();
        sb.AppendLine($"✓ Databases folder: {result.DatabasesFolderPath}");
        sb.AppendLine($"✓ PHP folder: {result.PhpFolderPath}");
        sb.AppendLine($"✓ Apache folder: {result.ApacheFolderPath}");
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
}
