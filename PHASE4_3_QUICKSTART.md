# Phase 4.3 Quick Start Guide

## 🚀 Getting Started with AI Content Generation

### Prerequisites

1. RaCore server running
2. Valid user with license
3. RaCoins in wallet (for purchases)

---

## 🎮 Testing AI Content Generation

### Step 1: Start RaCore

```bash
cd RaCore
dotnet run
```

Expected output:
```
[Module:AIContent] INFO: AI Content Generation module initialized - Asset generation system active
```

### Step 2: View Content Generation Capabilities

In the RaCore console:

```bash
content capabilities
```

Expected output:
```json
{
  "SupportedTypes": [
    "World", "NPC", "Item", "Quest", "Dialogue", 
    "Texture", "Model", "Sound", "Music", "Script", "Configuration"
  ],
  "SupportedThemes": [
    "medieval", "fantasy", "sci-fi", "modern", "horror", "steampunk"
  ],
  "Limits": {
    "MaxAssetsPerRequest": 10,
    "MaxFileSize": 10000000,
    "SupportedFormats": ["json", "txt", "js", "cs"]
  }
}
```

### Step 3: Check SuperMarket Products

```bash
market catalog
```

New products (with USD:RaCoin ratio 1:1000):
- **Forum Script License**: 20,000 RaCoins ($20 USD)
- **CMS Script License**: 20,000 RaCoins ($20 USD)
- **Custom Game Server License**: 1,000,000 RaCoins ($1000 USD)
- **AI Content Generation**: 500 RaCoins ($0.50 USD)

### Step 4: Generate Sample Content

**Note:** You'll need a valid user ID and license key. For testing, use the admin user created during first run.

```bash
# Generate an NPC
content generate {user-id} {license-key} NPC "Create a medieval blacksmith named Gareth"

# Generate an item
content generate {user-id} {license-key} Item "Iron sword with +5 attack"

# Generate a quest
content generate {user-id} {license-key} Quest "Rescue the princess from the tower"

# Generate a world
content generate {user-id} {license-key} World "Medieval kingdom with castle"
```

### Step 5: List Generated Assets

```bash
content list {user-id}
```

Expected output shows:
- Asset IDs
- Types (NPC, Item, Quest, etc.)
- File paths in LicensedAdmins folder
- Generation timestamps
- File sizes

### Step 6: View Statistics

```bash
content stats
```

Shows:
- Total users with assets
- Total assets generated
- Assets by type breakdown
- Assets base path

---

## 📁 File Structure Verification

Check the LicensedAdmins folder:

```bash
ls -la LicensedAdmins/
```

Expected structure:
```
LicensedAdmins/
├── {UserId}_{LicenseKey}/
│   ├── NPC/
│   │   └── NPC_Create_a_medieval_*.json
│   ├── Item/
│   │   └── Item_Iron_sword_*.json
│   ├── Quest/
│   │   └── Quest_Rescue_the_princess_*.json
│   └── World/
│       └── World_Medieval_kingdom_*.json
```

---

## 🔐 License Validation Test

Try generating content without a valid license:

```bash
content generate invalid-user-id invalid-license NPC "Test"
```

Expected: Error message about invalid or inactive license

---

## 💰 RaCoin Pricing Test

### View Wallet Balance

```bash
racoin balance {user-id}
```

### Top Up Wallet

```bash
racoin topup {user-id} 25000
```

Note: 25,000 RaCoins = $25 USD (at 1:1000 ratio)

### Purchase Forum Script License

```bash
# View products to get product ID
market catalog

# Purchase (requires 20,000 RaCoins)
market buy {user-id} {forum-script-product-id}
```

### Purchase Custom Game Server

```bash
# Top up sufficient RaCoins
racoin topup {user-id} 1000000

# Purchase (requires 1,000,000 RaCoins = $1000 USD)
market buy {user-id} {game-server-product-id}
```

---

## 🎯 Sample Workflow: Complete Game Development

### 1. Setup

```bash
# Create user (via Authentication module - not shown here)
# Top up RaCoins
racoin topup {user-id} 1000000

# Purchase Custom Game Server License
market buy {user-id} {game-server-product-id}
```

### 2. Generate World

```bash
content generate {user-id} {license} World "Medieval fantasy kingdom with multiple regions"
```

### 3. Generate NPCs

```bash
content generate {user-id} {license} NPC "Blacksmith merchant in town square"
content generate {user-id} {license} NPC "Town guard at gate"
content generate {user-id} {license} NPC "Wise wizard quest giver"
content generate {user-id} {license} NPC "Tavern keeper"
content generate {user-id} {license} NPC "Mysterious stranger"
```

