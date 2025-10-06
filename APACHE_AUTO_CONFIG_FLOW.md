# Apache Auto-Configuration Flow

## Before vs After Comparison

### BEFORE: Manual Configuration Required ❌

```
┌─────────────────────────────────────────────────┐
│  User launches RaOS.exe (double-click)          │
└────────────────┬────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────┐
│  No environment variables set                   │
│  (not running from command line)                │
└────────────────┬────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────┐
│  Program.cs checks:                             │
│  RACORE_CONFIGURE_APACHE_PROXY == "true"?       │
└────────────────┬────────────────────────────────┘
                 │
                 ▼ NO
┌─────────────────────────────────────────────────┐
│  Apache NOT configured                          │
│  ❌ http://localhost doesn't work               │
│  ✅ http://localhost:5000 works                 │
│  ✅ http://localhost:8080 works (if PHP server) │
└─────────────────────────────────────────────────┘

User must:
1. Learn about environment variables
2. Set RACORE_CONFIGURE_APACHE_PROXY=true
3. Relaunch application
4. Restart Apache manually
```

### AFTER: Automatic Configuration ✅

```
┌─────────────────────────────────────────────────┐
│  User launches RaOS.exe (double-click)          │
└────────────────┬────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────┐
│  Boot Sequence Starts                           │
│  BootSequenceManager.ExecuteBootSequenceAsync() │
└────────────────┬────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────┐
│  Step 1: Self-Healing Health Checks             │
│  ✅ Check all modules                           │
│  ✅ Auto-recover degraded modules               │
└────────────────┬────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────┐
│  Step 2: Apache Configuration                   │
│  1. Detect Apache installation                  │
│  2. Find httpd.conf                             │
│  3. Check if RaCore proxy exists                │
└────────────────┬────────────────────────────────┘
                 │
         ┌───────┴────────┐
         │                │
         ▼ YES            ▼ NO
    ┌────────┐      ┌────────────────────────────┐
    │ Skip   │      │ Auto-configure:            │
    │ Already│      │ 1. Enable proxy modules    │
    │ Done ✓ │      │ 2. Add VirtualHost         │
    └────────┘      │ 3. Create backup           │
                    │ 4. Write config            │
                    └────────────┬───────────────┘
                                 │
                                 ▼
                    ┌────────────────────────────┐
                    │ Success!                   │
                    │ ✨ Apache configured       │
                    │ 💾 Backup created          │
                    │ ⚠️  Restart Apache needed  │
                    └────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────┐
│  Step 3: PHP Configuration Check                │
│  ✅ Detect PHP                                  │
│  ✅ Show version                                │
└────────────────┬────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────┐
│  Boot Complete! Server Starts                   │
│  User sees clear instructions to restart Apache │
└────────────────┬────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────┐
│  After Apache Restart:                          │
│  ✅ http://localhost works!                     │
│  ✅ http://localhost:5000 works                 │
│  ✅ Custom domain works (if configured)         │
└─────────────────────────────────────────────────┘

User experience:
1. Double-click RaOS.exe
2. See automatic configuration happen
3. Restart Apache (once)
4. Access via standard URLs
```

## Detailed Flow Diagram

```
┌──────────────────────────────────────────────────────────────┐
│                    RaOS Startup                               │
└───────────────────────────┬──────────────────────────────────┘
                            │
                            ▼
              ┌──────────────────────────┐
              │   Load Modules           │
              │   (ModuleManager)        │
              └─────────┬────────────────┘
                        │
                        ▼
              ┌──────────────────────────┐
              │   Initialize Memory      │
              │   (MemoryModule)         │
              └─────────┬────────────────┘
                        │
                        ▼
              ┌──────────────────────────────────────┐
              │   Boot Sequence Manager              │
              │   ExecuteBootSequenceAsync()         │
              └─────────┬────────────────────────────┘
                        │
        ┌───────────────┼───────────────┐
        │               │               │
        ▼               ▼               ▼
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│   Step 1    │ │   Step 2    │ │   Step 3    │
│   Health    │ │   Apache    │ │   PHP       │
│   Checks    │ │   Config    │ │   Config    │
└──────┬──────┘ └──────┬──────┘ └──────┬──────┘
       │               │               │
       │    ┌──────────▼─────────┐     │
       │    │ Apache Detected?   │     │
       │    └──────┬─────┬───────┘     │
       │           │     │             │
       │      YES  │     │ NO          │
       │           ▼     ▼             │
       │      ┌─────┐  ┌─────┐         │
       │      │Find │  │Skip │         │
       │      │Conf │  │     │         │
       │      └──┬──┘  └─────┘         │
       │         │                     │
       │         ▼                     │
       │    ┌────────────────────┐    │
       │    │ Proxy Configured?  │    │
       │    └────┬──────┬────────┘    │
       │         │      │             │
       │    YES  │      │ NO          │
       │         ▼      ▼             │
       │    ┌─────┐  ┌──────────────┐ │
       │    │Skip │  │Configure:    │ │
       │    │     │  │- Enable mods │ │
       │    │     │  │- Add VHost   │ │
       │    │     │  │- Backup conf │ │
       │    │     │  │- Write file  │ │
       │    └─────┘  └──────┬───────┘ │
       │                    │         │
       │                    ▼         │
       │            ┌───────────────┐ │
       │            │ Report Status │ │
       │            │ Show backup   │ │
       │            │ Restart hint  │ │
       │            └───────┬───────┘ │
       │                    │         │
       └────────────────────┴─────────┘
                            │
                            ▼
              ┌──────────────────────────┐
              │   First Run Check        │
              │   (if not initialized)   │
              └─────────┬────────────────┘
                        │
                        ▼
              ┌──────────────────────────┐
              │   Start Web Server       │
              │   (Kestrel on port 5000) │
              └─────────┬────────────────┘
                        │
                        ▼
              ┌──────────────────────────┐
              │   Ready!                 │
              │   - Direct: :5000        │
              │   - Apache: :80 (after   │
              │     restart)             │
              └──────────────────────────┘
```

