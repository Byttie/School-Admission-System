# School Admission System

First-come, first-served online admission system. Parents apply for a seat in a class; the seat is held for a limited time and confirmed when the admission fee is paid. Unpaid holds expire automatically and the seat returns to the pool.

- **Backend:** ASP.NET Core (.NET 10), EF Core, PostgreSQL
- **Frontend:** React 19 (Vite), served by nginx
- **Run with:** Docker Compose

## Run the app

Only Docker is required.

    docker compose up

| Service  | URL                   |
|----------|-----------------------|
| Frontend | http://localhost:3000 |
| Backend  | http://localhost:8080 |

The backend and frontend images are pulled from Docker Hub (`rag1b/admission-backend`, `rag1b/admission-frontend`). The database schema is created automatically on startup.

Stop the app (data is kept):

    docker compose down

Stop and delete all data:

    docker compose down -v

## Configuration

| Variable | Service | Default | Purpose |
|---|---|---|---|
| `ConnectionStrings__Default` | backend | set in `docker-compose.yml` | PostgreSQL connection |
| `HOLD_DURATION_SECONDS` | backend | `600` | How long a seat stays on hold |
| `API_URL` | frontend | `http://localhost:8080` | Backend URL used by the browser |

To try a shorter hold time:

    HOLD_DURATION_SECONDS=30 docker compose up

(PowerShell: `$env:HOLD_DURATION_SECONDS=30; docker compose up`)

## Run the tests

Backend tests (8 tests, including a concurrency test) run against a real PostgreSQL database inside Docker:

    docker compose run --rm backend-tests

The first run builds the test image and takes a few minutes. Each test class uses its own temporary database, which is deleted afterwards, so app data is not affected.

To run them without Docker (requires the .NET 10 SDK and PostgreSQL on `localhost:5432` with user/password `postgres`):

    cd backend
    dotnet test

## Using the app

1. Enter the student's 17-digit birth registration number at the top of the page.
2. **Create Class (Admin):** create a class with a name and number of seats.
3. **Classes:** see total / on-hold / confirmed / available seats (refreshes every 3 seconds) and click **Apply**.
4. **Application:** see the status and time remaining; click **Pay Admission Fee** or **Cancel**.
5. **My Applications:** see all applications for the student.

## Project structure

    backend/
      AdmissionSystem.Api/     API (controllers, EF Core model, migrations)
      AdmissionSystem.Tests/   Integration tests (xUnit, real PostgreSQL)
      Dockerfile
    frontend/
      src/                     React app (pages, components, context)
      Dockerfile
    docker-compose.yml
    ASSUMPTIONS.md
    AI_USAGE.md
