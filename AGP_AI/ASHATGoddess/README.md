# ASHATGoddess — AI Personal Chat Bot (Roman goddess persona)

## Overview
- ASHATGoddess is a privacy-first personal AI assistant with a charismatic Roman-goddess personality: wise, playful, mischievous but respectful.
- Goal: bring the nostalgic charm of personality-driven assistants (like BonziBuddy) without invasive telemetry, spyware, or surprise behavior.

This repo includes:
- Design notes and persona definitions
- Frontend UI with animated goddess visualization
- **Headless host service** for autonomous operation without GUI
- Backend connectors for LLM, TTS, ASR, and vector memory via ASHATOS endpoints
- Safety & privacy controls, opt-in telemetry and memory only

## Features

### Core Features

#### GUI Mode
- **Living Animated Goddess Visualization**
  - Ethereal, glowing figure with flowing toga appearance
  - Adorned with a laurel wreath crown (symbolizing Venus/victory)
  - Soft, golden light emanating throughout
  - Glowing golden eyes conveying divine wisdom
  - Semi-transparent, regal aesthetic
  - Intricate Roman architectural elements (decorative columns and capitals)
  - Real-time animations: idle breathing, speaking, listening, thinking, greeting
  - Smooth animation states synchronized with AI activity
  - Dynamic glow and particle effects
- **Immersive Personality**
  - Rich Roman goddess character with contextual responses
  - Divine expressions and classical references
  - Warm, approachable yet majestic demeanor
- **Interactive Chat Interface**
  - Real-time conversation with ASHAT
  - Visual feedback for user input
  - Elegant Roman-themed UI
- **Desktop Presence**
  - Configurable transparency effects
  - Customizable visual settings (colors, animations, opacity)
- **Integration with RaStudios**
  - Voice commands to launch development tools

#### Headless Mode
- Run as a standalone service without GUI
- Configurable ASHATOS server endpoints
- Robust session management
- Consent-aware persistent memory (opt-in only)
- Roman goddess persona shaping
- Enhanced personality responses in fallback mode

See [HEADLESS_README.md](HEADLESS_README.md) for detailed documentation on headless mode.

### Advanced Game Engine Features (Phase 9.1 & 9.2)

#### Phase 9.1
- **Physics Engine** - Complete 2D physics with collision detection and rigid body dynamics
- **Advanced AI** - Pathfinding (A*), behavior trees, and state machines for intelligent NPCs
- **Multiplayer Sync** - TCP-based state synchronization across multiple clients
- **Voice Chat** - Integrated voice communication with audio capture and playback

#### Phase 9.2
- **Plugin Marketplace** - Dynamic plugin loading with marketplace support for external game logic
- **Visual Scripting** - Node-based visual scripting engine for game logic without coding
- **Performance Profiling** - Real-time performance analytics with FPS, memory, and sample tracking
- **Asset Pipeline** - Advanced asset loading, caching, and optimization system

See [PHASE_9_FEATURES.md](PHASE_9_FEATURES.md) for comprehensive documentation on all advanced features.

## Getting Started

### GUI Mode (Default)
```bash
dotnet run
```

### Headless Mode
```bash
# Run with default configuration
dotnet run -- --headless

# Run with custom configuration
dotnet run -- --headless --config custom_config.json
```

### Configuration
Create or modify `appsettings.json` to configure:
- ASHATOS server URL (e.g., http://agpstudios.online)
- API endpoint paths for LLM, TTS, ASR, Memory
- Session management settings
- Roman goddess persona details
- **Visual settings** (NEW!)
  - Animation speed and effects
  - Custom glow and accent colors
  - Transparency and opacity
  - Ambient visual effects

Example visual configuration:
```json
{
  "Visual": {
    "EnableAnimations": true,
    "AnimationSpeed": 1.0,
    "EnableAmbientEffects": true,
    "GlowColor": "#8A2BE2",
    "AccentColor": "#FFD700",
    "EnableTransparency": true,
    "WindowOpacity": 0.95
  }
}
```

## Architecture

The project now features a clean separation between:
- **Host Service**: Core logic for AI processing, session management, and ASHATOS integration
- **UI Layer**: Avalonia-based GUI that uses the host service
- **Configuration**: Centralized settings for easy deployment customization

This architecture allows the host service to be reused in other applications or run autonomously as a headless service.

## Contributing
- Add issues for features, assets, and integrations
- Follow the DESIGN.md and MVP-ROADMAP.md for priority work

## License
- Add an explicit license (recommended: MIT or AGPL depending on goals)

