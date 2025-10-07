using System;
using System.Text.Json;
using Abstractions;
using RaCore.Engine;

namespace RaCore.Modules.Extensions.ServerConfig;

/// <summary>
/// Server Configuration Module - Provides commands to view and manage server modes and configuration
/// </summary>
[RaModule(Category = "extensions")]
public sealed class ServerConfigModule : ModuleBase
{
    public override string Name => "ServerConfig";
    
    private FirstRunManager? _firstRunManager;
    
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    
    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        
        // Get reference to FirstRunManager - will be set by the main application
        // For now, we'll create it on demand if needed
        
        LogInfo("ServerConfig module initialized");
    }
    
    /// <summary>
    /// Set FirstRunManager reference (called by main application)
    /// </summary>
    public void SetFirstRunManager(FirstRunManager manager)
    {
        _firstRunManager = manager;
    }
    
    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim().ToLowerInvariant();
        
        if (string.IsNullOrEmpty(text) || text == "help" || text == "serverconfig help")
        {
            return GetHelp();
        }
        
        if (text == "serverconfig status" || text == "serverconfig info")
        {
            return GetServerStatus();
        }
        
        if (text == "serverconfig modes")
        {
            return GetAvailableModes();
        }
        
        if (text.StartsWith("serverconfig mode "))
        {
            var mode = text.Substring("serverconfig mode ".Length).Trim();
            return SetServerMode(mode);
        }
        
        return "Unknown serverconfig command. Type 'serverconfig help' for available commands.";
    }
    
    private string GetHelp()
    {
        return string.Join(Environment.NewLine,
            "ServerConfig Module - Manage server modes and configuration",
            "",
            "Commands:",
            "  serverconfig status        - Show current server configuration",
            "  serverconfig modes         - List available server modes",
            "  serverconfig mode <name>   - Change server mode (SuperAdmin only)",
            "",
            "Available Modes:",
            "  Alpha      - Early development and testing with full logging",
            "  Beta       - Pre-release testing with selected users",
            "  Omega      - Main server configuration (US-Omega)",
            "  Demo       - Demonstration instance with limited features",
            "  Production - Full production deployment (default)",
            "",
            "Example:",
            "  serverconfig status",
            "  serverconfig mode Beta"
        );
    }
    
    private string GetServerStatus()
    {
        if (_firstRunManager == null)
        {
            return JsonSerializer.Serialize(new
            {
                Success = false,
                Message = "FirstRunManager not available"
            }, _jsonOptions);
        }
        
        var config = _firstRunManager.GetServerConfiguration();
        
        return JsonSerializer.Serialize(new
        {
            Success = true,
            ServerMode = config.Mode.ToString(),
            IsFirstRun = config.IsFirstRun,
            InitializationCompleted = config.InitializationCompleted,
            InitializedAt = config.InitializedAt,
            Version = config.Version,
            LicenseKey = string.IsNullOrEmpty(config.LicenseKey) ? "Not Set" : "***-***-***-" + config.LicenseKey.Substring(Math.Max(0, config.LicenseKey.Length - 4)),
            LicenseType = config.LicenseType ?? "Not Set",
            AdminPasswordChanged = config.AdminPasswordChanged,
            AdminUsernameChanged = config.AdminUsernameChanged,
            AshatEnabled = config.AshatEnabled,
            SystemRequirementsMet = config.SystemRequirementsMet,
            SystemWarnings = config.SystemWarnings,
            CmsPath = config.CmsPath,
            MainServerUrl = config.MainServerUrl
        }, _jsonOptions);
    }
    
    private string GetAvailableModes()
    {
        var modes = Enum.GetValues<ServerMode>();
        var modeList = new List<object>();
        
        foreach (var mode in modes)
        {
            modeList.Add(new
            {
                Name = mode.ToString(),
                Value = (int)mode,
                Description = mode switch
                {
                    ServerMode.Alpha => "Early development and testing with full logging",
                    ServerMode.Beta => "Pre-release testing with selected users",
                    ServerMode.Omega => "Main server configuration (US-Omega)",
                    ServerMode.Demo => "Demonstration instance with limited features",
                    ServerMode.Production => "Full production deployment",
                    _ => ""
                }
            });
        }
        
        return JsonSerializer.Serialize(new
        {
            Success = true,
            AvailableModes = modeList
        }, _jsonOptions);
    }
    
    private string SetServerMode(string modeName)
    {
        if (_firstRunManager == null)
        {
            return JsonSerializer.Serialize(new
            {
                Success = false,
                Message = "FirstRunManager not available"
            }, _jsonOptions);
        }
        
        // Try to parse the mode
        if (!Enum.TryParse<ServerMode>(modeName, true, out var mode))
        {
            return JsonSerializer.Serialize(new
            {
                Success = false,
                Message = $"Invalid server mode: {modeName}",
                AvailableModes = Enum.GetNames<ServerMode>()
            }, _jsonOptions);
        }
        
        try
        {
            var oldMode = _firstRunManager.GetServerConfiguration().Mode;
            _firstRunManager.SetServerMode(mode);
            
            return JsonSerializer.Serialize(new
            {
                Success = true,
                Message = $"Server mode changed from {oldMode} to {mode}",
                OldMode = oldMode.ToString(),
                NewMode = mode.ToString(),
                Note = "Restart RaCore for changes to take full effect"
            }, _jsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                Success = false,
                Message = $"Failed to set server mode: {ex.Message}"
            }, _jsonOptions);
        }
    }
}
