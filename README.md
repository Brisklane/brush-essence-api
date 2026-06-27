# BrushEssence API

.NET 9 Web API for the Brush Essence oil-painting store, built with **Clean
Architecture** and **PostgreSQL** (EF Core). The companion storefront/admin UI is
the [Next.js app](../brush-essence-ui).

```
BrushEssence.Domain  →  Application  →  Infrastructure  →  Api
 (entities, rules)     (use cases,     (EF Core, repos,    (controllers,
                        DTOs, services)  storage, identity)  middleware, DI)
```

- **Domain** — entities, enums, status workflows. No dependencies.
- **Application** — use-case services, DTOs, FluentValidation, repository
  *interfaces*. Depends only on Domain.
- **Infrastructure** — EF Core `DbContext`, repositories, file storage, JWT/BCrypt.
- **Api** — controllers, middleware (exception handling, security headers),
  auth, rate limiting, output caching, health checks, Swagger.

## Features

Catalogue (paintings, **categories**, **mediums**), **cart** (guest via
`X-Cart-Token` + signed-in, merged on login), **orders** (checkout, history,
status workflow & tracking), **custom painting requests** (commissions with
reference-image uploads), **reviews & ratings** (moderated, cached averages), and
an **admin dashboard** (user / order / request / review management + analytics
and reports). The store is **single-currency (PKR)**, set server-side.

## Prerequisites

- .NET 9 SDK
- PostgreSQL (local dev DB defaults to `brushessence`, user `postgres`)
- EF Core tools: `dotnet tool install --global dotnet-ef`

## Configuration

Settings live in `src/BrushEssence.Api/appsettings.json` and
`appsettings.Development.json`. Local dev values (DB connection, JWT signing key)
are committed for convenience. **In production, supply secrets via environment
variables** (they override appsettings):

- `ConnectionStrings__DefaultConnection`
- `Jwt__Secret` (must be a long, random value — at least 32 chars)

Other notable config sections (all have sensible defaults):

- `Cors:AllowedOrigins` — origins allowed to call the API (the UI URL).
- `RateLimiting:Global` / `RateLimiting:Auth` — request limits per client IP
  (`PermitLimit`, `WindowSeconds`, `SegmentsPerWindow`).
- `ForwardedHeaders:Enabled` — set to `true` **only when running behind a trusted
  reverse proxy** so the real client IP is used for rate limiting (otherwise off,
  to prevent IP spoofing).
- `FileStorage` — upload size/type limits (images are stored in PostgreSQL).

## Running

```bash
dotnet run --project src/BrushEssence.Api
```

- API: http://localhost:5150
- Swagger UI (Development): http://localhost:5150/swagger — use the **Authorize**
  button to paste a JWT access token and call protected endpoints.
- Health: `/health`, `/health/ready` (DB), `/health/live` (structured JSON).

## Production readiness

Built-in ASP.NET Core features (no extra packages):

- **Output caching** — public catalogue reads are cached ~60s for anonymous
  callers and evicted when an admin writes (`api/paintings`, `api/categories`,
  `api/mediums`).
- **Rate limiting** — a global limiter (default 100/min per IP) plus a stricter,
  sliding-window `auth` policy (default 10/min) on `api/auth/*`. Configurable via
  the `RateLimiting` section; returns `429` + `Retry-After`.
- **Security headers** — CSP, `X-Frame-Options`, `X-Content-Type-Options`,
  `Referrer-Policy`, `Permissions-Policy`, plus HSTS in production.
- **Centralized errors** — all errors return RFC 7807 `ProblemDetails` with a
  `traceId` (see `GlobalExceptionHandler`).
- **Structured logging** — Serilog with a JSON file sink (`logs/`).
- **Audit logging** — order status changes, review moderation, and user/admin
  actions are logged with the acting user (`IAuditLogger`).

## Tests

```bash
dotnet test
```

## Database migrations (EF Core)

Migrations live in `src/BrushEssence.Infrastructure/Persistence/Migrations`.
The `Infrastructure` project holds the `DbContext`; the `Api` project is the
startup project (it provides configuration/DI at design time).

**Add a migration:**

```bash
dotnet ef migrations add <Name> \
  --project src/BrushEssence.Infrastructure \
  --startup-project src/BrushEssence.Api \
  --output-dir Persistence/Migrations
```

**Apply migrations to the database:**

```bash
dotnet ef database update \
  --project src/BrushEssence.Infrastructure \
  --startup-project src/BrushEssence.Api
```

**Other useful commands:**

```bash
# Remove the last (unapplied) migration
dotnet ef migrations remove --project src/BrushEssence.Infrastructure --startup-project src/BrushEssence.Api

# Roll back to a specific migration
dotnet ef database update <PreviousMigrationName> --project src/BrushEssence.Infrastructure --startup-project src/BrushEssence.Api

# Generate an idempotent SQL script (for production deploys)
dotnet ef migrations script --idempotent --project src/BrushEssence.Infrastructure --startup-project src/BrushEssence.Api
```

> Set `ASPNETCORE_ENVIRONMENT=Development` when running EF commands locally so the
> dev connection string is picked up.

## Authentication & authorization

Custom JWT auth (no ASP.NET Identity):

- **Endpoints** (`/api/auth`): `register`, `login`, `refresh`, `logout`,
  `forgot-password`, `reset-password`, and `GET me` (requires a bearer token).
- **Passwords** are hashed with BCrypt (work factor 12).
- **Access tokens** are short-lived JWTs (15 min); **refresh tokens** are stored
  hashed (SHA-256) in the DB and rotated on every use.
- **Roles** (`Admin`, `Customer`) are seeded by migration. New registrations get
  the `Customer` role. The `AdminOnly` policy guards `/api/admin/*`.

### Promoting a user to Admin

No admin user is seeded (for safety). To grant the Admin role to an existing user:

```sql
INSERT INTO user_roles ("UserId", "RoleId")
SELECT u."Id", r."Id"
FROM users u, roles r
WHERE u."Email" = 'you@example.com' AND r."Name" = 'Admin';
```

### Email (forgot password)

No email provider is configured yet. `forgot-password` logs the reset token via
Serilog (look for `Reset token (DEV ONLY)` in the console/log file). Wire up an
email sender before production use.
