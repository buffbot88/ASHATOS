# AGP_Studios — API Endpoint List (v1)

This document lists recommended HTTP and realtime endpoints AGP_Studios (the desktop client/launcher) will use to interact with backend services and local runtime components. Grouped by service and purpose. Use /api/v1 as the base for HTTP endpoints. Add appropriate OAuth2 scopes and JWTs for auth; endpoints that require stronger privileges note required scopes.

Tags: auth, users, catalog, assets, builds, launcher, server, session, matchmaking, social, ai, admin, webhook, realtime

---

## Conventions
- Base path: /api/v1
- Auth:
  - Public endpoints: no auth required
  - User endpoints: Bearer JWT (access_token)
  - Service/admin endpoints: Bearer JWT with admin/service scope + mTLS recommended
  - AGP_AI actions that apply patches require an extra consent token header X-AGP-AI-Consent: true
- Pagination: use `?page=<n>&per_page=<n>` and Link header
- Idempotency: use Idempotency-Key header for create/upload operations where applicable
- Errors: standard JSON Problem Details { type, title, status, detail, instance, errors? }

---

# 1 — Authentication & Accounts (AGP_CMS)
- POST /api/v1/auth/register
  - Description: Create a new user account (email/password).
  - Auth: public
  - Body: { "username","email","password" }
  - Response: 201 { "id","username","email","created_at" }

- POST /api/v1/auth/login
  - Description: Obtain access and refresh tokens.
  - Auth: public
  - Body: { "email","password" }
  - Response: 200 { "access_token","refresh_token","expires_in","scopes" }

- POST /api/v1/auth/refresh
  - Description: Refresh access_token using refresh_token.
  - Auth: public
  - Body: { "refresh_token" }
  - Response: 200 { "access_token","refresh_token","expires_in" }

- POST /api/v1/auth/logout
  - Description: Revoke current refresh token & logout.
  - Auth: Bearer
  - Response: 204

- GET /api/v1/users/me
  - Description: Get current user profile.
  - Auth: Bearer
  - Response: 200 { id, username, email, avatar_url, roles[], created_at }

- PATCH /api/v1/users/me
  - Description: Update profile (display name, avatar).
  - Auth: Bearer
  - Body: { "display_name?", "avatar_url?" }
  - Response: 200 updated profile

---

# 2 — Catalog / Library / Marketplace (AGP_CMS)
- GET /api/v1/catalog
  - Description: List published catalog items (games, templates, assets).
  - Auth: optional (public listing)
  - Query: ?q=&category=&sort=&page=&per_page=
  - Response: 200 { items: [ {id, slug, name, short_desc, icon_url, latest_version} ], pagination }

- GET /api/v1/catalog/{slug}
  - Description: Get catalog item details and versions.
  - Auth: optional
  - Response: 200 { id, slug, name, description, versions:[{version,manifest,release_date}], publisher }

- GET /api/v1/catalog/{slug}/versions/{version}/manifest
  - Description: Get package manifest for a specific version (files, deltas).
  - Auth: optional (private versions require auth)
  - Response: 200 manifest JSON

- POST /api/v1/publish
  - Description: Publisher upload new catalog item/version (multipart or presigned-upload flow).
  - Auth: Bearer (scope: publish)
  - Body: { metadata, version, changelog } -> returns upload URLs or accepts artifact
  - Response: 201 { item_id, version_id, status:"uploaded|processing" }

- GET /api/v1/catalog/{slug}/download?version={v}
  - Description: Returns presigned URL(s) or CDN manifest to download package.
  - Auth: optional or requires auth if protected
  - Response: 200 { downloads: [ {url, sha256, size} ], deltas: [...] }

---

# 3 — Assets (AGP_CMS)
- POST /api/v1/assets/upload
  - Description: Start an asset upload (returns presigned URLs or accept multipart)
  - Auth: Bearer (scope: publish)
  - Body: { filename, content_type, size, tags[] }
  - Response: 201 { asset_id, upload_url, finalize_url }

- POST /api/v1/assets/{asset_id}/finalize
  - Description: Finalize an upload and add metadata.
  - Auth: Bearer (scope: publish)
  - Body: { manifest, checksum }
  - Response: 200 { asset_id, url }

