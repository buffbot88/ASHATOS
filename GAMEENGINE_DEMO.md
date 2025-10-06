# Game Engine Demo - Creating a Medieval Town

## Overview

This demo shows how to use the RaCore Game Engine to create a medieval town with NPCs, terrain, and interactive elements using both the API and AI-driven generation.

## Prerequisites

1. RaCore server running on `http://localhost:7077`
2. Authentication credentials (username: admin, password: admin123)
3. `curl` and `jq` installed (or use Postman/similar tool)

## Step 1: Authenticate

First, obtain an authentication token:

```bash
curl -X POST http://localhost:7077/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}'
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "token": "your-auth-token-here",
  "user": { ... },
  "tokenExpiresAt": "2025-01-06T12:00:00Z"
}
```

Save the token for subsequent requests.

## Step 2: Create a Medieval Town Scene

Create a new game scene to represent our medieval town:

```bash
curl -X POST http://localhost:7077/api/gameengine/scene \
  -H "Authorization: Bearer your-auth-token-here" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Medieval Market Town",
    "description": "A bustling medieval town with a central marketplace"
  }'
```

**Response:**
```json
{
  "success": true,
  "message": "Scene 'Medieval Market Town' created successfully",
  "data": {
    "id": "abc-123-def-456",
    "name": "Medieval Market Town",
    "description": "A bustling medieval town with a central marketplace",
    "createdAt": "2025-01-05T12:00:00Z",
    "createdBy": "admin",
    "isActive": true,
    "entities": []
  }
}
```

Save the scene ID (`abc-123-def-456` in this example) for the next steps.

## Step 3: AI-Generate Medieval Town Content

Use AI to automatically generate NPCs, terrain, and other game elements:

```bash
curl -X POST http://localhost:7077/api/gameengine/scene/abc-123-def-456/generate \
  -H "Authorization: Bearer your-auth-token-here" \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Generate a bustling medieval marketplace with merchants, guards, a blacksmith, and an innkeeper",
    "theme": "medieval",
    "entityCount": 15,
    "generateNPCs": true,
    "generateTerrain": true,
    "generateQuests": false
  }'
```

**Response:**
```json
{
  "success": true,
  "message": "Generated 16 entities in scene 'Medieval Market Town'",
  "data": [
    {
      "id": "entity-001",
      "name": "Garret_0",
      "type": "NPC",
      "position": { "x": 10, "y": 0, "z": 5 },
      "properties": {
        "dialogue": "Greetings, traveler! I am Garret_0. How may I assist you?",
        "occupation": "Blacksmith"
      },
      "createdBy": "AI:admin"
    },
    // ... 14 more NPCs
    {
      "id": "terrain-001",
      "name": "medieval Terrain",
      "type": "Terrain",
      "scale": { "x": 100, "y": 1, "z": 100 },
      "properties": {
        "theme": "medieval"
      }
    }
  ]
}
```

The AI has now generated:
- 15 medieval NPCs with appropriate names, dialogues, and occupations
- 1 terrain element sized for a town
- Randomized positions for natural placement

## Step 4: Add Custom Entities

You can also manually add specific entities with custom properties:

```bash
curl -X POST http://localhost:7077/api/gameengine/scene/abc-123-def-456/entity \
  -H "Authorization: Bearer your-auth-token-here" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Town Mayor",
    "type": "NPC",
    "position": { "x": 0, "y": 0, "z": 0 },
    "rotation": { "x": 0, "y": 90, "z": 0 },
    "scale": { "x": 1, "y": 1, "z": 1 },
    "properties": {
      "occupation": "Mayor",
      "dialogue": "Welcome to our town! I am the mayor. How can I help you?",
      "questGiver": true,
      "health": 100,
      "importance": "high"
    }
  }'
```

**Response:**
```json
{
  "success": true,
  "message": "Entity 'Town Mayor' created successfully",
  "data": {
    "id": "entity-mayor-001",
    "name": "Town Mayor",
    "type": "NPC",
    "position": { "x": 0, "y": 0, "z": 0 },
    "properties": {
      "occupation": "Mayor",
      "dialogue": "Welcome to our town! I am the mayor. How can I help you?",
      "questGiver": true,
      "health": 100,
      "importance": "high"
    },
    "createdBy": "admin"
  }
}
```

## Step 5: List All Entities

View all entities in your medieval town:

```bash
curl -X GET http://localhost:7077/api/gameengine/scene/abc-123-def-456/entities \
  -H "Authorization: Bearer your-auth-token-here"
```

**Response:**
```json
{
  "success": true,
  "entities": [
    // Array of 17 entities (15 AI-generated + 1 terrain + 1 mayor)
  ]
}
```

## Step 6: View Scene Details

Get complete scene information including all entities:

```bash
curl -X GET http://localhost:7077/api/gameengine/scene/abc-123-def-456 \
  -H "Authorization: Bearer your-auth-token-here"
```

## Step 7: Check Engine Statistics

Monitor the game engine's performance and status:

```bash
curl -X GET http://localhost:7077/api/gameengine/stats \
  -H "Authorization: Bearer your-auth-token-here"
```

**Response:**
```json
{
  "success": true,
  "stats": {
    "totalScenes": 1,
    "activeScenes": 1,
    "totalEntities": 17,
    "memoryUsageMB": 5,
    "uptime": "00:15:30",
    "connectedClients": 0,
    "startTime": "2025-01-05T10:00:00Z"
  }
}
```

## Example: Complete Medieval Town Script

Here's a complete bash script to create a medieval town:

