# Deployment Guide

## Prerequisites

- .NET 10 SDK to build; .NET 10 ASP.NET Core runtime to host.
- SQL Server. LocalDB is supported for Windows development only.
- `sqlcmd` or SQL Server Management Studio for schema initialization.
- Node.js 22.12+ and npm to build/run the Angular client.
- Writable persistent storage if report files must survive restarts.

## Configuration

ASP.NET Core loads `appsettings.json`, then `appsettings.{Environment}.json`; environment variables override JSON. Nested environment keys use double underscores, such as `ConnectionStrings__DefaultConnection` and `Jwt__Secret`.

The shared settings are in `appsettings.json`; local database/JWT/threshold/job/file-log values are in `appsettings.Development.json`. `appsettings.Production.json` contains literal `##ReplacementTokens##`. ASP.NET Core does **not** substitute these strings automatically.

For Azure, choose one configuration method:

1. Set `ASPNETCORE_ENVIRONMENT=Production` and configure a JSON-aware pipeline replacement step for the `##` token delimiters before starting the API; or
2. Set Azure App Service application settings or Key Vault references to override the corresponding configuration keys. This is preferable for secrets.

Never commit a transformed production file or print secret values in deployment logs. Replace every token you rely on. Numeric/boolean settings must become valid numeric/boolean JSON values in a transformed file; environment overrides should also have correctly typed values.

Configuration keys:

- `ConnectionStrings__DefaultConnection`: SQL Server used by application data and Hangfire storage.
- `Jwt__Secret`: HS256 signing key; random and at least 32 UTF-8 bytes.
- `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpirationMinutes`: JWT issuer, audience, and expiry.
- `MonitoringThresholds__CpuUsage`: CPU alert threshold; default 80.
- `MonitoringThresholds__MemoryUsage`: memory alert threshold; default 90.
- `MonitoringThresholds__DiskUsage`: disk alert threshold; default 90.
- `Jobs__MetricsCollectionIntervalMinutes`: Hangfire minute-step cron setting. Use a positive step below 60; it aligns to wall-clock minute boundaries rather than exact elapsed time.
- `Reports__Directory`: directory used for generated CSV files.
- `FileLogging__Enabled`: enable/disable daily JSON file output; console output remains enabled.
- `FileLogging__Path`: file path pattern, e.g. `logs/monitoring-.json`.
- `FileLogging__RetainedFileCountLimit`: maximum retained rolling files; default 14.
- `FileLogging__FileSizeLimitBytes`: maximum file size before size-based rolling; default 10 MiB.
- `AllowedHosts`: ASP.NET Core host filtering.

The Development JWT key is for local use only. Cryptographic validation still requires valid JWT settings even though custom startup checks were removed.

## Database Initialization

Initialize SQL Server using the schema script:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i database/schema.sql
```

The script creates application tables, constraints, indexes, seed roles/permissions, and a demonstration server. It migrates legacy `UserRoles` assignments when that table exists. Hangfire creates its own SQL objects when the API starts. EF migration automation is not configured; apply future database changes using reviewed SQL scripts.

### Local Demo Administrator

For a local assessment demonstration only, run this after `schema.sql`:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i database/seed-demo-admin.sql
```

The script comments contain the public `demo.admin` username and password. It inserts a precomputed ASP.NET Identity-compatible hash only if the username/email are unused; it never resets an account. **Never run the demo seed in Production and remove the demo account before deployment.**

For Production, register an account using a private password and promote it with `database/promote-admin.sql`. Public registration assigns User only.

## Build and Test

```powershell
dotnet restore MonitoringDashboard.sln
dotnet build MonitoringDashboard.sln --configuration Release
dotnet test MonitoringDashboard.sln --configuration Release
```

Frontend:

```powershell
Set-Location frontend
npm install --legacy-peer-deps
npm run build
npx ng test --watch=false
```

## Local Run

Start the WebAPI using its HTTPS profile:

```powershell
dotnet run --project src/MonitoringDashboard.WebAPI --launch-profile https
```