- GET /api/v1/assets/{asset_id}
  - Description: Asset metadata and CDN URL.
  - Auth: optional or required based on visibility
  - Response: 200 { asset_id, url, size, content_type, tags }

---

# 4 — Builds & CI (AGP_CMS)
- POST /api/v1/builds/request
  - Description: Request a build for a repository or template (hooks CI).
  - Auth: Bearer (scope: publish)
  - Body: { repo_url, commit_sha, platform_targets[], build_args? }
  - Response: 202 { build_id, status:"queued" }

- GET /api/v1/builds/{build_id}
  - Description: Get build status and artifact URLs.
  - Auth: Bearer (owner/publisher)
  - Response: 200 { build_id, status, logs_url, artifacts:[] }

- GET /api/v1/builds/{build_id}/artifacts/{artifact_id}/download
  - Description: Presigned URL for artifact download.
  - Auth: as above
  - Response: 200 { url }

---

# 5 — Launcher & Client Management (AGP_Studios backend or local runtime)
These endpoints can be implemented as local HTTP endpoints (localhost) within the desktop client for controlling local installs, sandbox runners and updater — or as part of a remote updater service.

- GET /api/v1/launcher/catalog
  - Description: Get remote or merged catalog (combines remote CMS and local dev entries).
  - Auth: optional
  - Response: 200 { items: [...] }

- POST /api/v1/launcher/install
  - Description: Start installing a package to a local path.
  - Auth: Bearer
  - Body: { catalog_slug, version, install_path?, options? }
  - Response: 202 { install_id, status:"in_progress" }

- GET /api/v1/launcher/install/{install_id}/status
  - Description: Check installation progress.
  - Auth: Bearer
  - Response: 200 { install_id, status, progress, message }

- POST /api/v1/launcher/uninstall
  - Description: Uninstall local package.
  - Auth: Bearer
  - Body: { install_id or catalog_slug }
  - Response: 200 { success:true }

- POST /api/v1/launcher/launch
  - Description: Launch a local game or server instance (with launch args).
  - Auth: Bearer
  - Body: { install_id, args[], env:{}, run_mode: "client"|"server"|"dedicated" }
  - Response: 200 { instance_id, pid, started_at }

- POST /api/v1/launcher/stop
  - Description: Stop a running local instance.
  - Auth: Bearer
  - Body: { instance_id }
  - Response: 200 { success:true }

- GET /api/v1/launcher/instances
  - Description: List running local instances.
  - Auth: Bearer
  - Response: 200 [ {instance_id, pid, status, path, started_at} ]

- GET /api/v1/launcher/updates/check
  - Description: Check for launcher update.
  - Auth: optional
  - Response: 200 { update_available: bool, version, notes, url }

- POST /api/v1/launcher/updates/apply
  - Description: Start applying launcher update (user-interactive).
  - Auth: Bearer
  - Response: 202 { update_id, status:"applying" }

---

# 6 — Game Server Control & Templates (AGP_GameServer)
- GET /api/v1/servers/templates
  - Description: List available server templates (hello-world, authoritative, relay-enabled).
  - Auth: optional
  - Response: 200 [ { id, name, description, repo_url, example_versions } ]

- POST /api/v1/servers/templates/{template_id}/instantiate
  - Description: Create a new server instance (scaffold) for local dev or remote instance.
  - Auth: Bearer (scope: server:create)
  - Body: { name, version, config: { port?, max_players?, env:{} }, destination: "local"|"cloud" }
  - Response: 201 { instance_id, files_url, status:"created" }

- GET /api/v1/servers/instances/{instance_id}
  - Description: Get instance metadata and state.
  - Auth: Bearer
  - Response: 200 { instance_id, state, host, port, started_at, config }

- POST /api/v1/servers/instances/{instance_id}/start
  - Description: Start a server instance (local or remote).
  - Auth: Bearer (scope: server:admin)
  - Response: 200 { instance_id, status:"starting" }

- POST /api/v1/servers/instances/{instance_id}/stop
  - Description: Stop the instance gracefully.
  - Auth: Bearer (scope: server:admin)
  - Response: 200 { instance_id, status:"stopping" }

