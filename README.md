# PhysioDesk

Clinic management app for a physiotherapy practice — patients, therapists,
scheduling, billing, dashboard.

## Stack note (read first)

The take-home spec originally required **Python/FastAPI** for the backend and
PostgreSQL. This was changed, **agreed with PhysioGhar HealthTech over
LinkedIn**, to:

- Backend: **ASP.NET Core Web API (.NET 8, C#)**
- ORM: **EF Core 8**, code-first migrations
- Database: **SQL Server** (kept provider-agnostic — no raw T-SQL, no
  SQL-Server-only column types — so switching back to PostgreSQL is a
  one-line `UseNpgsql` change)
- Frontend: **Next.js (App Router) + TypeScript + Tailwind**, as originally
  specified — unchanged.

## Status

Build order (per plan): Auth → Patients → Therapists → Scheduling → Billing →
Dashboard. Each feature is built and verified end-to-end before the next
starts.

- [x] Auth — login, JWT access token + httpOnly-cookie refresh token, role
      policies wired (`AdminOnly`, `StaffOrAdmin`)
- [ ] Patients
- [ ] Therapists
- [ ] Scheduling
- [ ] Billing
- [ ] Dashboard

## Setup

### Prerequisites
- .NET 8 SDK
- SQL Server (local instance or container) — see connection string below
- Node 18+

### Backend

```bash
cd backend
dotnet restore

# Local secrets (do not commit real values into appsettings.json):
dotnet user-secrets init --project PhysioDesk.Api
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,1433;Database=PhysioDesk;User Id=sa;Password=<your-password>;TrustServerCertificate=True" --project PhysioDesk.Api
dotnet user-secrets set "Jwt:SigningKey" "<a long random string, 32+ chars>" --project PhysioDesk.Api

dotnet ef migrations add InitialCreate --project PhysioDesk.Api
dotnet ef database update --project PhysioDesk.Api

dotnet run --project PhysioDesk.Api
```

API: `https://localhost:5001` (check `launchSettings.json` for the exact
port). Swagger UI: `https://localhost:5001/swagger`.

Migrations + seed run automatically on startup (`DbInitializer`), so no
separate seed command is needed after `dotnet run`.

### Test credentials

| Role  | Email                  | Password   |
|-------|------------------------|------------|
| Admin | admin@physiodesk.dev   | Admin123!  |
| Staff | staff@physiodesk.dev   | Staff123!  |

### Frontend

Not yet scaffolded — next slice.

## Assumptions

- **Clinic-local time**: all appointment dates/times are stored and displayed
  as entered, with no timezone conversion. The clinic operates in a single
  timezone; multi-timezone support is out of scope.
- **Therapist soft-delete**: deleting a therapist sets `IsActive = false`
  rather than removing the row. Existing appointments referencing that
  therapist are preserved as historical records.
- Single active refresh token per user (no multi-device session table) — in
  scope for a small clinic staff, out of scope would be a full session list.

## What I'd do with more time

- (updated as the build progresses)

## API docs

Swagger/OpenAPI UI is served at `/swagger` once the backend is running.
