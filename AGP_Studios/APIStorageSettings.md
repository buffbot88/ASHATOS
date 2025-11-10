# AGP_Studios — Per-User Hosted Folder / Storage Integration (API extensions)

Summary
- You said each user will host their own folder(s) (their own accounts). This file updates the API design to support user-hosted storage folders and connectors so AGP_Studios can read/write, publish, and manage artifacts in each user's storage location while preserving security, auditing and UX parity with a central CMS-backed flow.
- Below are recommended endpoints, auth notes, canonical flows (connect, upload, publish, sync), supported connector types, and security/operational notes.

Design goals
- Support any user-hosted storage: S3-compatible, WebDAV, SFTP, SMB, HTTPS file server, or a local folder exposed by the client agent.
- Keep publisher ownership explicit: artifacts live under the user's storage root and are only published to the public AGP catalog when the user initiates publish.
- Use short-lived credentials/presigned URLs and scoped tokens for safe direct upload/download.
- Provide a discovery & test-connection flow to let users add storage providers and verify connectivity.
- Allow AGP_AI and server templates to read/write only with explicit user consent; actions that modify user storage must be auditable and require consent tokens.

Conventions
- Base path: /api/v1 (same as existing)
- Storage paths are scoped to the authenticated user: use /api/v1/users/{user_id}/storage/* or /api/v1/storage/providers for provider management.
- Auth: Bearer JWT; provider secrets stored encrypted server-side OR use OAuth flows / short-lived credentials / presigned URLs.
- Idempotency and uploads: idempotency-key for upload-init/publish operations.
- Audit: every write/publish action logged with actor, timestamp, and consent token when coming from AGP_AI.

Supported connector types
- s3 (S3-compatible with access_key/secret + region + bucket + optional prefix)
- webdav (url + username/password or OAuth)
- sftp (host/port/username + method:password|key)
- smb (server/share + credentials)
- http(s) direct (server provides presigned uploads or direct POST)
- local-agent (AGP_Studios local file agent: exposes user's local path through a local HTTPS endpoint; recommended for private/non-networked storage)

Endpoints (new / updated)

1) Storage Providers & Connections
- GET /api/v1/storage/providers
  - Description: List supported provider types and connection schemas (client uses this to render connect UI).
  - Auth: optional
  - Response: 200 [{ type:"s3"|"webdav"|"sftp"|"smb"|"http"|"local-agent", display_name, fields:[{name, type, label, required}] }]

- POST /api/v1/users/{user_id}/storage/connect
  - Description: Create a storage connection for the user (saves encrypted connection metadata server-side or initiates OAuth flow).
  - Auth: Bearer (user = user_id)
  - Body: { provider_type, credentials_or_oauth_redirect? , display_name, root_path? }
  - Response: 201 { connection_id, status:"connected"|"pending_oauth", test_result? }

- POST /api/v1/users/{user_id}/storage/{connection_id}/test
  - Description: Test connectivity and permissions (read/list/write) for the given connection.
  - Auth: Bearer
  - Body: optional { test_path }
  - Response: 200 { connection_id, ok:true/false, details: { can_read, can_write, free_space_bytes? } }

- GET /api/v1/users/{user_id}/storage/connections
  - Description: List the user's configured storage connections (metadata only).
  - Auth: Bearer
  - Response: 200 [{ connection_id, provider_type, display_name, root_path, created_at, last_tested }]

- DELETE /api/v1/users/{user_id}/storage/{connection_id}
  - Description: Remove connection (revokes secrets stored, does NOT delete remote files).
  - Auth: Bearer
  - Response: 204

2) File Listing / Read / Metadata
- GET /api/v1/users/{user_id}/storage/{connection_id}/list?path=/some/path&page=&per_page=
  - Description: List files and folders under `path` (normalized under the connection's root_path).
  - Auth: Bearer
  - Response: 200 { items:[{ name, path, is_dir, size, modified_at, id }], pagination }

- GET /api/v1/users/{user_id}/storage/{connection_id}/meta?path=/some/file
  - Description: Get file metadata.
  - Auth: Bearer
  - Response: 200 { id, path, size, content_type, checksum, url? }

- GET /api/v1/users/{user_id}/storage/{connection_id}/download?path=/some/file
  - Description: Return either a proxied download stream or presigned URL for the client to fetch.
  - Auth: Bearer
  - Response: 200 { url, expires_at } or 200 stream

3) Upload flow (two options: presigned direct upload or proxied upload)
- POST /api/v1/users/{user_id}/storage/{connection_id}/upload-init
  - Description: Initialize an upload for a given path; returns presigned URLs or an upload session id.
  - Auth: Bearer
  - Body: { path, filename, content_type, size, multipart?:bool }
  - Response: 201 { upload_id, presigned_urls:[{part, url}], finalize_url, expires_at }

- POST /api/v1/users/{user_id}/storage/{connection_id}/upload-finalize
  - Description: Finalize multipart or presigned upload; validate checksum and publish metadata.
  - Auth: Bearer
  - Body: { upload_id, checksum, metadata? }
  - Response: 200 { file_id, path, size, url }

- POST /api/v1/users/{user_id}/storage/{connection_id}/upload-proxy
  - Description: Upload via AGP_CMS proxy (for providers where presigned not possible) — client streams to CMS which writes to provider.
  - Auth: Bearer
  - Response: 201 { file_id, path, url }

4) File operations (copy/move/delete/share)
- POST /api/v1/users/{user_id}/storage/{connection_id}/copy
  - Body: { src_path, dest_path, overwrite?:bool }
  - Response: 200 { success:true, dest_path }

- POST /api/v1/users/{user_id}/storage/{connection_id}/move
  - Body: { src_path, dest_path, overwrite?:bool }
  - Response: 200 { success:true, dest_path }

- DELETE /api/v1/users/{user_id}/storage/{connection_id}/file?path=/some/path
  - Description: Delete file/folder
  - Auth: Bearer
  - Response: 204

- POST /api/v1/users/{user_id}/storage/{connection_id}/share
  - Description: Create a time-limited public or secret share link for a file/folder.
  - Body: { path, max_age_seconds, read_only?:bool, one_time?:bool, allowed_ips?:[] }
  - Response: 201 { share_id, url, expires_at }

5) Usage / Quotas / Health
- GET /api/v1/users/{user_id}/storage/{connection_id}/usage
  - Description: Storage usage and quota (if provider reports).
  - Auth: Bearer
  - Response: 200 { used_bytes, free_bytes?, quota_bytes?, reported_by_provider?:bool }

- GET /api/v1/users/{user_id}/storage/{connection_id}/health
  - Description: Provider health (last test, latency, last_failure)
  - Auth: Bearer/admin
  - Response: 200 { ok:true, last_tested_at, last_error? }

6) Publish flow (user publishes artifacts from their storage into AGP catalog)
- POST /api/v1/users/{user_id}/publish/from-storage
  - Description: Request publish where source artifact lives in a user storage path. Server will fetch/ingest artifact, verify checksum, and create a catalog item/version.
  - Auth: Bearer (scope: agp:publish)
  - Body: { connection_id, path, metadata:{name, version, changelog, visibility:public|private}, manifest_path? , sign_manifests?:bool }
  - Response: 202 { publish_id, status:"queued", ingest_tasks:[{task, status}] }
  - Notes: Publishing requires the user to confirm ownership and indicate license/visibility. For large files server should use presigned fetch or provider APIs to copy without proxying bytes through AGP where possible.

- GET /api/v1/publish/{publish_id}
  - Description: Publish status (ingestion, validation, scan results, completion)
  - Response: 200 { publish_id, status, errors, catalog_item_id? }

7) Agent-assisted local storage (local-agent)
- POST /api/v1/users/{user_id}/storage/local-agent/register
  - Description: Register a local AGP_Studios agent instance (the client exposes a local HTTPS server or socket the backend can request from).
  - Auth: Bearer
  - Body: { agent_id, capabilities:[ "file-proxy","mount" ], local_url:"https://127.0.0.1:PORT" }
  - Response: 201 { agent_id, status }

- POST /api/v1/users/{user_id}/storage/local-agent/command
  - Description: Request local agent to perform file operations that should not be proxied through central servers (e.g., read private keys, run local scripts, mount).
  - Auth: Bearer (user must have consented)
  - Body: { agent_id, command:"list|read|write|stream", path, params }
  - Response: proxied stream or JSON. Local agent must require a user interaction/trust on first use.

8) AGP_AI & storage interactions (explicit consent)
- When AGP_AI needs to read/write files in the user's storage, require an AI consent header: X-AGP-AI-Consent: true and record the consent token reference.
- POST /api/v1/users/{user_id}/storage/{connection_id}/ai/preview-patch
  - Body: { files:[{path,content}], patch:diffOrInstructions, ai_action_id }
  - Response: 200 { preview:[{path, original, modified}], audit_id }

- POST /api/v1/users/{user_id}/storage/{connection_id}/ai/apply-patch
  - Description: Apply an AI-generated patch to user's storage (requires user consent & an idempotent token).
  - Headers: X-AGP-AI-Consent: true
  - Body: { ai_action_id, audit_comment, allow_backup?:true }
  - Response: 200 { result:"applied"|"partial"|"failed", changes:[...] }

Operational & security notes
- Use short-lived tokens and presigned URLs for reads/writes. Do not store plaintext provider secrets — keep them encrypted with per-tenant keys.
- Prefer server-side provider COPY operations (S3->S3) to avoid relaying large artifacts through AGP when both source and destination support it.
- Virus/malware scanning: any published artifact must be scanned before being listed as downloadable in catalog.
- Quotas and rate-limiting: enforce per-user rate limits and per-connection concurrency (to avoid DoS).
- Audit & versioning: keep file-level audit records and optional backups before destructive operations (delete/overwrite) to allow user rollback.
- CORS & browser flows: for Web UI flows, support CORS-safe presigned URLs and OAuth redirect flows.
- Provider lifecycle: if a user deletes a connection, do NOT delete artifacts from provider (document clearly). Provide a "forget and optionally purge" flow.
- Disclosure & privacy: clearly show users what AGP reads or uploads on first connect; require explicit consent for AGP_AI to operate.

Example UX flows (concise)

A) Connect storage provider (S3)
1. Client requests GET /api/v1/storage/providers and displays "S3".
2. User enters credentials or starts OAuth. Client POSTs /users/{id}/storage/connect.
3. Server stores encrypted metadata and POST /users/{id}/storage/{conn}/test returns can_read/can_write.
4. Client shows the connected folder in library settings.

B) Publish a build from user storage
1. User uploads build to /user-root/builds/mygame-1.0.zip (via presigned upload returned by upload-init).
2. User clicks "Publish" in launcher, client calls POST /api/v1/users/{id}/publish/from-storage with connection_id + path.
3. Server ingests artifact (via direct provider API if available), scans artifact, creates catalog entry, returns publish_id.
4. User confirms metadata and completes publish.

C) AGP_AI patching a project inside user's storage
1. User selects a folder and requests "AI: add matchmaking".
2. Client calls POST /api/v1/ai/diff or forward to /users/{id}/storage/.../ai/preview-patch with files reference.
3. AI returns preview; client shows diff. If user accepts, client sends POST /.../ai/apply-patch with X-AGP-AI-Consent:true.
4. Apply operation is logged and archived.

Mapping to repos
- AGP_CMS: keeper of user connections metadata, publish ingestion, presigned URL orchestration, scanning, and central audit logs.
- AGP_Studios: local agent, UI for connecting providers, mount local-agent, initiate uploads/publish, call storage endpoints.
- AGP_GameServer: when templates want to read/write artifacts into user storage for builds, they use the same provider connection metadata or short-lived tokens orchestrated by AGP_CMS.
- AGP_AI: uses the ai/preview-patch and ai/apply-patch storage endpoints; always requires recorded consent.

What's changed (summary)
- Introduced a new user-scoped storage/connect API surface and upload/publish flows.
- Added local-agent commands to support purely local-hosted folders without exposing credentials to the cloud.
- Added AI-specific consented endpoints for patching user storage.
- Presigned/pull-copy-first approach recommended to avoid central bandwidth bottlenecks.

Next step I can take for you
- Add these endpoints to the existing OpenAPI stub (openapi_stub.yaml) and split them into AGP_CMS and AGP_Studios API files so SDKs can be generated. I can generate the OpenAPI YAML additions next so you can run codegen for C# client libraries.