- POST /api/v1/servers/instances/{instance_id}/exec
  - Description: Execute admin command on server (e.g., reload config, run maintenance).
  - Auth: Bearer (scope: server:admin)
  - Body: { command, args? }
  - Response: 200 { output, exit_code }

- GET /api/v1/servers/instances/{instance_id}/health
  - Description: Health check and metrics.
  - Auth: Bearer or public with token
  - Response: 200 { healthy:true, players_connected, memory_mb, cpu_percent }

- POST /api/v1/servers/instances/{instance_id}/upload-artifact
  - Description: Upload server build or patch to instance (presigned flow).
  - Auth: Bearer (scope: publish)
  - Response: 200 { upload_url }

---

# 7 — Sessions & Matchmaking (AGP_GameServer / AGP_CMS)
- POST /api/v1/matchmaking/queue
  - Description: Add player to matchmaking queue with filters.
  - Auth: Bearer
  - Body: { user_id, skill?, region?, game_mode?, metadata? }
  - Response: 202 { ticket_id, estimated_wait_secs }

- GET /api/v1/matchmaking/tickets/{ticket_id}
  - Description: Ticket status (matched or not).
  - Auth: Bearer
  - Response: 200 { ticket_id, state, match_id?, players[] }

- POST /api/v1/sessions
  - Description: Create a new session (host or cloud-backed).
  - Auth: Bearer
  - Body: { host_user_id, server_instance_id?, config:{}, max_players }
  - Response: 201 { session_id, join_token, server_addr }

- GET /api/v1/sessions/{session_id}
  - Description: Session info and join details.
  - Auth: Bearer
  - Response: 200 { session_id, players:[{id, display_name}], server_addr, state }

- POST /api/v1/sessions/{session_id}/join
  - Description: Join a session (returns connection details).
  - Auth: Bearer
  - Body: { user_id }
  - Response: 200 { connection: { protocol, url, token } }

---

# 8 — Social (AGP_CMS)
- POST /api/v1/friends/{user_id}/invite
  - Description: Invite user to friends.
  - Auth: Bearer
  - Response: 202 { invite_id }

- POST /api/v1/friends/{user_id}/accept
  - Description: Accept friend invite.
  - Auth: Bearer
  - Response: 200 { friendship_id }

- GET /api/v1/users/{user_id}/presence
  - Description: Get presence (online/offline, current session).
  - Auth: Bearer
  - Response: 200 { user_id, status:"online"|"offline", session_id?, last_seen }

- POST /api/v1/invites
  - Description: Send session invite with deep link.
  - Auth: Bearer
  - Body: { from_user_id, to_user_id, session_id, message? }
  - Response: 201 { invite_id, deep_link }

---

# 9 — AGP_AI (AGP_AI service) — conversational code assistant
- POST /api/v1/ai/generate-scaffold
  - Description: Generate a server+client scaffold from a natural language prompt.
  - Auth: Bearer (scope: ai:generate)
  - Body: { prompt, language:"csharp"|"python", template_id?, repo_target?:{owner,repo,branch?} }
  - Response: 200 { scaffold_id, summary, files:[{path, diff? or content_link}], estimated_time }

- POST /api/v1/ai/diff
  - Description: Ask AI to produce a patch/diff for provided files and a request.
  - Auth: Bearer (scope: ai:generate)
  - Body: { prompt, files:[{path,content}], apply_tests?:bool }
  - Response: 200 { diff_id, patch_url, preview_html }

- POST /api/v1/ai/apply-patch
  - Description: Apply patch to a repo branch (requires consent header).
  - Auth: Bearer (scope: ai:apply)
  - Headers: X-AGP-AI-Consent:true
  - Body: { patch_id, repo:{owner,repo,branch}, author:{name,email} }
  - Response: 200 { pr_url or commit_id, status:"applied" }

- POST /api/v1/ai/explain
  - Description: Explain code; returns natural language explanation.
  - Auth: Bearer
  - Body: { files:[{path,content}], question }
  - Response: 200 { explanation: string, references: [{path,line_range}] }

- GET /api/v1/ai/history
  - Description: List previous AI actions for auditing (read-only).
  - Auth: Bearer (scope: ai:read, admin)
  - Response: 200 [ { id, user_id, prompt_hash, result_summary, created_at } ]