The HTTPS endpoint is `https://localhost:7179`; `http://localhost:5234` redirects to HTTPS. Swagger UI is `/swagger`, OpenAPI JSON is `/openapi/v1.json`, and Hangfire dashboard is `/hangfire`. Current `Program.cs` maps these routes in every environment. Before Production deployment, make API documentation exposure an explicit choice and add authenticated/authorized Hangfire dashboard access; its default authorization is local-only.

In a second terminal:

```powershell
Set-Location frontend
npm start
```

Open `http://localhost:4200`. The Angular development proxy sends `/api` and `/hubs` to `https://localhost:7179`. Certificate verification is disabled in that local proxy only. Restart Angular after changing `frontend/src/proxy.conf.json`.

## Azure App Service Outline

1. Provision SQL Server and apply `database/schema.sql` as a release step.
2. Publish `src/MonitoringDashboard.WebAPI` for .NET 10 and deploy the output.
3. Set `ASPNETCORE_ENVIRONMENT=Production`.
4. Configure connection string, JWT settings, and other values through App Service settings or Key Vault references; alternatively replace every required JSON token before startup.
5. Deploy the Angular production build to a static host or serve it through an appropriate web host. The checked-in Angular proxy is for local development; configure the production API base/proxy separately.
6. Configure HTTPS/TLS, health probing, database backups, secret storage/rotation, and centralized log collection.
7. Gate Swagger/OpenAPI as appropriate for your environment. Do not expose Hangfire's built-in dashboard remotely until explicit authentication/authorization is configured; its default authorization is local-only.

## Logging

Serilog handles existing `ILogger<T>` calls. Console output is configured for centralized collection. Optional file logging writes structured JSON to daily files such as `monitoring-20261011.json`; files also roll at the configured size limit. `RetainedFileCountLimit` counts files, not calendar days. File sink changes require an API restart.

Development enables file logging at `logs/monitoring-.json`. On Azure, select a persistent writable path if file logging is enabled; ephemeral App Service storage may be cleared. With multiple replicas, prefer centralized console logs or unique per-instance file paths. Restrict log access and never log credentials, bearer tokens, or sensitive request bodies. Generated `logs/` folders are ignored by Git.

## Production and Scaling Notes

- Use least-privilege SQL credentials and encrypted connections.
- Configure trusted forwarded headers before relying on client-IP rate limiting behind a reverse proxy.
- Add a SignalR backplane before running multiple API instances.
- Use shared durable report storage across instances; local report files are not shared.
- Role updates use serializable transactions for last-administrator protection; monitor deadlocks during concurrent role changes.
- `Program.cs` currently maps Swagger and Hangfire routes without an environment guard. Hangfire's built-in dashboard authorization is local-only by default; configure and verify remote authorization before exposure.
- Metrics are simulated. Delayed checks read database status and do not ping machines. The report-ready continuation logs a simulated notification, not email.

## Troubleshooting

Common symptoms and checks:

- Login returns 307: use the HTTPS API profile and restart Angular so its proxy targets `https://localhost:7179`.
- SQL connection failure: verify `ConnectionStrings__DefaultConnection`, SQL availability, and schema setup.
- JWT authentication fails: check secret length, issuer/audience, expiry, and unreplaced production tokens.
- No metric history: confirm Hangfire started and the recurring job is scheduled. Samples are simulated.
- Report remains Pending/Processing: check Hangfire worker logs and write permission for `Reports__Directory`.
- Download fails after restart/scale-out: ensure report storage is persistent/shared and the stored `FilePath` is reachable.
- Hangfire dashboard denies remote access: default authorization is local-only. Add an authenticated dashboard authorization filter; do not expose anonymously.
- File logs are missing: check `FileLogging__Enabled`, path writability, retention/size settings, and restart after configuration changes. Console logs remain available.

## Not Implemented

Refresh-token rotation, real email delivery, alert acknowledgement, formal EF migrations, SQL Server concurrency integration tests, WebApplicationFactory API integration tests, shared report storage implementation, SignalR backplane, Docker, CI/CD, and cloud infrastructure-as-code are not included.
