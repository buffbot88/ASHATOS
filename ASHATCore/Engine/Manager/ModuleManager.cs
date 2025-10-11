using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Abstractions;
using ASHATCore.Engine.Manager;

namespace ASHATCore.Engine.Manager
{
    /// <summary>
    /// Central manager for loading, tracking, and invoking ASHATCore modules.
    /// Supports dynamic discovery, async/sync invocation, event bus, diagnostics, and drop-in extension.
    /// </summary>
    public class ModuleManager : IDisposable
    {
        private readonly List<Modulewrapper> modules = [];
        private readonly HashSet<string> loadedAssemblySimpleNames = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> assemblySearchRoots = [];
        private bool resolveASHATttached = false;

        /// <summary>
        /// Enables or disables debug logging for DLL scanning, module loading, and errors.
        /// </summary>
        public bool DebugLoggingEnabled { get; set; } = false;

        public string ModuleNamespacePrefix { get; set; } = "ASHATCore.Modules";
        public IReadOnlyList<Modulewrapper> Modules => modules.AsReadOnly();
        public IReadOnlyList<string> ModuleSearchPaths => assemblySearchRoots.AsReadOnly();

        public IReadOnlyList<Modulewrapper> CoreModules => [.. modules
            .Where(static m => m.Instance != null &&
                m.Category.Equals("core", StringComparison.OrdinalIgnoreCase) &&
                m.Instance.GetType().Namespace != null &&
                (string.Equals(m.Instance.GetType().Namespace, "ASHATCore.Modules", StringComparison.Ordinal)
                 || m.Instance.GetType().Namespace!.StartsWith("ASHATCore.Modules.", StringComparison.Ordinal)))];

        public void RegisterBuiltInModule(IRaModule module)
        {
            var wrapper = new Modulewrapper(module);
            modules.Add(wrapper);
        }

        public ModuleManager()
        {
            TryAddSearchPath(AppContext.BaseDirectory);
            TryAddSearchPath(Path.Combine(AppContext.BaseDirectory));

            // Add root directory for comprehensive scanning
            var rootDir = Directory.GetCurrentDirectory();
            TryAddSearchPath(rootDir);
            TryAddSearchPath(Path.Combine(rootDir, "Modules"));

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = asm.GetName().Name;
                if (!string.IsNullOrWhiteSpace(name))
                    loadedAssemblySimpleNames.Add(name);
            }
        }

        public void AddSearchPath(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
                TryAddSearchPath(path);
        }

        private void TryAddSearchPath(string path)
        {
            try
            {
                var full = Path.GetFullPath(path);
                if (Directory.Exists(full) && !assemblySearchRoots.Contains(full, StringComparer.OrdinalIgnoreCase))
                    assemblySearchRoots.Add(full);
            }
            catch { }
        }

        public ModuleLoadResult LoadModules(bool requireAttributeOrNamespace = true)
        {
            var result = new ModuleLoadResult();

            if (DebugLoggingEnabled)
            {
                Console.WriteLine("[ModuleManager] Starting module loading...");
            }

            LoadExternalModuleAssembliesRecursive();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var candidateTypes = new List<Type>();

            foreach (var asm in assemblies)
            {
                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException rex)
                {
                    result.Errors.Add($"Failed to get types from assembly {asm.FullName}: {rex.Message}");
                    foreach (var le in rex.LoaderExceptions ?? [])
                        result.Errors.Add($" - LoaderException: {le?.GetType().Name}: {le?.Message}");
                    continue;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Failed to get types from assembly {asm.FullName}: {ex.GetType().Name}: {ex.Message}");
                    continue;
                }
                candidateTypes.AddRange(types);

                if (DebugLoggingEnabled)
                {
                    Console.WriteLine($"[ModuleManager] Assembly loaded: {asm.FullName}, Types found: {types.Length}");
                }
            }

            if (DebugLoggingEnabled)
            {
                foreach (var t in candidateTypes)
                    Console.WriteLine($"[ModuleManager] Candidate type: {t.FullName}");
            }

