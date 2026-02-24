Plan: Currency Converter Platform

TL;DR — Build an ASP.NET Core 8 Web API (backend) and a React + TypeScript + Vite SPA (frontend). Backend will source rates from Frankfurter, expose latest/conversion/historical (paginated) endpoints, implement caching, retries with exponential backoff, a circuit breaker, JWT auth + RBAC, rate limiting, structured logging (Serilog) and request correlation. Frontend consumes the API, shows conversion UI, latest rates, and paginated historical rates. I will scaffold the project structure, services, provider factory, tests, Dockerfiles, and CI-ready workflows so you can iterate quickly.

Steps
1. Create backend project skeleton and DI wiring
   - Add project src/CurrencyConverter.Api/CurrencyConverter.Api.csproj
   - Bootstrap host and services in src/CurrencyConverter.Api/Program.cs
   - Centralize registrations in src/CurrencyConverter.Api/StartupExtensions.cs
   - Configure appsettings in src/CurrencyConverter.Api/appsettings.json
2. Implement core API surface
   - Controllers:
     - src/CurrencyConverter.Api/Controllers/CurrencyController.cs — CurrencyController (latest, convert, historical endpoints + pagination)
     - src/CurrencyConverter.Api/Controllers/AuthController.cs — AuthController (login, token refresh)
   - Models:
     - src/CurrencyConverter.Api/Models/CurrencyConversionRequest.cs
     - src/CurrencyConverter.Api/Models/CurrencyConversionResponse.cs
     - Error model for excluded currencies
3. Exchange-rate provider abstraction and Frankfurter provider
   - Interface: src/CurrencyConverter.Api/Providers/IExchangeRateProvider.cs — IExchangeRateProvider
   - Frankfurter adapter: src/CurrencyConverter.Api/Providers/FrankfurterProvider.cs — uses Frankfurter API
   - Provider factory: src/CurrencyConverter.Api/Providers/ExchangeRateProviderFactory.cs
   - Correlate internal calls by propagating a CorrelationId header
4. Currency business service and caching
   - Service interface: src/CurrencyConverter.Api/Services/ICurrencyService.cs
   - Implementation: src/CurrencyConverter.Api/Services/CurrencyService.cs — caching (EF Core-backed cache + in-memory short TTL)
   - Data context: src/CurrencyConverter.Api/Data/ExchangeRateDbContext.cs — EF Core with migrations (SQLite dev / Postgres prod)
5. Resilience, retries, and circuit breaker
   - Use Polly policies wired via DI in src/CurrencyConverter.Api/StartupExtensions.cs
   - Retry with exponential backoff + jitter; circuit-breaker for Frankfurter calls; caching fallback
6. Security & RBAC
   - JWT settings: src/CurrencyConverter.Api/Auth/JwtSettings.cs
   - Token service: src/CurrencyConverter.Api/Auth/TokenService.cs — issue/validate JWTs, simple in-memory user store with role claims for interview scope
   - Enforce RBAC on endpoints with [Authorize(Roles = "User,Admin")]
   - Rate limiting: ASP.NET Core rate limiting configured in src/CurrencyConverter.Api/StartupExtensions.cs
7. Observability & Logging
   - Structured logging with Serilog configured in src/CurrencyConverter.Api/Program.cs and src/CurrencyConverter.Api/appsettings.json
   - Log: client IP, client ID (from JWT), method, endpoint, status code, response time, correlation id
   - Expose basic metrics/health endpoints: /health and /metrics
8. Testing
   - Unit tests (xUnit) in tests/CurrencyConverter.Api.Tests/*
     - Target >= 90% coverage for core business logic (ICurrencyService, provider parsing, excluded-currencies validation)
   - Integration tests hitting a mocked Frankfurter (WireMock or HttpMessageHandler) in tests/Integration/*
   - Coverage reports via coverlet and report generation in CI
9. Frontend (React + TypeScript + Vite)
   - Scaffold project at client
     - client/package.json, client/tsconfig.json, client/vite.config.ts
     - Entry: client/src/main.tsx
   - Pages/components:
     - Converter UI at client/src/pages/Converter.tsx — amount, source, target, validate excluded currencies, show errors
     - Latest rates view at client/src/pages/Rates.tsx — select base currency
     - Historical rates at client/src/pages/Historical.tsx — date range selector + paginated table
     - API wrapper: client/src/api/ApiClient.ts — handles auth token injection, correlation id propagation
   - Tests: Jest + React Testing Library for core flows (conversion, excluded-currency validation, historical pagination)
10. Devops & local run
    - Dockerfiles:
      - docker/backend/Dockerfile
      - docker/frontend/Dockerfile
      - docker/docker-compose.yml for local compose with DB
    - CI workflow: .github/workflows/ci.yml — build, test, coverage, lint, container build
11. README & AI usage notes
    - Add README.md explaining setup, design decisions, AI usage (what was accepted/edited), and commands to run

Verification
- Backend:
  - Run unit tests: dotnet test — collect coverage via coverlet; assert coverage >= 90% for target assemblies
  - Run integration tests pointing provider to a local WireMock or mocked HttpClient
  - Start app locally:

    dotnet run --project src/CurrencyConverter.Api

  - Confirm endpoints:
    - GET /api/v1/rates/latest?base=EUR
    - POST /api/v1/convert
    - GET /api/v1/rates/historical?base=EUR&from=2020-01-01&to=2020-01-31&page=1&pageSize=20
- Frontend:
  - Start dev server:

    cd client && npm install && npm run dev

  - Run tests: npm test
  - Manual checks: conversion flow, excluded currency validation, pagination
- CI:
  - GitHub Actions should run dotnet test, npm test, and generate coverage artifacts

Decisions
- Target: NET 8 and C# 12 (chosen for latest LTS/feature parity)
- Frontend: React + TypeScript + Vite
- Persistence: SQLite for local dev, Postgres for prod
- Auth: Simple JWT with in-memory user store and role claims (faster for interview scope; can be swapped to ASP.NET Identity later)
- Exchange provider: Frankfurter per spec — implemented as a pluggable provider behind IExchangeRateProvider
- Resilience libs: Polly for retries & circuit-breaker; EF Core caching + in-memory short-term cache
- Logging: Serilog structured logging and correlation IDs
- Rate limiting: ASP.NET Core built-in rate-limiting middleware

Next actions
- Scaffold the repository layout and create initial files (controllers, interfaces, DI wiring, sample provider, basic frontend scaffold, minimal tests and CI templates).
- Or adjust any of the above decisions if you want different defaults.

Notes
- Excluded currencies (TRY, PLN, THB, MXN) must return HTTP 400 with clear error message when used as source or target in conversion endpoints.
- Ensure internal correlations: propagate CorrelationId when calling Frankfurter and log it alongside request/response.
- AI usage details should be added to README describing which suggestions were accepted vs adapted.
