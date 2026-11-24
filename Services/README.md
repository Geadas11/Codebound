# Codebound Services — accounts, friends, sessions, stats, cloud saves

The online backend for Codebound. **The game never needs it to play locally**: worlds, saves, building, programming and
survival all work offline; this service only adds accounts and community features. Why this design (and the
alternatives compared) is in `../ONLINE_PLAN.md`.

- ASP.NET Core 8 minimal API, one dependency (`Microsoft.Data.Sqlite`), SQLite in WAL mode.
- Passwords: PBKDF2-SHA256, 600 000 iterations, per-user salt. Tokens: random 256-bit, stored only as SHA-256 hashes.
- Access tokens last 15 minutes; refresh tokens 30 days, rotate on every use, and a reused one revokes that device session.

## Run it (development)
Needs the .NET 8 SDK (the Unity Editor ships one: `<Unity>/Editor/Data/DotNetSdk/dotnet.exe`).
```
cd Services
dotnet run --project Codebound.Services --environment Development
```
It listens on `http://127.0.0.1:5080`. In Development, emails are **not sent**: they are kept in memory and readable at
`http://127.0.0.1:5080/dev/outbox` (only from this machine) — that is where the verification codes appear.

Tests (start the real service in-process with a temporary database and a movable clock):
```
dotnet test
```

## Configuration
`appsettings.json` (production defaults, no secrets), `appsettings.Development.json`, then environment variables, which
win: `CODEBOUND__<Setting>` (double underscore for sections).

| Setting | Default | Notes |
|---|---|---|
| `Codebound__Database` | `codebound.db` | SQLite file path |
| `Codebound__RequireHttps` | `true` | Refuses plain HTTP (behind a TLS proxy the proxy must send `X-Forwarded-Proto`) |
| `Codebound__TrustedProxies__0` | — | IP of the reverse proxy whose `X-Forwarded-For/Proto` are believed |
| `Codebound__Smtp__Host` / `__Port` / `__User` / `__Password` / `__From` | — | Real email (any SMTP provider). **Password only from the environment** |
| `Codebound__DevOutbox` | `false` | `true` only in development |
| `Codebound__PasswordIterations` | `600000` | |
| `Codebound__GlobalRequestsPerMinute` / `AuthRequestsPerMinute` / `SearchRequestsPerMinute` | 240 / 20 / 30 | Rate limits (per IP; search per account) |
| `Codebound__RetentionDays` | `30` | How long a deleted account's bare row and audit trail stay (abuse handling) |
| `Urls` | `http://127.0.0.1:5080` | Keep it on localhost and put the TLS proxy in front |

Nothing secret is in the repository or in the Unity build. The game only knows the service's public URL.

## Production
1. A small Linux VPS (≈ 4–6 €/month) with the .NET 8 ASP.NET Core runtime, `dotnet publish -c Release`.
2. A TLS reverse proxy in front, e.g. Caddy (free, automatic Let's Encrypt certificates):
   `codebound.example.com { reverse_proxy 127.0.0.1:5080 }`.
3. Environment: `ASPNETCORE_ENVIRONMENT=Production`, `Codebound__TrustedProxies__0=127.0.0.1`, SMTP settings (an SMTP
   provider with a free tier, e.g. a transactional email service, or the domain's mail host — choose and document the cost).
4. Run as a systemd service with `Restart=always`.

### Migrations
Numbered SQL scripts in `Migrations.cs`, applied in order at start-up, each in a transaction, recorded in `schema_version`.
Never edit a published migration — add the next one.

### Logs and monitoring
- One line per request (method, path, status, time) — never bodies, tokens, passwords or emails. Production logs are
  JSON lines (for journald / any collector). Unhandled errors are logged with their stack and answered with a generic 500.
- `GET /health` → `{ status, version, time }` for an uptime monitor (e.g. a free external pinger).
- Audit table: logins, failed logins, password and email changes, refresh-token reuse, deletions.

### Backups
`dotnet Codebound.Services.dll backup /backups/codebound-$(date +%F).db` makes an online copy (safe while running). Run
it daily from cron, keep 14 days, copy off the server. Restore = stop the service, replace the database file, start.

### Data and privacy
- Emails are private: never shown to other players; "register" and "forgot password" never reveal whether an email has an account.
- Account deletion (password + typing DELETE) removes email, password, profile details, friends, blocks, invites,
  statistics, achievements and cloud saves at once; the bare row and its audit trail are removed after `RetentionDays`.
- `GET /me/export` gives the player everything kept about them. Statistics are kept only with consent (`statsConsent`);
  withdrawing it deletes them.

## What is not here (yet)
- Push notifications: the game polls `/me/notifications`.
- A relay for players behind strict NAT: games still need the host's port open (UPnP in the game, or port forwarding).
- Host migration: the world lives on the host's PC; if the host leaves, the game ends for everyone.