            var moduleTypes = candidateTypes
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    t.IsPublic &&
                    typeof(IRaModule).IsAssignableFrom(t) &&
                    t.GetConstructor(Type.EmptyTypes) != null &&
                    (
                        t.GetCustomAttribute<RaModuleAttribute>() != null
                        || (!requireAttributeOrNamespace || (t.Namespace != null && t.Namespace.StartsWith(ModuleNamespacePrefix, StringComparison.Ordinal)))
                    )
                )
                .Distinct()
                .ToList();

            if (DebugLoggingEnabled)
            {
                foreach (var t in moduleTypes)
                    Console.WriteLine($"[ModuleManager] Module type detected: {t.FullName}");
            }

            // PATCH: Move MemoryModule to front of list so it loads first
            var memoryType = moduleTypes.FirstOrDefault(t => t.Name == "MemoryModule");
            if (memoryType != null)
            {
                moduleTypes.Remove(memoryType);
                moduleTypes.Insert(0, memoryType); // Ensure MemoryModule loads before all others
            }

            var newwrappers = new List<Modulewrapper>();
            foreach (var t in moduleTypes)
            {
                try
                {
                    if (DebugLoggingEnabled)
                        Console.WriteLine($"[ModuleManager] Instantiating module: {t.FullName}");
                    var inst = (IRaModule)Activator.CreateInstance(t)!;
                    var wrapper = new Modulewrapper(inst);
                    newwrappers.Add(wrapper);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Failed to instantiate module {t.FullName}: {ex.GetType().Name}: {ex.Message}");
                    if (DebugLoggingEnabled)
                        Console.WriteLine($"[ModuleManager] ERROR: Failed to instantiate {t.FullName}: {ex.Message}");
                }
            }

            modules.AddRange(newwrappers);

            foreach (var wrapper in newwrappers)
            {
                try
                {
                    if (DebugLoggingEnabled)
                        Console.WriteLine($"[ModuleManager] Initializing module: {wrapper.Name}");
                    wrapper.Initialize(this);
                    result.Loaded.Add(new ModulewrapperView(wrapper));
                }
                catch (Exception ex)
                {
                    var t = wrapper.Instance?.GetType();
                    var name = t?.FullName ?? "(unknown)";
                    result.Errors.Add($"Failed to initialize module {name}: {ex.GetType().Name}: {ex.Message}");
                    if (DebugLoggingEnabled)
                        Console.WriteLine($"[ModuleManager] ERROR: Failed to initialize {name}: {ex.Message}");
                }
            }

            try
            {
                RaiseSystemEvent("SystemBoot");
                if (DebugLoggingEnabled)
                    Console.WriteLine("[ModuleManager] Raised SystemBoot event.");
            }
            catch (Exception ex)
            {
                if (DebugLoggingEnabled)
                    Console.WriteLine($"[ModuleManager] SystemBoot event error: {ex.Message}");
            }

            if (DebugLoggingEnabled && result.Errors.Count > 0)
            {
                Console.WriteLine("[ModuleManager] Errors during module load:");
                foreach (var err in result.Errors)
                    Console.WriteLine($"  - {err}");
            }

            return result;
        }

        public void UnloadAllModules()
        {
            foreach (var w in modules.ToList())
            {
                try { w.Dispose(); } catch { }
            }
            modules.Clear();
        }

        public ModuleLoadResult ReloadModules(bool requireAttributeOrNamespace = true)
        {
            UnloadAllModules();
            return LoadModules(requireAttributeOrNamespace);
        }

        public IRaModule? GetModuleByName(string name)
        {
            return Modules.Select(m => m.Instance).FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public Modulewrapper? GetwrapperByName(string name)
        {
            return modules.FirstOrDefault(w => string.Equals(w.Instance.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public IRaModule? GetModuleInstanceByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var inst = modules.Select(m => m.Instance).FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
            if (inst != null) return inst;
            return modules.Select(m => m.Instance).FirstOrDefault(i => string.Equals(i.GetType().Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public string? SafeInvokeModuleByName(string name, string input)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            Modulewrapper? wrapper = modules.FirstOrDefault(w =>
                string.Equals(w.Instance?.Name, name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(w.GetType().Name, name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(w.Instance?.GetType().Name, name, StringComparison.OrdinalIgnoreCase));

            object? targetInstance = wrapper?.Instance;
            if (targetInstance == null)
            {
                targetInstance = modules.Select(m => m.Instance).FirstOrDefault(i =>
                    string.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(i.GetType().Name, name, StringComparison.OrdinalIgnoreCase));
                if (targetInstance == null) return null;
            }

            try
            {
                var t = targetInstance.GetType();

                var proc = t.GetMethod("Process", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [typeof(string)], null);
                if (proc != null)
                {
                    var outObj = proc.Invoke(targetInstance, [input]);
                    if (outObj is string s && !string.IsNullOrEmpty(s)) return s;
                    if (outObj != null) return outObj.ToString();
                    return null;
                }

                if (wrapper != null)
                {
                    var wtype = wrapper.GetType();
                    var wproc = wtype.GetMethod("Process", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [typeof(string)], null)
                                ?? wtype.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [typeof(string)], null);
                    if (wproc != null)
                    {
                        var outObj = wproc.Invoke(wrapper, [input]);
                        if (outObj is string s2 && !string.IsNullOrEmpty(s2)) return s2;
                        if (outObj != null) return outObj.ToString();
                    }
                }

                var lastProp = t.GetProperty("LastResponse", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (lastProp != null)
                {
                    var val = lastProp.GetValue(targetInstance);
                    if (val is string s3 && !string.IsNullOrEmpty(s3)) return s3;
                    if (val != null) return val.ToString();
                }
            }
            catch (TargetInvocationException tie)
            {
                if (DebugLoggingEnabled)
                    Console.WriteLine($"[ModuleManager] Module invocation failed [{name}]: {tie.InnerException?.Message ?? tie.Message}");
                return $"(module {name} invocation error: {tie.InnerException?.Message ?? tie.Message})";
            }
            catch (Exception ex)
            {
                if (DebugLoggingEnabled)
                    Console.WriteLine($"[ModuleManager] Module invocation exception [{name}]: {ex.Message}");
                return $"(module {name} invocation exception: {ex.Message})";
            }

            return null;
        }

        public string? InvokeModuleProcessByNameFallback(IEnumerable<string> candidateNames, string input)
        {
            if (candidateNames == null) return null;
            foreach (var cname in candidateNames)
            {
                try
                {
                    var res = SafeInvokeModuleByName(cname, input);
                    if (!string.IsNullOrEmpty(res)) return res;
                }
                catch { }
            }
            return null;
        }

        public void RaiseSystemEvent(string name, object? payload = null)
        {
            foreach (var w in modules)
            {
                var inst = w.Instance;
                if (inst == null) continue;
                var t = inst.GetType();

                try
                {
                    if (payload != null)
                    {
                        var typedName = "On" + name;
                        var mTyped = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                      .FirstOrDefault(m =>
                                      {
                                          if (!string.Equals(m.Name, typedName, StringComparison.Ordinal)) return false;
                                          var ps = m.GetParameters();
                                          return ps.Length == 1 && ps[0].ParameterType.IsInstanceOfType(payload);
                                      });
                        if (mTyped != null)
                        {
                            mTyped.Invoke(inst, [payload]);
                            continue;
                        }
                    }

                    if (string.Equals(name, "Warmup", StringComparison.OrdinalIgnoreCase))
                    {
                        var onWarmup = t.GetMethod("OnWarmup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                        if (onWarmup != null)
                        {
                            onWarmup.Invoke(inst, null);
                            continue;
                        }
                    }

                    var onEvt2 = t.GetMethod("OnSystemEvent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [typeof(string), typeof(object)], null)
                    ?? t.GetMethod("OnSystemEvent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [typeof(string), typeof(object)], null);
                    if (onEvt2 != null)
                    {
                        onEvt2.Invoke(inst, [name, payload!]);
                        continue;
                    }

                    var onEvt1 = t.GetMethod("OnSystemEvent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [typeof(string)], null);
                    if (onEvt1 != null)
                    {
                        onEvt1.Invoke(inst, [name]);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    if (DebugLoggingEnabled)
                        Console.WriteLine($"[ModuleManager] System event '{name}' handler error in {t.FullName}: {ex.Message}");
                }
            }
        }

        private void LoadExternalModuleAssembliesRecursive()
        {
            AttachAssemblyResolverOnce();

            foreach (var root in assemblySearchRoots.ToList())
            {
                if (!Directory.Exists(root)) continue;

                IEnumerable<string> dlls;
                try { dlls = Directory.EnumerateFiles(root, "*.dll", SearchOption.AllDirectories); }
                catch { continue; }

                foreach (var path in dlls)
                {
                    var fileName = Path.GetFileName(path);

                    // Load all DLLs in the Modules folder, regardless of name
                    bool inModulesFolder = path.Contains($"{Path.DirectorySeparatorChar}Modules{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                                          || Path.GetDirectoryName(path)?.EndsWith("Modules", StringComparison.OrdinalIgnoreCase) == true;

                    if (!inModulesFolder) continue;

                    if (DebugLoggingEnabled)
                        Console.WriteLine($"[ModuleManager] Loading DLL: {path}");

                    try
                    {
                        var an = AssemblyName.GetAssemblyName(path);
                        var simple = an.Name ?? "";
                        if (string.IsNullOrWhiteSpace(simple)) continue;
                        if (loadedAssemblySimpleNames.Contains(simple)) continue;

                        Assembly.LoadFrom(path);
                        loadedAssemblySimpleNames.Add(simple);

                        if (DebugLoggingEnabled)
                            Console.WriteLine($"[ModuleManager] DLL loaded: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        if (DebugLoggingEnabled)
                            Console.WriteLine($"[ModuleManager] ERROR: Failed to load DLL {fileName}: {ex.Message}");
                    }
                }
            }
        }

        private void AttachAssemblyResolverOnce()
        {
            if (resolveASHATttached) return;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveFromSearchRoots;
            resolveASHATttached = true;
        }

        private Assembly? ResolveFromSearchRoots(object? sender, ResolveEventArgs args)
        {
            try
            {
                var requested = new AssemblyName(args.Name);
                var targetName = requested.Name + ".dll";
                foreach (var root in assemblySearchRoots)
                {
                    if (!Directory.Exists(root)) continue;

                    var match = Directory.EnumerateFiles(root, targetName, SearchOption.AllDirectories).FirstOrDefault();
                    if (match != null)
                    {
                        try { return Assembly.LoadFrom(match); } catch { }
                    }
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Scans the entire application root directory and subdirectories for discoveASHATble content.
        /// Returns information about modules, Configurations, and external resources.
        /// </summary>
        public EnvironmentDiscoveryResult DiscoverEnvironment()
        {
            var result = new EnvironmentDiscoveryResult
            {
                RootDirectory = Directory.GetCurrentDirectory(),
                AppBaseDirectory = AppContext.BaseDirectory,
                DiscoveryTime = DateTime.UtcNow
            };

            try
            {
                // Discover module folders
                result.ModuleFolders = DiscoverModuleFolders(result.RootDirectory);

                // Discover external resources (Nginx, PHP, Databases, etc.)
                result.ExternalResources = DiscoverExternalResources(result.RootDirectory);

                // Discover Configuration files
                result.ConfigurationFiles = DiscoverConfigurationFiles(result.RootDirectory);

                // Discover admin instances
                result.AdminInstances = DiscoveASHATdminInstances(result.RootDirectory);

                if (DebugLoggingEnabled)
                {
                    Console.WriteLine($"[ModuleManager] Environment Discovery Complete:");
                    Console.WriteLine($"  Root: {result.RootDirectory}");
                    Console.WriteLine($"  Module Folders: {result.ModuleFolders.Count}");
                    Console.WriteLine($"  External Resources: {result.ExternalResources.Count}");
                    Console.WriteLine($"  Configuration Files: {result.ConfigurationFiles.Count}");
                    Console.WriteLine($"  Admin Instances: {result.AdminInstances.Count}");
                }
            }
            catch (Exception ex)
            {
                if (DebugLoggingEnabled)
                    Console.WriteLine($"[ModuleManager] Environment discovery error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Scans specified folders for changes and returns update information.
        /// Useful for detecting modifications in Nginx, PHP, admin instances, and module folders.
        /// </summary>
        public FolderUpdateResult ScanForUpdates(string[] foldersToScan)
        {
            var result = new FolderUpdateResult
            {
                ScanTime = DateTime.UtcNow,
                ScannedFolders = new List<string>(foldersToScan)
            };

            foreach (var folder in foldersToScan)
            {
                try
                {
                    if (!Directory.Exists(folder)) continue;

                    var folderInfo = new FolderInfo
                    {
                        Path = folder,
                        Name = Path.GetFileName(folder) ?? folder,
                        LastModified = Directory.GetLastWriteTimeUtc(folder)
                    };

                    // Count files recursively
                    folderInfo.FileCount = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories).Count();

                    // Get subdirectories
                    folderInfo.Subdirectories = Directory.EnumerateDirectories(folder, "*", SearchOption.TopDirectoryOnly)
                        .Select(d => Path.GetFileName(d) ?? d)
                        .ToList();

                    result.FolderDetails.Add(folderInfo);

                    if (DebugLoggingEnabled)
                        Console.WriteLine($"[ModuleManager] Scanned folder: {folder} ({folderInfo.FileCount} files)");
                }
                catch (Exception ex)
                {
                    if (DebugLoggingEnabled)
                        Console.WriteLine($"[ModuleManager] Error scanning folder {folder}: {ex.Message}");
                }
            }

            return result;
        }

        private List<string> DiscoverModuleFolders(string rootDir)
        {
            var moduleFolders = new List<string>();

            try
            {
                // Check standard module locations
                var standardLocations = new[]
                {
                    Path.Combine(rootDir, "Modules"),
                    Path.Combine(rootDir, "ASHATCore", "Modules"),
                    Path.Combine(rootDir, "ASHATCore", "Modules", "Core"),
                    Path.Combine(rootDir, "ASHATCore", "Modules", "Extensions")
                };

                foreach (var location in standardLocations)
                {
                    if (Directory.Exists(location))
                    {
                        moduleFolders.Add(location);

                        // Add subdirectories as potential module folders
                        var subdirs = Directory.EnumerateDirectories(location, "*", SearchOption.TopDirectoryOnly);
                        moduleFolders.AddRange(subdirs);
                    }
                }
            }
            catch { }

            return moduleFolders;
        }

        private List<ExternalResource> DiscoverExternalResources(string rootDir)
        {
            var resources = new List<ExternalResource>();

            try
            {
                // Check for common external resources
                var resourceMappings = new Dictionary<string, string>
                {
                    { "Nginx", "Web Server" },
                    { "nginx", "Web Server" },
                    { "Apache", "Web Server" },
                    { "php", "Runtime" },
                    { "PHP", "Runtime" },
                    { "Databases", "Data Storage" },
                    { "wwwroot", "Web Content" },
                    { "Admins", "Admin Instances" }
                };

                foreach (var kvp in resourceMappings)
                {
                    var path = Path.Combine(rootDir, kvp.Key);
                    if (Directory.Exists(path))
                    {
                        resources.Add(new ExternalResource
                        {
                            Name = kvp.Key,
                            Type = kvp.Value,
                            Path = path,
                            Exists = true
                        });
                    }
                }
            }
            catch { }

            return resources;
        }

        private List<string> DiscoverConfigurationFiles(string rootDir)
        {
            var configFiles = new List<string>();

            try
            {
                // Search for common Configuration file patterns
                var patterns = new[] { "*.json", "*.conf", "*.config", "*.ini", "appsettings*.json" };

                foreach (var pattern in patterns)
                {
                    var files = Directory.EnumerateFiles(rootDir, pattern, SearchOption.AllDirectories)
                        .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                                   !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                        .Take(100); // Limit to avoid excessive results

                    configFiles.AddRange(files);
                }
            }
            catch { }

            return configFiles;
        }

        private List<string> DiscoveASHATdminInstances(string rootDir)
        {
            var instances = new List<string>();

            try
            {
                var adminsPath = Path.Combine(rootDir, "Admins");
                if (Directory.Exists(adminsPath))
                {
                    instances.AddRange(Directory.EnumerateDirectories(adminsPath, "*", SearchOption.TopDirectoryOnly));
                }
            }
            catch { }

            return instances;
        }

        public void Dispose()
        {
            UnloadAllModules();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Result of environment discovery scan
    /// </summary>
    public class EnvironmentDiscoveryResult
    {
        public string RootDirectory { get; set; } = string.Empty;
        public string AppBaseDirectory { get; set; } = string.Empty;
        public DateTime DiscoveryTime { get; set; }
        public List<string> ModuleFolders { get; set; } = new();
        public List<ExternalResource> ExternalResources { get; set; } = new();
        public List<string> ConfigurationFiles { get; set; } = new();
        public List<string> AdminInstances { get; set; } = new();
    }

    /// <summary>
    /// Information about an external resource (Nginx, PHP, etc.)
    /// </summary>
    public class ExternalResource
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool Exists { get; set; }
    }

    /// <summary>
    /// Result of folder update scan
    /// </summary>
    public class FolderUpdateResult
    {
        public DateTime ScanTime { get; set; }
        public List<string> ScannedFolders { get; set; } = new();
        public List<FolderInfo> FolderDetails { get; set; } = new();
    }

    /// <summary>
    /// Information about a scanned folder
    /// </summary>
    public class FolderInfo
    {
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public int FileCount { get; set; }
        public List<string> Subdirectories { get; set; } = new();
    }
}