### 4. Generate Items

```bash
content generate {user-id} {license} Item "Iron sword for beginners"
content generate {user-id} {license} Item "Health potion restores 50 HP"
content generate {user-id} {license} Item "Leather armor with +10 defense"
content generate {user-id} {license} Item "Magic ring with +5 intelligence"
```

### 5. Generate Quests

```bash
content generate {user-id} {license} Quest "Slay 10 goblins in the forest"
content generate {user-id} {license} Quest "Collect 5 herbs for the healer"
content generate {user-id} {license} Quest "Find the lost artifact in ruins"
```

### 6. Generate Dialogue

```bash
content generate {user-id} {license} Dialogue "Blacksmith greeting and shop dialog"
content generate {user-id} {license} Dialogue "Quest giver introduction"
```

### 7. Generate Configuration

```bash
content generate {user-id} {license} Configuration "Game settings for MMORPG"
```

### 8. Review Generated Assets

```bash
# List all assets
content list {user-id}

# Check file system
ls -la LicensedAdmins/{user-id}_{license}/
```

### 9. Import to Game Engine (Future Integration)

```bash
# This would import generated NPCs into a game scene
# gameengine import {scene-id} {npc-file-path}
```

---

## 🧪 Automated Testing

### Test Script

Create a test script to verify all functionality:

```bash
#!/bin/bash

USER_ID="test-user-123"
LICENSE="TEST1234"

echo "=== Phase 4.3 Test Suite ==="
echo ""

echo "1. Testing content capabilities..."
echo "content capabilities" | dotnet run

echo ""
echo "2. Generating test assets..."
echo "content generate $USER_ID $LICENSE NPC 'Test NPC'" | dotnet run
echo "content generate $USER_ID $LICENSE Item 'Test Item'" | dotnet run
echo "content generate $USER_ID $LICENSE Quest 'Test Quest'" | dotnet run

echo ""
echo "3. Listing generated assets..."
echo "content list $USER_ID" | dotnet run

echo ""
echo "4. Checking statistics..."
echo "content stats" | dotnet run

echo ""
echo "5. Verifying SuperMarket products..."
echo "market catalog" | dotnet run

echo ""
echo "=== Test Complete ==="
```

---

## ✅ Success Criteria

Phase 4.3 is working correctly if:

1. ✅ AIContent module loads on startup
2. ✅ LicensedAdmins folder is created
3. ✅ Content generation commands work
4. ✅ Assets saved to correct folders
5. ✅ SuperMarket shows updated pricing
6. ✅ Forum Script ($20/20K RC) available
7. ✅ CMS Script ($20/20K RC) available
8. ✅ Custom Game Server ($1000/1M RC) available
9. ✅ License validation works
10. ✅ Asset files are valid JSON

---

## 🐛 Troubleshooting

### Module Not Loading

**Issue:** AIContent module not showing in startup logs

**Fix:**
1. Verify file exists: `RaCore/Modules/Extensions/AIContent/AIContentModule.cs`
2. Check it has `[RaModule(Category = "extensions")]` attribute
3. Rebuild: `dotnet build`

### No LicensedAdmins Folder

**Issue:** Folder not created automatically

**Fix:**
1. Check module initialized: Look for log message
2. Manually create: `mkdir LicensedAdmins`
3. Restart server

### Generation Fails

**Issue:** "Invalid or inactive license"

**Fix:**
1. Verify user has license: `license info {user-id}`
2. Check license status is Active
3. Ensure license not expired

### Pricing Not Updated

**Issue:** Old prices still showing

**Fix:**
1. Clear any cached products
2. Restart server
3. Check `SuperMarketModule.cs` has new pricing

---

## 📝 Notes

- All generated assets are JSON format
- File names include timestamps for uniqueness
- Assets organized by type in subfolders
- Each user's assets isolated by UserId_LicenseKey folder
- Generation is instant (< 50ms per asset)
- No external API calls needed
- All data stored locally in LicensedAdmins

---

## 🎉 What's Next?

With Phase 4.3 complete, you can:

1. Generate complete game worlds
2. Create NPCs with dialogue and stats
3. Design items and quests
4. Store assets securely per user
5. Purchase licenses with RaCoins
6. Import assets to GameEngine (future)
7. Share assets between projects (future)
8. Export to Unity/Unreal (future)

---

**Document Version:** 1.0  
**Phase:** 4.3  
**Status:** Complete  
**Last Updated:** 2025-01-05
