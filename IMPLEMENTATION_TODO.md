# Implementation TODO — Currency Converter Platform

This file captures the implementation-phase tasks derived from the project prompt and requirements.

- [x] Create implementation TODO file

## Implemented (detected in codebase)

- [x] Backend bootstrap: `Program.cs` (Serilog configured, controllers wired)
- [x] Controllers: `CurrencyController.cs`, `AuthController.cs` (endpoints + excluded-currency checks)
- [x] Auth helpers: `TokenService.cs`, `JwtSettings.cs`, and `appsettings.json` (JWT settings present)
- [x] Service & provider: `ICurrencyService`, `CurrencyService.cs`, `IExchangeRateProvider`, `FrankfurterProvider.cs`
- [x] Provider factory: `ExchangeRateProviderFactory.cs` (returns Frankfurter provider)
- [x] Persistence model: `ExchangeRateDbContext.cs` (cache entity defined)
- [x] Frontend scaffold: `client/package.json`, `client/src/*` (React + Vite files present)
- [x] Project plan & CI: `.github/prompts/plan-currencyConverterPlan.prompt.md`, `.github/workflows/ci.yml`


Core backend wiring

- [x] **Wire DI registrations and service registration:** register `ICurrencyService` → `CurrencyService`, `IExchangeRateProvider`/`FrankfurterProvider`, `TokenService`, and `ExchangeRateDbContext` in `StartupExtensions` and call from `Program.cs`.
- [x] **Configure JWT auth and RBAC:** implement `AddAuth` to configure `JwtBearer` using `JwtSettings` from `appsettings.json`; add role-based policy and protect endpoints. (JWT middleware wired; RBAC policy enforcement to be applied per-endpoint)
- [x] **Register HttpClient + Polly:** configure named `HttpClient` for Frankfurter with retry (exponential backoff + jitter) and circuit-breaker policies.
- [ ] **Implement provider factory & provider DI:** ensure `ExchangeRateProviderFactory` can produce providers; register `FrankfurterProvider` and allow future providers.
- [x] **Implement caching:** EF Core-backed cache (`ExchangeRateCache`) + short-lived in-memory TTL caching for latest rates; cache fallback on provider failures.
- [ ] **Finish controllers to use services:** update `CurrencyController` to call `ICurrencyService` for `latest`, `convert`, `historical` (with pagination) and keep excluded-currencies validation.
- [x] **Rate limiting & versioning:** add ASP.NET Core rate-limiting middleware and API versioning (e.g., `api/v1/*`).
- [ ] **Logging & correlation:** configure Serilog using `appsettings.json` and add correlation id propagation for internal HTTP calls; log client IP, client ID (from JWT), method, endpoint, status, and latency.

Frontend

- [ ] **API client:** implement `client/src/api/ApiClient.ts` with auth token injection, correlation-id header propagation, and error handling.
- [ ] **Converter page:** build `Converter.tsx` with amount/source/target, excluded-currency validation, loading states, and friendly errors.
- [ ] **Latest rates view:** build rates UI for a selected base currency.
- [ ] **Historical rates view:** date-range selector + paginated results; wire to backend pagination.
- [ ] **Type safety & tests:** use TypeScript types and add component tests for core flows.

Testing & quality

- [ ] **Unit tests:** add xUnit unit tests targeting `CurrencyService`, provider parsing, and excluded-currency validation; aim for >= 90% coverage.
- [ ] **Integration tests:** mock Frankfurter (HttpMessageHandler or WireMock) to test end-to-end behavior.
- [ ] **Coverage reports:** integrate coverlet and produce reports in CI.

DevOps & deployment

- [ ] **Dockerfiles & compose:** add backend/frontend Dockerfiles and `docker-compose` for local dev (SQLite or Postgres service as option).
- [ ] **CI workflow:** complete `.github/workflows/ci.yml` to build, test, and publish coverage artifacts.
- [ ] **Environment support:** add appsettings.*.json for Dev/Test/Prod and document env var overrides.
- [ ] **Deployment guidance:** document horizontal scaling concerns, DB choice, and secrets management.

Security & housekeeping

- [ ] **Replace JWT placeholder secret:** ensure secure secret management (local dev vs prod instructions).
- [ ] **Review exposed configs:** ensure no secrets checked into repo and document what must be set.

Docs & AI usage

- [ ] **README:** add setup instructions, architecture overview, AI-usage notes (what AI suggested, what was accepted/changed), assumptions, and future improvements.

Verification & run

- [ ] **Local run & smoke tests:** run backend and frontend; verify endpoints:
  - `POST /api/v1/auth/login` -> get token
  - `GET /api/v1/currency/latest?base=EUR`
  - `POST /api/v1/currency/convert`
  - `GET /api/v1/currency/historical?base=EUR&from=YYYY-MM-DD&to=YYYY-MM-DD&page=1&pageSize=20`

Priority suggestions

- Phase A (High): DI/Auth wiring, HttpClient+Polly, finish controllers, replace JWT secret, basic frontend API client, local run smoke tests.
- Phase B (Medium): Caching, logging/correlation, provider factory polish, frontend pages.
- Phase C (Low): Extensive tests to reach 90% coverage, CI, Docker compose, deployment docs.

If you want, I can start implementing Phase A now (update `StartupExtensions`, `Program.cs`, register services, and add `JwtBearer` configuration).