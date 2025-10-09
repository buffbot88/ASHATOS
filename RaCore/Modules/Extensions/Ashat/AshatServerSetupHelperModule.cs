using System.Text;
using Abstractions;
using RaCore.Engine.Manager;
using RaCore.Modules.Extensions.ServerSetup;

namespace RaCore.Modules.Extensions.Ashat;

/// <summary>
/// ASHAT Server Setup Helper Module
/// Provides interactive guidance for server administrators setting up RaOS public servers
/// Includes FTP configuration assistance and training/course revisit support
/// </summary>
[RaModule(Category = "extensions")]
public sealed class AshatServerSetupHelperModule : ModuleBase
{
    public override string Name => "AshatServerSetupHelper";

    private ModuleManager? _manager;
    private IServerSetupModule? _serverSetup;
    
    // Training courses/lessons tracking
    private readonly Dictionary<string, CourseInfo> _availableCourses = new();
    private readonly Dictionary<string, List<string>> _userCourseProgress = new();

    public override void Initialize(object? manager)
    {
        base.Initialize(manager);
        _manager = manager as ModuleManager;

        // Get ServerSetup module reference
        if (_manager != null)
        {
            _serverSetup = _manager.GetModuleInstanceByName("ServerSetup") as IServerSetupModule;
        }

        // Initialize available training courses
        InitializeTrainingCourses();

        LogInfo("ASHAT Server Setup Helper Module initialized");
        LogInfo("Interactive server setup guidance: READY");
        LogInfo("Training courses available for review");
    }

    public override string Process(string input)
    {
        var text = (input ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text) || text.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return GetHelp();
        }

        if (text.StartsWith("ashat setup ", StringComparison.OrdinalIgnoreCase))
        {
            text = text["ashat setup ".Length..].Trim();
        }

        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return GetHelp();

        var command = parts[0].ToLowerInvariant();

