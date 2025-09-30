# RaProject v2 Build Phases Roadmap

**Branch:** `main`  
**Version:** 2.x (AI Host + Modular UI)

---

## Phase 1: Core UI Foundation
- [x] Clean project structure & remove legacy/unneeded files
- [x] Ensure .csproj files do not manually include sources/pages (use SDK defaults)
- [x] Set up main app manifest/assets/config/global usings
- [x] Dynamic Modules menu (populated from Ra.Core.ModuleManager)
- [x] Docked properties panel on left (shows only one module at a time)
- [x] Main console/log output area
- [x] Initial testing and UI polish

---

## Phase 2: Advanced Features (*Current Phase*)
- [x] Module property panels: show commands, status, help, etc. per module
- [x] Command execution UI (send input to modules, show output)
- [ ] Localization and multi-language UI
- [ ] Theme customization (dark/light mode, etc.)
- [ ] User/configurable settings panel
- [ ] Error handling and diagnostics UI
- [ ] Performance tuning and async module events

---

## Phase 3: AI Game Server Mainframe & Extensions
- [ ] **RaCore module improvements** (core system upgrades, APIs, new features)
- [ ] **Game "screen" drawing panel** (UI for 2D/3D output, rendering, and visual debugging)
- [ ] Begin work on **AI Game Server Mainframe/Client Birther** for generating 3D MMORPG games
- [ ] Add support for external APIs (weather, etc.) as plugins
- [ ] Add skill/module store/discovery UI
- [ ] Export/import module settings and user data
- [ ] Scripting interface for advanced users

---

## Phase 4: Release, Docs, & Community
- [ ] Write user and dev documentation
- [ ] Finalize release builds and packaging
- [ ] Launch demo, gather community feedback
- [ ] Plan next major version/features

---

## Notes
- This file will be updated as we complete steps and plan new features.
- If you want to add/track issues, reference this roadmap for phase context.

---