## Key Improvements

### 1. Automatic Detection
```
Before: Manual env var → Configure
After:  Auto-detect → Auto-configure
```

### 2. Idempotent Behavior
```
First Run:  Detect → Configure → Backup → Write
Second Run: Detect → Already Configured → Skip
Third Run:  Detect → Already Configured → Skip
```

### 3. Safety First
```
1. Create timestamped backup
2. Read existing config
3. Only append (never remove)
4. Report all actions
5. Provide undo instructions
```

### 4. Smart Handling
```
┌─────────────────────────────────────────┐
│ Scenario: httpd.conf already modified   │
├─────────────────────────────────────────┤
│ Detection: "# RaCore Reverse Proxy"     │
│ Action: Skip configuration              │
│ Message: "Already configured ✓"         │
│ Result: No changes made                 │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ Scenario: Permission denied             │
├─────────────────────────────────────────┤
│ Detection: UnauthorizedAccessException  │
│ Action: Show error + instructions       │
│ Message: "Run as Administrator"         │
│ Result: Clear guidance for user         │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ Scenario: Apache not found              │
├─────────────────────────────────────────┤
│ Detection: FindApacheExecutable() fails │
│ Action: Continue boot sequence          │
│ Message: "Apache not found - okay!"     │
│ Result: App works on port 5000          │
└─────────────────────────────────────────┘
```

## Error Handling Flow

```
Auto-Configure Attempt
         │
         ▼
    ┌────────┐
    │Success?│
    └───┬─┬──┘
        │ │
    YES │ │ NO
        │ └─────────┐
        ▼           ▼
   ┌────────┐  ┌─────────────────────┐
   │Report  │  │Check error type:    │
   │Success │  │- Access Denied?     │
   │Show    │  │- File Not Found?    │
   │Backup  │  │- Module Error?      │
   │path    │  │- Other?             │
   └────────┘  └──────┬──────────────┘
                      │
            ┌─────────┼─────────┐
            │         │         │
            ▼         ▼         ▼
     ┌──────────┐ ┌─────┐ ┌────────┐
     │Admin    │ │Path │ │Generic │
     │Required │ │Help │ │Manual  │
     │Instructions│ │Show│ │Steps   │
     └──────────┘ └─────┘ └────────┘
            │         │         │
            └─────────┴─────────┘
                      │
                      ▼
            ┌──────────────────┐
            │Continue Boot     │
            │(non-fatal error) │
            └──────────────────┘
```

## User Journey

### Journey 1: Happy Path (Windows)
```
1. 👤 User downloads RaOS
2. 📦 User extracts files
3. 🖱️ User double-clicks RaOS.exe
4. ⏳ Boot sequence runs (15 seconds)
5. ✨ Apache automatically configured
6. 📝 User sees: "Restart Apache to enable"
7. 🔄 User restarts Apache service
8. 🎉 User accesses http://localhost
9. ✅ Everything works!
```

### Journey 2: No Apache (Any Platform)
```
1. 👤 User launches RaOS
2. ⏳ Boot sequence runs
3. ℹ️ "Apache not found - that's okay!"
4. ✅ Server starts on port 5000
5. 🌐 User accesses http://localhost:5000
6. ✅ Everything works!
```

### Journey 3: Linux/Mac Manual Config
```
1. 👤 User launches RaOS
2. ⏳ Boot sequence runs
3. ℹ️ Apache detected on Linux
4. 📝 Clear manual config instructions shown
5. 👤 User copies config to Apache
6. 🔄 User runs: sudo systemctl restart apache2
7. 🌐 User accesses http://localhost
8. ✅ Everything works!
```