        return command switch
        {
            "guide" => GetServerSetupGuide(),
            "ftp" when parts.Length >= 2 => HandleFtpSetupGuide(parts[1]),
            "health" => CheckServerHealthWithGuidance(),
            "courses" => ListAvailableCourses(),
            "course" when parts.Length >= 2 => GetCourseContent(parts[1]),
            "launch" => LaunchPublicServerWithFtp(),
            "checklist" => GetSetupChecklist(),
            "troubleshoot" when parts.Length >= 2 => GetTroubleshootingHelp(parts[1]),
            _ => "Unknown command. Type 'ashat setup help' for available commands."
        };
    }

    private void InitializeTrainingCourses()
    {
        _availableCourses["ftp-basics"] = new CourseInfo
        {
            Id = "ftp-basics",
            Title = "FTP Server Setup Basics",
            Description = "Introduction to FTP server configuration for RaOS",
            Lessons = new List<string>
            {
                "Understanding FTP and vsftpd",
                "Installing vsftpd on Linux",
                "Basic vsftpd configuration",
                "Testing FTP connectivity"
            }
        };

        _availableCourses["ftp-security"] = new CourseInfo
        {
            Id = "ftp-security",
            Title = "Secure FTP Configuration",
            Description = "Advanced security practices for FTP access",
            Lessons = new List<string>
            {
                "Creating restricted FTP users",
                "Implementing chroot jails",
                "Setting up SSL/TLS for FTP",
                "Monitoring FTP access logs"
            }
        };

        _availableCourses["server-health"] = new CourseInfo
        {
            Id = "server-health",
            Title = "Server Health Monitoring",
            Description = "Understanding and maintaining server health",
            Lessons = new List<string>
            {
                "Server health check basics",
                "Troubleshooting common issues",
                "Monitoring essential folders",
                "Ensuring operational readiness"
            }
        };

        _availableCourses["public-server-launch"] = new CourseInfo
        {
            Id = "public-server-launch",
            Title = "Public Server Launch Guide",
            Description = "Step-by-step guide to launching a RaOS public server",
            Lessons = new List<string>
            {
                "Pre-launch server health check",
                "FTP setup and configuration",
                "Admin instance creation",
                "Security best practices",
                "Post-launch verification"
            }
        };
    }

    private string GetHelp()
    {
        return @"ASHAT Server Setup Helper - Interactive Guidance

Available Commands:
  ashat setup guide             - Get comprehensive server setup guide
  ashat setup ftp <step>        - Get FTP setup guidance (steps: check, install, configure, secure)
  ashat setup health            - Check server health with recommendations
  ashat setup launch            - Launch public server with FTP (guided workflow)
  ashat setup checklist         - Get server setup checklist
  ashat setup courses           - List available training courses
  ashat setup course <id>       - View specific course content
  ashat setup troubleshoot <issue> - Get troubleshooting help

Training Course IDs:
  - ftp-basics          : FTP Server Setup Basics
  - ftp-security        : Secure FTP Configuration
  - server-health       : Server Health Monitoring
  - public-server-launch: Public Server Launch Guide

Examples:
  ashat setup guide
  ashat setup ftp install
  ashat setup course ftp-security
  ashat setup launch";
    }

    private string GetServerSetupGuide()
    {
        var sb = new StringBuilder();
        sb.AppendLine("🚀 ASHAT Server Setup Guide");
        sb.AppendLine();
        sb.AppendLine("Welcome! I'll guide you through setting up your RaOS public server with FTP access.");
        sb.AppendLine();
        sb.AppendLine("=== Prerequisites ===");
        sb.AppendLine("✓ Linux server (Ubuntu, Debian, CentOS, etc.)");
        sb.AppendLine("✓ Root/sudo access");
        sb.AppendLine("✓ RaCore installed and running");
        sb.AppendLine();
        sb.AppendLine("=== Setup Steps ===");
        sb.AppendLine();
        sb.AppendLine("1. Check Server Health");
        sb.AppendLine("   Command: serversetup health");
        sb.AppendLine("   Purpose: Ensure live server is operational before FTP setup");
        sb.AppendLine();
        sb.AppendLine("2. Install FTP Server (if not already installed)");
        sb.AppendLine("   Command: sudo apt install vsftpd");
        sb.AppendLine("   Verify: serversetup ftp status");
        sb.AppendLine();
        sb.AppendLine("3. Create Admin Instance");
        sb.AppendLine("   Command: serversetup admin create license=<number> username=<name>");
        sb.AppendLine("   Purpose: Create isolated admin workspace");
        sb.AppendLine();
        sb.AppendLine("4. Setup FTP Access");
        sb.AppendLine("   Command: serversetup ftp setup license=<number> username=<name>");
        sb.AppendLine("   Purpose: Configure FTP directory structure and symlinks");
        sb.AppendLine();
        sb.AppendLine("5. Create Restricted FTP User (Recommended for Security)");
        sb.AppendLine("   Command: serversetup ftp createuser username=raos_ftp path=/path/to/raos");
        sb.AppendLine("   Purpose: Create secure, restricted FTP user for RaOS folder only");
        sb.AppendLine();
        sb.AppendLine("6. Get Connection Info");
        sb.AppendLine("   Command: serversetup ftp info license=<number> username=<name>");
        sb.AppendLine("   Purpose: Retrieve FTP credentials and connection details");
        sb.AppendLine();
        sb.AppendLine("7. Test FTP Connection");
        sb.AppendLine("   Use an FTP client (FileZilla, WinSCP) to verify connectivity");
        sb.AppendLine();
        sb.AppendLine("💡 Pro Tips:");
        sb.AppendLine("  - Always check server health before FTP setup");
        sb.AppendLine("  - Use restricted FTP users for enhanced security");
        sb.AppendLine("  - Review available training courses: ashat setup courses");
        sb.AppendLine("  - For guided workflow: ashat setup launch");
        sb.AppendLine();
        sb.AppendLine("Need help with a specific step? Use: ashat setup ftp <step>");
        sb.AppendLine("Available steps: check, install, configure, secure");

        return sb.ToString();
    }

    private string HandleFtpSetupGuide(string step)
    {
        return step.ToLowerInvariant() switch
        {
            "check" => GetFtpCheckGuide(),
            "install" => GetFtpInstallGuide(),
            "configure" => GetFtpConfigureGuide(),
            "secure" => GetFtpSecureGuide(),
            _ => "Unknown FTP setup step. Available: check, install, configure, secure"
        };
    }

    private string GetFtpCheckGuide()
    {
        var sb = new StringBuilder();
        sb.AppendLine("📋 FTP Setup - Step 1: Check Server Status");
        sb.AppendLine();
        sb.AppendLine("Before setting up FTP, verify your server is ready:");
        sb.AppendLine();
        sb.AppendLine("1. Check server health:");
        sb.AppendLine("   $ serversetup health");
        sb.AppendLine();
        sb.AppendLine("   This verifies:");
        sb.AppendLine("   ✓ Databases folder is accessible");
        sb.AppendLine("   ✓ PHP folder is accessible");
        sb.AppendLine("   ✓ Admins folder is accessible");
        sb.AppendLine("   ✓ FTP folder exists or can be created");
        sb.AppendLine();
        sb.AppendLine("2. Check FTP server status:");
        sb.AppendLine("   $ serversetup ftp status");
        sb.AppendLine();
        sb.AppendLine("   Expected output (if installed):");
        sb.AppendLine("   ✅ vsftpd is installed");
        sb.AppendLine("   Status: ✅ Running");
        sb.AppendLine();
        sb.AppendLine("⚠️  If server health check fails:");
        sb.AppendLine("   - Resolve any folder access issues first");
        sb.AppendLine("   - FTP setup requires operational live server");
        sb.AppendLine();
        sb.AppendLine("📚 Related course: ashat setup course server-health");
        sb.AppendLine();
        sb.AppendLine("Next step: ashat setup ftp install");

        return sb.ToString();
    }

    private string GetFtpInstallGuide()
    {
        var sb = new StringBuilder();
        sb.AppendLine("📦 FTP Setup - Step 2: Install vsftpd");
        sb.AppendLine();
        sb.AppendLine("Install the Very Secure FTP Daemon (vsftpd):");
        sb.AppendLine();
        sb.AppendLine("For Ubuntu/Debian:");
        sb.AppendLine("  $ sudo apt update");
        sb.AppendLine("  $ sudo apt install vsftpd");
        sb.AppendLine();
        sb.AppendLine("For CentOS/RHEL:");
        sb.AppendLine("  $ sudo yum install vsftpd");
        sb.AppendLine();
        sb.AppendLine("Start and enable the service:");
        sb.AppendLine("  $ sudo systemctl start vsftpd");
        sb.AppendLine("  $ sudo systemctl enable vsftpd");
        sb.AppendLine();
        sb.AppendLine("Verify installation:");
        sb.AppendLine("  $ serversetup ftp status");
        sb.AppendLine();
        sb.AppendLine("Configure firewall (if needed):");
        sb.AppendLine("  $ sudo ufw allow 21/tcp");
        sb.AppendLine("  $ sudo ufw allow 40000:50000/tcp");
        sb.AppendLine("  $ sudo ufw reload");
        sb.AppendLine();
        sb.AppendLine("📚 Related course: ashat setup course ftp-basics");
        sb.AppendLine();
        sb.AppendLine("Next step: ashat setup ftp configure");

        return sb.ToString();
    }

    private string GetFtpConfigureGuide()
    {
        var sb = new StringBuilder();
        sb.AppendLine("⚙️  FTP Setup - Step 3: Configure FTP Access");
        sb.AppendLine();
        sb.AppendLine("Configure FTP access for your admin instances:");
        sb.AppendLine();
        sb.AppendLine("1. Create an admin instance (if not already created):");
        sb.AppendLine("   $ serversetup admin create license=12345 username=admin1");
        sb.AppendLine();
        sb.AppendLine("2. Setup FTP access for the admin:");
        sb.AppendLine("   $ serversetup ftp setup license=12345 username=admin1");
        sb.AppendLine();
        sb.AppendLine("   This creates:");
        sb.AppendLine("   ✓ FTP directory structure");
        sb.AppendLine("   ✓ Symlink to admin instance");
        sb.AppendLine("   ✓ FTP configuration file");
        sb.AppendLine();
        sb.AppendLine("3. Get connection information:");
        sb.AppendLine("   $ serversetup ftp info license=12345 username=admin1");
        sb.AppendLine();
        sb.AppendLine("4. Test connection using FTP client:");
        sb.AppendLine("   - Host: Your server IP");
        sb.AppendLine("   - Port: 21");
        sb.AppendLine("   - Username: Your Linux username (e.g., racore)");
        sb.AppendLine("   - Password: Your Linux password");
        sb.AppendLine();
        sb.AppendLine("💡 Tip: Use FileZilla or WinSCP for easy FTP access");
        sb.AppendLine();
        sb.AppendLine("Next step (recommended): ashat setup ftp secure");

        return sb.ToString();
    }

    private string GetFtpSecureGuide()
    {
        var sb = new StringBuilder();
        sb.AppendLine("🔒 FTP Setup - Step 4: Secure FTP Access");
        sb.AppendLine();
        sb.AppendLine("Create a restricted FTP user for enhanced security:");
        sb.AppendLine();
        sb.AppendLine("Why create a restricted FTP user?");
        sb.AppendLine("  ✓ User is restricted to RaOS folder only (chroot jail)");
        sb.AppendLine("  ✓ No shell access to the system");
        sb.AppendLine("  ✓ Cannot navigate outside designated directory");
        sb.AppendLine("  ✓ Minimizes security risks");
        sb.AppendLine();
        sb.AppendLine("Command:");
        sb.AppendLine("  $ serversetup ftp createuser username=raos_ftp path=/path/to/raos");
        sb.AppendLine();
        sb.AppendLine("Follow the provided instructions to:");
        sb.AppendLine("  1. Create the restricted FTP user");
        sb.AppendLine("  2. Set a secure password");
        sb.AppendLine("  3. Configure directory ownership");
        sb.AppendLine("  4. Update vsftpd configuration");
        sb.AppendLine("  5. Restart vsftpd service");
        sb.AppendLine();
        sb.AppendLine("📚 Related course: ashat setup course ftp-security");
        sb.AppendLine();
        sb.AppendLine("Additional security measures:");
        sb.AppendLine("  - Enable SSL/TLS in vsftpd.conf");
        sb.AppendLine("  - Monitor FTP logs regularly");
        sb.AppendLine("  - Use strong passwords");
        sb.AppendLine("  - Limit FTP access to specific IP addresses (if possible)");
        sb.AppendLine();
        sb.AppendLine("✅ Setup complete! Your server is ready for secure FTP access.");

        return sb.ToString();
    }

    private string CheckServerHealthWithGuidance()
    {
        if (_serverSetup == null)
        {
            return "❌ ServerSetup module not available. Cannot check server health.";
        }

        var health = _serverSetup.CheckLiveServerHealthAsync().GetAwaiter().GetResult();

        var sb = new StringBuilder();
        sb.AppendLine("🏥 Server Health Check with ASHAT Guidance");
        sb.AppendLine();
        sb.AppendLine(health.Message);
        sb.AppendLine();

        if (!health.IsOperational)
        {
            sb.AppendLine("💡 ASHAT Recommendations:");
            sb.AppendLine();
            
            foreach (var issue in health.Issues)
            {
                sb.AppendLine($"Issue: {issue}");
                sb.AppendLine(GetRecommendationForIssue(issue));
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine("✅ Your server is healthy and ready for FTP setup!");
            sb.AppendLine();
            sb.AppendLine("Next steps:");
            sb.AppendLine("  1. Setup FTP: ashat setup ftp install");
            sb.AppendLine("  2. Or use guided launch: ashat setup launch");
        }

        return sb.ToString();
    }

    private string GetRecommendationForIssue(string issue)
    {
        if (issue.Contains("Databases"))
            return "  → Check if RaCore is running in the correct directory\n" +
                   "  → Verify folder permissions: ls -la Databases/";
        
        if (issue.Contains("PHP"))
            return "  → Create PHP folder: mkdir php\n" +
                   "  → Verify folder permissions: sudo chown racore:racore php/";
        
        if (issue.Contains("Admins"))
            return "  → Create Admins folder: mkdir Admins\n" +
                   "  → Folder will be created automatically on first admin instance";
        
        if (issue.Contains("FTP"))
            return "  → FTP folder will be created automatically\n" +
                   "  → Verify you have write permissions in RaCore directory";
        
        if (issue.Contains("vsftpd") && issue.Contains("not installed"))
            return "  → Install vsftpd: sudo apt install vsftpd\n" +
                   "  → Or use guided install: ashat setup ftp install";
        
        if (issue.Contains("not running"))
            return "  → Start FTP service: sudo systemctl start vsftpd\n" +
                   "  → Enable on boot: sudo systemctl enable vsftpd";

        return "  → Check server documentation for resolution\n" +
               "  → Use: ashat setup troubleshoot <issue>";
    }

    private string LaunchPublicServerWithFtp()
    {
        var sb = new StringBuilder();
        sb.AppendLine("🚀 ASHAT Public Server Launch - Guided Workflow");
        sb.AppendLine();
        sb.AppendLine("This guided workflow will help you launch a public server with FTP access.");
        sb.AppendLine();

        // Step 1: Check server health
        if (_serverSetup == null)
        {
            sb.AppendLine("❌ ServerSetup module not available. Cannot proceed.");
            return sb.ToString();
        }

        var health = _serverSetup.CheckLiveServerHealthAsync().GetAwaiter().GetResult();
        
        sb.AppendLine("Step 1: Server Health Check");
        if (health.IsOperational)
        {
            sb.AppendLine("  ✅ Server is operational");
        }
        else
        {
            sb.AppendLine($"  ❌ Server has issues: {health.Issues.Count}");
            sb.AppendLine("  → Please resolve server health issues first");
            sb.AppendLine("  → Command: ashat setup health");
            return sb.ToString();
        }
        sb.AppendLine();

        // Step 2: Check FTP status
        var ftpStatus = _serverSetup.GetFtpStatusAsync().GetAwaiter().GetResult();
        
        sb.AppendLine("Step 2: FTP Server Status");
        if (ftpStatus.IsInstalled && ftpStatus.IsRunning)
        {
            sb.AppendLine("  ✅ FTP server is installed and running");
        }
        else if (ftpStatus.IsInstalled)
        {
            sb.AppendLine("  ⚠️  FTP server installed but not running");
            sb.AppendLine("  → Start FTP: sudo systemctl start vsftpd");
        }
        else
        {
            sb.AppendLine("  ❌ FTP server not installed");
            sb.AppendLine("  → Install FTP: ashat setup ftp install");
            return sb.ToString();
        }
        sb.AppendLine();

        // Step 3: Admin instance
        sb.AppendLine("Step 3: Create Admin Instance");
        sb.AppendLine("  Command: serversetup admin create license=<number> username=<name>");
        sb.AppendLine();

        // Step 4: FTP setup
        sb.AppendLine("Step 4: Setup FTP Access");
        sb.AppendLine("  Command: serversetup ftp setup license=<number> username=<name>");
        sb.AppendLine();

        // Step 5: Secure FTP
        sb.AppendLine("Step 5: Create Restricted FTP User (Recommended)");
        sb.AppendLine("  Command: serversetup ftp createuser username=raos_ftp path=/path/to/raos");
        sb.AppendLine();

        // Step 6: Connection info
        sb.AppendLine("Step 6: Get Connection Info");
        sb.AppendLine("  Command: serversetup ftp info license=<number> username=<name>");
        sb.AppendLine();

        sb.AppendLine("✅ Your server is ready for FTP launch!");
        sb.AppendLine();
        sb.AppendLine("For detailed guidance on each step:");
        sb.AppendLine("  - ashat setup guide");
        sb.AppendLine("  - ashat setup ftp <step>");
        sb.AppendLine();
        sb.AppendLine("📚 Review training courses: ashat setup courses");

        return sb.ToString();
    }

    private string GetSetupChecklist()
    {
        var sb = new StringBuilder();
        sb.AppendLine("✅ RaOS Public Server Setup Checklist");
        sb.AppendLine();
        sb.AppendLine("Prerequisites:");
        sb.AppendLine("  [ ] Linux server (Ubuntu, Debian, CentOS, etc.)");
        sb.AppendLine("  [ ] Root/sudo access");
        sb.AppendLine("  [ ] RaCore installed and running");
        sb.AppendLine();
        sb.AppendLine("Server Health:");
        sb.AppendLine("  [ ] Run: serversetup health");
        sb.AppendLine("  [ ] All folders accessible");
        sb.AppendLine("  [ ] No critical issues reported");
        sb.AppendLine();
        sb.AppendLine("FTP Installation:");
        sb.AppendLine("  [ ] Install vsftpd: sudo apt install vsftpd");
        sb.AppendLine("  [ ] Start service: sudo systemctl start vsftpd");
        sb.AppendLine("  [ ] Enable on boot: sudo systemctl enable vsftpd");
        sb.AppendLine("  [ ] Configure firewall for FTP ports");
        sb.AppendLine();
        sb.AppendLine("Admin Instance:");
        sb.AppendLine("  [ ] Create admin: serversetup admin create license=<num> username=<name>");
        sb.AppendLine("  [ ] Verify admin folder exists");
        sb.AppendLine();
        sb.AppendLine("FTP Configuration:");
        sb.AppendLine("  [ ] Setup FTP: serversetup ftp setup license=<num> username=<name>");
        sb.AppendLine("  [ ] Verify FTP directories created");
        sb.AppendLine("  [ ] Check symlinks are working");
        sb.AppendLine();
        sb.AppendLine("Security (Recommended):");
        sb.AppendLine("  [ ] Create restricted FTP user");
        sb.AppendLine("  [ ] Configure chroot jail");
        sb.AppendLine("  [ ] Enable SSL/TLS in vsftpd");
        sb.AppendLine("  [ ] Use strong passwords");
        sb.AppendLine();
        sb.AppendLine("Testing:");
        sb.AppendLine("  [ ] Get connection info: serversetup ftp info");
        sb.AppendLine("  [ ] Test FTP connection with client");
        sb.AppendLine("  [ ] Verify file access through FTP");
        sb.AppendLine("  [ ] Check logs for any issues");
        sb.AppendLine();
        sb.AppendLine("Use 'ashat setup guide' for detailed instructions on each item.");

        return sb.ToString();
    }

    private string ListAvailableCourses()
    {
        var sb = new StringBuilder();
        sb.AppendLine("📚 ASHAT Training Courses - Server Setup");
        sb.AppendLine();
        sb.AppendLine("Available courses for review:");
        sb.AppendLine();

        foreach (var course in _availableCourses.Values)
        {
            sb.AppendLine($"[{course.Id}] {course.Title}");
            sb.AppendLine($"    {course.Description}");
            sb.AppendLine($"    Lessons: {course.Lessons.Count}");
            sb.AppendLine();
        }

        sb.AppendLine("To view a course: ashat setup course <id>");
        sb.AppendLine("Example: ashat setup course ftp-security");

        return sb.ToString();
    }

    private string GetCourseContent(string courseId)
    {
        if (!_availableCourses.TryGetValue(courseId, out var course))
        {
            return $"Course '{courseId}' not found. Use 'ashat setup courses' to list available courses.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📖 {course.Title}");
        sb.AppendLine();
        sb.AppendLine($"Description: {course.Description}");
        sb.AppendLine();
        sb.AppendLine("Lessons:");
        sb.AppendLine();

        for (int i = 0; i < course.Lessons.Count; i++)
        {
            sb.AppendLine($"{i + 1}. {course.Lessons[i]}");
        }

        sb.AppendLine();
        sb.AppendLine("💡 This is a reference course you can revisit anytime.");
        sb.AppendLine();
        
        // Provide related practical commands
        sb.AppendLine("Related Commands:");
        if (courseId.Contains("ftp"))
        {
            sb.AppendLine("  - ashat setup ftp <step>");
            sb.AppendLine("  - serversetup ftp status");
            sb.AppendLine("  - serversetup ftp setup");
        }
        if (courseId.Contains("server-health"))
        {
            sb.AppendLine("  - ashat setup health");
            sb.AppendLine("  - serversetup health");
        }
        if (courseId.Contains("launch"))
        {
            sb.AppendLine("  - ashat setup launch");
            sb.AppendLine("  - ashat setup guide");
        }

        return sb.ToString();
    }

    private string GetTroubleshootingHelp(string issue)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"🔧 Troubleshooting: {issue}");
        sb.AppendLine();

        var lowerIssue = issue.ToLowerInvariant();

        if (lowerIssue.Contains("ftp") || lowerIssue.Contains("connection"))
        {
            sb.AppendLine("Common FTP connection issues:");
            sb.AppendLine();
            sb.AppendLine("1. Cannot connect to FTP server:");
            sb.AppendLine("   → Check if vsftpd is running: sudo systemctl status vsftpd");
            sb.AppendLine("   → Check firewall: sudo ufw status");
            sb.AppendLine("   → Verify port 21 is open");
            sb.AppendLine();
            sb.AppendLine("2. Authentication failed:");
            sb.AppendLine("   → Verify username and password are correct");
            sb.AppendLine("   → Check if user exists: id <username>");
            sb.AppendLine("   → Review /var/log/vsftpd.log for errors");
            sb.AppendLine();
            sb.AppendLine("3. Permission denied:");
            sb.AppendLine("   → Check folder permissions: ls -la");
            sb.AppendLine("   → Verify user has access to the directory");
            sb.AppendLine("   → Check vsftpd configuration for chroot settings");
        }
        else if (lowerIssue.Contains("health") || lowerIssue.Contains("server"))
        {
            sb.AppendLine("Server health issues:");
            sb.AppendLine();
            sb.AppendLine("1. Folders not accessible:");
            sb.AppendLine("   → Check if running in correct directory: pwd");
            sb.AppendLine("   → Verify folder permissions: ls -la");
            sb.AppendLine("   → Create missing folders if needed");
            sb.AppendLine();
            sb.AppendLine("2. FTP server not running:");
            sb.AppendLine("   → Start vsftpd: sudo systemctl start vsftpd");
            sb.AppendLine("   → Check logs: sudo journalctl -u vsftpd");
        }
        else
        {
            sb.AppendLine("General troubleshooting steps:");
            sb.AppendLine();
            sb.AppendLine("1. Check server health: serversetup health");
            sb.AppendLine("2. Review server logs");
            sb.AppendLine("3. Verify all prerequisites are met");
            sb.AppendLine("4. Consult documentation: FTP_MANAGEMENT.md");
            sb.AppendLine();
            sb.AppendLine("For specific issues:");
            sb.AppendLine("  - FTP issues: ashat setup troubleshoot ftp");
            sb.AppendLine("  - Server issues: ashat setup troubleshoot server");
        }

        sb.AppendLine();
        sb.AppendLine("Still need help? Review training courses: ashat setup courses");

        return sb.ToString();
    }

    private class CourseInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Lessons { get; set; } = new();
    }
}
