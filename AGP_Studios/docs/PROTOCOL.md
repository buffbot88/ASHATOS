# RaOS Integration Protocol Documentation

## Overview

This document defines the communication protocol between RaStudios (Python client) and RaOS server (C#/.NET). The protocol supports authentication, content synchronization, game management, and real-time events.

## Security

### Transport Layer Security (TLS)
- All communication MUST use TLS 1.2 or higher
- WebSocket connections use WSS (WebSocket Secure)
- REST API endpoints use HTTPS

### Authentication
RaOS uses **token-based authentication** with JWT (JSON Web Tokens):

1. **Initial Authentication**
   - Client sends credentials (username + password hash)
   - Server validates and returns access token + refresh token
   - Access token expires after 1 hour (configurable)
   - Refresh token valid for 30 days

2. **Token Refresh**
   - Before access token expires, use refresh token to get new access token
   - Refresh tokens can only be used once (rotation)

3. **Authorization Header**
   - Format: `Authorization: Bearer <access_token>`
   - Include in all authenticated requests

## Protocol Messages

All messages use JSON format. Each message has the following structure:

```json
{
  "action": "action_name",
  "auth_token": "optional_access_token",
  "...": "action-specific fields"
}
```

### Response Format

```json
{
  "success": true/false,
  "data": { ... },
  "error": "error message if success=false",
  "timestamp": "ISO 8601 timestamp"
}
```

## Authentication Actions

### 1. Authenticate User

**Request:**
```json
{
  "action": "authenticate",
  "username": "user@example.com",
  "password_hash": "SHA256 hash of password"
}
```

**Response:**
```json
{
  "success": true,
  "access_token": "eyJhbGciOiJIUzI1NiIs...",
  "refresh_token": "eyJhbGciOiJIUzI1NiIs...",
  "expires_in": 3600,
  "user_profile": {
    "user_id": "uuid",
    "username": "user@example.com",
    "display_name": "User Name"
  },
  "roles": ["player", "developer"]
}
```

### 2. Refresh Access Token

**Request:**
```json
{
  "action": "refresh_token",
  "refresh_token": "eyJhbGciOiJIUzI1NiIs..."
}
```

**Response:**
```json
{
  "success": true,
  "access_token": "new_access_token",
  "expires_in": 3600
}
```

### 3. Logout

**Request:**
```json
{
  "action": "logout",
  "access_token": "current_access_token"
}
```

**Response:**
```json
{
  "success": true
}
```

## Game Development Actions

### 4. Create Game Project

**Request:**
```json
{
  "action": "create_game_project",
  "auth_token": "access_token",
  "name": "My Awesome Game",
  "description": "An epic adventure game"
}
```

**Response:**
```json
{
  "success": true,
  "project_id": "uuid",
  "created_date": "2025-01-01T00:00:00Z"
}
```

### 5. Load Game Project

**Request:**
```json
{
  "action": "load_game_project",
  "auth_token": "access_token",
  "project_id": "uuid"
}
```

**Response:**
```json
{
  "success": true,
  "project": {
    "project_id": "uuid",
    "name": "My Awesome Game",
    "description": "An epic adventure game",
    "created_date": "2025-01-01T00:00:00Z",
    "modified_date": "2025-01-15T12:00:00Z",
    "assets": [...],
    "scenes": [...],
    "scripts": [...]
  }
}
```

### 6. Save Game Project

**Request:**
```json
{
  "action": "save_game_project",
  "auth_token": "access_token",
  "project": {
    "project_id": "uuid",
    "name": "My Awesome Game",
    "...": "project data"
  }
}
```

**Response:**
```json
{
  "success": true,
  "modified_date": "2025-01-15T12:30:00Z"
}
```

### 7. List Game Projects

**Request:**
```json
{
  "action": "list_game_projects",
  "auth_token": "access_token"
}
```

**Response:**
```json
{
  "success": true,
  "projects": [
    {
      "project_id": "uuid",
      "name": "Project 1",
      "description": "Description",
      "modified_date": "2025-01-15T12:00:00Z"
    },
    ...
  ]
}
```

### 8. Sync Assets

**Request:**
```json
{
  "action": "sync_assets",
  "auth_token": "access_token",
  "project_id": "uuid"
}
```

**Response:**
```json
{
  "success": true,
  "assets": [
    {
      "asset_id": "uuid",
      "name": "player_sprite.png",
      "type": "image",
      "url": "https://raos.server/assets/uuid",
      "size": 1024,
      "hash": "sha256_hash"
    },
    ...
  ]
}
```

### 9. Add Asset

**Request:**
```json
{
  "action": "add_asset",
  "auth_token": "access_token",
  "project_id": "uuid",
  "asset_name": "player_sprite.png",
  "asset_type": "image",
  "asset_data": "base64_encoded_data"
}
```

**Response:**
```json
{
  "success": true,
  "asset_id": "uuid",
  "asset_url": "https://raos.server/assets/uuid"
}
```

## Game Player Actions

### 10. List Available Games

**Request:**
```json
{
  "action": "list_games",
  "auth_token": "access_token"
}
```

**Response:**
```json
{
  "success": true,
  "games": [
    {
      "game_id": "uuid",
      "name": "Epic Adventure",
      "description": "An epic game",
      "genre": "RPG",
      "players": "1-4",
      "rating": "4.5/5"
    },
    ...
  ]
}
```

### 11. Launch Game

**Request:**
```json
{
  "action": "launch_game",
  "auth_token": "access_token",
  "game_id": "uuid",
  "mode": "stream" // or "download"
}
```

**Response:**
```json
{
  "success": true,
  "session_id": "uuid",
  "stream_url": "wss://raos.server/game/stream/uuid" // if mode=stream
}
```

### 12. Stop Game

**Request:**
```json
{
  "action": "stop_game",
  "auth_token": "access_token",
  "session_id": "uuid"
}
```

**Response:**
```json
{
  "success": true
}
```

### 13. Get Player Profile

**Request:**
```json
{
  "action": "get_player_profile",
  "auth_token": "access_token"
}
```

**Response:**
```json
{
  "success": true,
  "profile": {
    "username": "player123",
    "level": 42,
    "experience": 15000,
    "stats": { ... }
  }
}
```

### 14. Get Achievements

**Request:**
```json
{
  "action": "get_achievements",
  "auth_token": "access_token",
  "game_id": "uuid" // optional
}
```

**Response:**
```json
{
  "success": true,
  "achievements": [
    {
      "achievement_id": "uuid",
      "name": "First Steps",
      "description": "Complete the tutorial",
      "unlocked": true,
      "unlock_date": "2025-01-10T15:30:00Z"
    },
    ...
  ]
}
```

### 15. Get Leaderboard

**Request:**
```json
{
  "action": "get_leaderboard",
  "auth_token": "access_token",
  "game_id": "uuid",
  "category": "global" // or "friends", "regional"
}
```

**Response:**
```json
{
  "success": true,
  "leaderboard": [
    {
      "rank": 1,
      "player_name": "ProGamer123",
      "score": 999999
    },
    ...
  ]
}
```

## Content Management Actions

### 16. List Content

**Request:**
```json
{
  "action": "list_content",
  "auth_token": "access_token",
  "content_type": "blog" // optional: blog, post, image, video, etc.
}
```

**Response:**
```json
{
  "success": true,
  "content_list": [
    {
      "asset_id": "uuid",
      "asset_type": "blog",
      "title": "My Blog Post",
      "created_date": "2025-01-01T00:00:00Z",
      "modified_date": "2025-01-15T12:00:00Z"
    },
    ...
  ]
}
```

### 17. Fetch Content

**Request:**
```json
{
  "action": "fetch_content",
  "auth_token": "access_token",
  "asset_id": "uuid"
}
```

**Response:**
```json
{
  "success": true,
  "asset": {
    "asset_id": "uuid",
    "asset_type": "blog",
    "title": "My Blog Post",
    "content": "Full content text...",
    "metadata": { ... },
    "created_date": "2025-01-01T00:00:00Z",
    "modified_date": "2025-01-15T12:00:00Z"
  }
}
```

### 18. Create Content

**Request:**
```json
{
  "action": "create_content",
  "auth_token": "access_token",
  "asset_type": "blog",
  "title": "New Blog Post",
  "content": "Content text..."
}
```

**Response:**
```json
{
  "success": true,
  "asset_id": "uuid"
}
```

### 19. Update Content

**Request:**
```json
{
  "action": "update_content",
  "auth_token": "access_token",
  "asset": {
    "asset_id": "uuid",
    "asset_type": "blog",
    "title": "Updated Title",
    "content": "Updated content...",
    "metadata": { ... }
  }
}
```

**Response:**
```json
{
  "success": true,
  "modified_date": "2025-01-15T12:30:00Z"
}
```

### 20. Delete Content

**Request:**
```json
{
  "action": "delete_content",
  "auth_token": "access_token",
  "asset_id": "uuid"
}
```

**Response:**
```json
{
  "success": true
}
```

### 21. Upload Binary Asset

**Request:**
```json
{
  "action": "upload_binary_asset",
  "auth_token": "access_token",
  "asset_type": "image",
  "filename": "photo.jpg",
  "file_data": "base64_encoded_binary_data",
  "compressed": true,
  "format": "jpeg"
}
```

**Response:**
```json
{
  "success": true,
  "asset_id": "uuid",
  "asset_url": "https://raos.server/assets/uuid"
}
```

### 22. Analyze Asset

**Request:**
```json
{
  "action": "analyze_asset",
  "auth_token": "access_token",
  "asset_id": "uuid"
}
```

**Response:**
```json
{
  "success": true,
  "analysis": {
    "size": 1024000,
    "format": "PNG",
    "dimensions": "1920x1080",
    "color_space": "RGB",
    "quality_score": 0.95
  }
}
```

## Real-Time Events

RaOS supports real-time event streaming via WebSocket for live updates:

### Event Message Format

```json
{
  "event_type": "event_name",
  "timestamp": "2025-01-15T12:00:00Z",
  "data": { ... }
}
```

### Supported Event Types

1. **asset_updated** - Asset has been updated on server
2. **project_modified** - Game project modified by another user
3. **game_state_change** - Game state changed (started, paused, stopped)
4. **achievement_unlocked** - Player unlocked an achievement
5. **leaderboard_updated** - Leaderboard rankings changed
6. **content_published** - New content published
7. **system_notification** - System-wide notification

### Subscribe to Events

**Request:**
```json
{
  "action": "subscribe_events",
  "auth_token": "access_token",
  "event_types": ["asset_updated", "achievement_unlocked"]
}
```

**Response:**
```json
{
  "success": true,
  "subscription_id": "uuid"
}
```

## Error Codes

| Code | Message | Description |
|------|---------|-------------|
| 1000 | Invalid request | Malformed JSON or missing required fields |
| 1001 | Unauthorized | Missing or invalid auth token |
| 1002 | Forbidden | User lacks required permissions |
| 1003 | Not found | Requested resource not found |
| 1004 | Rate limited | Too many requests |
| 1005 | Server error | Internal server error |
| 1006 | Service unavailable | Service temporarily unavailable |

## Connection Management

### WebSocket Connection

1. **Connect**: `wss://raos.server:port/ws`
2. **Heartbeat**: Send ping every 30 seconds to keep connection alive
3. **Reconnection**: Implement exponential backoff (1s, 2s, 4s, 8s, max 60s)

### REST API Fallback

If WebSocket unavailable, use REST API:
- Base URL: `https://raos.server:port/api/v1`
- Endpoints: `/auth`, `/games`, `/content`, `/projects`

## Best Practices

1. **Always use TLS/WSS** for security
2. **Implement token refresh** before expiry
3. **Handle reconnection** gracefully
4. **Validate responses** before processing
5. **Log all errors** for debugging
6. **Implement request timeouts** (30s default)
7. **Cache frequently accessed data**
8. **Use compression** for large payloads
9. **Implement rate limiting** on client side
10. **Monitor connection health** with heartbeats

## Version History

- **v1.0** (2025-01-15): Initial protocol specification