Notes:
- Never auto-apply code without explicit user consent.
- All AI actions are logged for audit. Actions that modify code require patch preview and user approval.

---

# 10 — Realtime (SignalR/WebSocket/gRPC)
Recommended hubs (SignalR style) for in-client realtime features. Connection string: wss://realtime.agp.example.com/hubs/{hubName}?access_token=...

- /hubs/presence
  - Events: UserConnected(user), UserDisconnected(user), PresenceUpdate(user, status)

- /hubs/chat
  - Methods: SendMessage(sessionId, message)
  - Events: MessageReceived({from, message, timestamp}), TypingIndicator

- /hubs/session
  - Methods: SubscribeSession(sessionId), SessionUpdate(sessionId, state), PlayerJoined, PlayerLeft
  - Events: SessionStateChanged, PlayerList

- /hubs/matchmaking
  - Methods: JoinQueue(ticket), LeaveQueue(ticket)
  - Events: Matched(matchInfo), QueueUpdate(ticket, position)

Security:
- Validate JWT at connection handshake
- Enforce per-connection rate limits and message size caps

---

# 11 — Admin & Monitoring (admin scope)
- GET /api/v1/admin/health
  - Description: Service health (internal)
  - Auth: Bearer (scope: admin:read)
  - Response: 200 { services: {...}, uptime }

- GET /api/v1/admin/metrics
  - Description: Aggregated metrics (requires extra role)
  - Auth: Bearer (scope: admin:metrics)
  - Response: 200 prometheus/json metrics

- POST /api/v1/admin/force-uninstall
  - Description: Force uninstall an instance on a user's machine (use with extreme caution)
  - Auth: Bearer (scope: admin:action)
  - Body: { user_id, install_id, reason }
  - Response: 200 { result }

---

# 12 — Webhooks
- POST /webhooks/builds (subscription event)
  - Description: Build finished, payload includes build_id, status, artifacts
  - Auth: HMAC or secret header

- POST /webhooks/publish
  - Description: New catalog item published
  - Auth: HMAC

- POST /webhooks/ai-action
  - Description: AI applied patch or proposed change (for auditing systems)

---

# 13 — Suggested Scopes
- openid profile email
- agp:publish
- agp:server:admin
- agp:server:create
- agp:matchmaking
- agp:ai:generate
- agp:ai:apply
- agp:admin
- agp:metrics

---

# 14 — Mapping to repositories
- AGP_CMS: auth, users, catalog, assets, builds, marketplace, webhooks, admin
- AGP_GameServer: server templates, instantiate/start/stop, health, session endpoints, matchmaking back-end
- AGP_Studios (client & local runtime): launcher/local endpoints, local instance management, in-client AI UI, SignalR clients
- AGP_AI: ai/generate-scaffold, ai/diff, ai/apply-patch, ai/explain, ai/history
- Realtime host (could be in CMS or separate): SignalR hubs for presence/chat/session/matchmaking

---

# 15 — OpenAPI / Implementation notes
- Start with OpenAPI v3 for all HTTP endpoints and generate SDKs for C# (AGP_Studios), TypeScript (web UI), and Python (tools).
- Version endpoints with /api/v1 and use semantic versioning for releases.
- Provide a minimal mock server (wiremock or local stub) to enable offline development for the launcher.

---

# Appendix — Minimal sample JSON shapes

User (profile)
```json
{
  "id":"uuid",
  "username":"player1",
  "email":"player1@example.com",
  "display_name":"Player One",
  "avatar_url":"https://cdn.example.com/avatars/uuid.png",
  "roles":["user"],
  "created_at":"2025-01-01T12:00:00Z"
}
```

Catalog item (summary)
```json
{
  "id":"uuid",
  "slug":"my-game",
  "name":"My Game",
  "short_desc":"A sample game",
  "icon_url":"https://cdn.example.com/icons/my-game.png",
  "latest_version": "1.2.0"
}
```

Session join response
```json
{
  "session_id":"uuid",
  "connection": {
    "protocol":"websocket",
    "url":"wss://gameserver.example.com/ws",
    "token":"short-lived-jwt"
  },
  "expires_at":"2025-11-09T17:05:19Z"
}
```
