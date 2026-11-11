# AI Usage

**Tool:** Claude (Claude Code in VS Code)

**Where it was used**
- **Backend:** I was new to .NET, so AI helped with project setup and explained each command. Most backend code was written with AI, then reviewed and simplified.
- **Frontend:** mostly written with AI, then checked against the brief.
- **Docker:** I had dockerized Node.js apps before, but AI helped with containerizing .NET and building a separate test image.

**Mistakes and how I found them**
- **Package version conflict:** Npgsql pulled in EF Core Relational 10.0.4 while other packages needed 10.0.14. Found with `dotnet list package`; fixed by adding Relational 10.0.14 explicitly.
- **Wrong test database:** the test project used EF InMemory, which can't run `FOR UPDATE`, `ExecuteUpdateAsync` or migrations, and has no locks, so a concurrency test would prove nothing. Replaced with real PostgreSQL databases created per test run via `WebApplicationFactory`.
- **Extra features:** the AI added things the brief didn't ask for (extra UI features, a regex where a simple check worked). Removed extra features that weren't needed.
- **`.gitignore` in the wrong folder:** build files kept getting committed; I noticed and moved it to the repo root.