```bash
#!/bin/bash

# Login and get token
TOKEN=$(curl -s -X POST http://localhost:7077/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}' | \
  jq -r '.token')

echo "Authenticated with token: ${TOKEN:0:20}..."

# Create scene
SCENE=$(curl -s -X POST http://localhost:7077/api/gameengine/scene \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "Medieval Town", "description": "A bustling marketplace"}' | \
  jq -r '.data.id')

echo "Created scene: $SCENE"

# AI-generate content
curl -s -X POST http://localhost:7077/api/gameengine/scene/$SCENE/generate \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Generate a medieval town with NPCs",
    "theme": "medieval",
    "entityCount": 20,
    "generateNPCs": true,
    "generateTerrain": true
  }' | jq '.message'

# Add custom mayor
curl -s -X POST http://localhost:7077/api/gameengine/scene/$SCENE/entity \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Town Mayor",
    "type": "NPC",
    "properties": {
      "occupation": "Mayor",
      "questGiver": true
    }
  }' | jq '.message'

# View results
echo "Scene entities:"
curl -s -X GET http://localhost:7077/api/gameengine/scene/$SCENE/entities \
  -H "Authorization: Bearer $TOKEN" | \
  jq '.entities | length'

echo "Engine stats:"
curl -s -X GET http://localhost:7077/api/gameengine/stats \
  -H "Authorization: Bearer $TOKEN" | \
  jq '.stats'
```

## Advanced Usage Examples

### Creating Different Themed Worlds

#### Fantasy World
```bash
curl -X POST http://localhost:7077/api/gameengine/scene/$SCENE_ID/generate \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Generate a mystical fantasy forest with elves and magic",
    "theme": "fantasy",
    "entityCount": 10,
    "generateNPCs": true,
    "generateTerrain": true
  }'
```

#### Sci-Fi Space Station
```bash
curl -X POST http://localhost:7077/api/gameengine/scene/$SCENE_ID/generate \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Generate a futuristic space station with crew members",
    "theme": "scifi",
    "entityCount": 15,
    "generateNPCs": true,
    "generateTerrain": false
  }'
```

### Creating Complex Entities

#### Quest Giver NPC
```json
{
  "name": "Wizard Merlin",
  "type": "NPC",
  "position": { "x": 25, "y": 5, "z": 10 },
  "properties": {
    "occupation": "Wizard",
    "questGiver": true,
    "quests": [
      {
        "id": "quest_001",
        "title": "Find the Ancient Artifact",
        "reward": 1000
      }
    ],
    "dialogue": {
      "greeting": "Ah, an adventurer! I have a task for you.",
      "questAccept": "Excellent! Find the ancient artifact in the Dark Forest.",
      "questComplete": "You've done it! Here is your reward."
    },
    "magicPower": 100,
    "spells": ["fireball", "teleport", "heal"]
  }
}
```

#### Interactive Building
```json
{
  "name": "Town Inn",
  "type": "Building",
  "position": { "x": -15, "y": 0, "z": 20 },
  "scale": { "x": 10, "y": 8, "z": 12 },
  "properties": {
    "buildingType": "inn",
    "interactable": true,
    "services": ["rest", "food", "lodging"],
    "owner": "Garret the Innkeeper",
    "pricePerNight": 10,
    "hasRooms": true,
    "roomCount": 6
  }
}
```

## Integration with Other RaCore Modules

### With AICodeGen Module

Generate a complete game project that includes the game engine scene:

```
codegen generate Create an MMORPG with a medieval town hub
```

This will:
1. Generate the game project structure
2. Create code for game logic
3. Include world configuration
4. Set up NPC definitions
5. Integrate with the Game Engine

### With Speech Module (via WebSocket)

Control the game engine using natural language (future enhancement):

```
"Create a new medieval scene called Dragon's Keep"
"Add 20 guards to the castle scene"
"Generate a fantasy forest with magical creatures"
```

## Monitoring & Management

### Check Scene Status
```bash
curl -X GET http://localhost:7077/api/gameengine/scenes \
  -H "Authorization: Bearer $TOKEN"
```

### Delete a Scene
```bash
curl -X DELETE http://localhost:7077/api/gameengine/scene/abc-123-def-456 \
  -H "Authorization: Bearer $TOKEN"
```

### Monitor Engine Health
```bash
watch -n 5 'curl -s -X GET http://localhost:7077/api/gameengine/stats \
  -H "Authorization: Bearer $TOKEN" | jq ".stats"'
```

## Best Practices

1. **Scene Organization**: Use descriptive scene names for easy management
2. **Entity Properties**: Add custom properties for game-specific data
3. **Position Planning**: Plan NPC positions to avoid overlap
4. **Theme Consistency**: Use consistent themes within a scene
5. **Performance**: Monitor entity counts and memory usage
6. **Permissions**: Use Admin role for scene/entity creation

## Troubleshooting

### Issue: "Insufficient permissions"
**Solution**: Ensure you're logged in as an Admin or SuperAdmin user.

### Issue: "Scene not found"
**Solution**: Verify the scene ID with `GET /api/gameengine/scenes`.

### Issue: Too many entities
**Solution**: Split content across multiple scenes or reduce entityCount.

## Next Steps

1. **Persistence**: Add database storage for scenes/entities
2. **WebSocket Events**: Real-time entity updates
3. **Physics**: Add collision and physics simulation
4. **Dashboards**: Web UI for visual scene management
5. **Client Integration**: Connect game clients to the engine

## Documentation

- Full API Reference: See [GAMEENGINE_API.md](GAMEENGINE_API.md)
- Module Documentation: See `RaCore/Modules/Extensions/GameEngine/README.md`
- Authentication Guide: See [AUTHENTICATION_QUICKSTART.md](AUTHENTICATION_QUICKSTART.md)

---

**Demo Version**: 1.0  
**Phase**: 4.1  
**Status**: ✅ Working Example  
**Last Updated**: 2025-10-06
