# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project state

This is an early-stage ASP.NET Core Web API (net10.0), currently still mostly the default project template. It is **not** a git repository yet (no `.git` directory) — if git operations are needed, confirm with the user first (e.g. offer `git init`).

The intended purpose (per naming and `.env.sample`) is a microservice for sending email via SMTP (Brevo). As of now:
- `Services/EmailService.cs` — empty stub class, no members.
- `Utilities/EnvironmentVaraibles.cs` — empty stub class (note the typo in the filename/class name — preserve it unless asked to rename, since renaming affects the public type name).
- `Program.cs` — unmodified template startup; `DotNetEnv` is referenced in the `.csproj` but `Env.Load()` is never called, so `.env` is not actually loaded at runtime yet.
- `Controllers/WeatherForecastController.cs` and `WeatherForecast.cs` — leftover template sample code, not yet replaced with real endpoints.

When implementing email functionality, wire up `DotNetEnv.Env.Load()` in `Program.cs` before `builder.Build()` and read SMTP settings via `Environment.GetEnvironmentVariable`, matching the keys in `.env.sample` (`SMTP_HOST`, `SMTP_PORT`, `SMTP_LOGIN`, `SMTP_KEY`).

## Commands

Run all commands from the repo root or `EmailMicroService/` (the single project referenced by `EmailMicroService.slnx`).

```
dotnet restore
dotnet build
dotnet run --project EmailMicroService        # runs on http://localhost:5238 (see launchSettings.json)
```

There are no automated tests in this repo yet.

`EmailMicroService/EmailMicroService.http` contains sample HTTP requests usable with the VS Code/Rider REST client against the running dev server.

### Docker

`EmailMicroService/Dockerfile` is a standard multi-stage build (SDK build → aspnet runtime). Build context must be the repo root (it does `COPY ["EmailMicroService/EmailMicroService.csproj", "EmailMicroService/"]` then `COPY . .`), e.g.:

```
docker build -f EmailMicroService/Dockerfile -t email-microservice .
```

## Configuration

- Secrets/config live in `EmailMicroService/.env` (gitignored); `EmailMicroService/.env.sample` documents the required keys (SMTP host/port/login/key for Brevo).
- `appsettings.json` / `appsettings.Development.json` only contain default logging config — no custom config sections yet.
- `UserSecretsId` is set in the `.csproj`, so `dotnet user-secrets` is also available for local secrets if preferred over `.env`.
