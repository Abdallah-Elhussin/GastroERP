# GastroERP

Enterprise ERP/POS platform for restaurants — multi-tenant, bilingual (Arabic/English), built on **.NET 9**.

---

## Project Overview

GastroERP combines daily restaurant operations (POS, kitchen, delivery) with full back-office ERP: inventory, purchasing, finance, HR, workflow approvals, reporting/BI, and AI intelligence.

---

## Features

- Multi-tenant SaaS architecture
- POS, KDS, floor plans, shifts, payments
- Inventory, recipes, purchasing, stock counts
- Double-entry finance & ZATCA-ready invoicing
- CRM, loyalty, delivery management
- HR: attendance, leave, payroll, recruitment
- Central workflow & approval engine
- Reporting (40+ reports) + Reporting Platform (dashboards, KPIs, scheduling)
- AI: forecasting, recommendations, NL query, fraud/churn analytics

---

## Architecture

Clean Architecture + DDD + CQRS (MediatR). See [`Document/GastroERP Documentation.md`](Document/GastroERP%20Documentation.md).

---

## Technology Stack

| Layer | Stack |
|-------|-------|
| Backend | .NET 9, ASP.NET Core, EF Core 9, SQL Server |
| Auth | JWT, permission-based authorization |
| Frontend | `Frontend/` (separate app) |

---

## Project Structure

```
gastro-erp/
├── Backend/          # .NET solution (Domain, Application, Persistence, Infrastructure, Presentation)
├── Frontend/         # Client application
└── Document/         # Project documentation
```

---

## Installation

**Prerequisites:** .NET 9 SDK, SQL Server or LocalDB

```powershell
cd d:\gastro-erp\Backend
dotnet restore
dotnet build
```

---

## Configuration

Edit `Backend/src/GastroErp.Presentation/appsettings.json`:

- `ConnectionStrings:DefaultConnection`
- `Jwt:*` (issuer, audience, key)
- `Cors:AllowedOrigins`

---

## Database

```powershell
cd d:\gastro-erp\Backend
dotnet ef database update --project src/GastroErp.Persistence --startup-project src/GastroErp.Presentation
```

Migrations are applied; database seeds on first API run.

---

## Running

```powershell
cd d:\gastro-erp\Backend\src\GastroErp.Presentation
dotnet run --launch-profile http
```

- API: `http://localhost:5162`
- Swagger: `http://localhost:5162/swagger`
- Health: `http://localhost:5162/health/ready`

---

## Testing

```powershell
cd d:\gastro-erp\Backend
dotnet test
```

Integration tests for Reporting Platform: `ReportingPlatformIntegrationTests` (requires .NET 9 runtime).

---

## Documentation

Start here: [`Document/00 - Documentation Index.md`](Document/00%20-%20Documentation%20Index.md)

Standards: [`Document/04 - Documentation Standards.md`](Document/04%20-%20Documentation%20Standards.md)

Post-Phase 29 verification: [`Document/Post Phase 29 — Verification Report.md`](Document/Post%20Phase%2029%20—%20Verification%20Report.md)

---

## Roadmap

- Phase 30+: Audit & Compliance, production deployment guides, frontend integration

---

## Build Status

```
0 Errors · 0 Warnings
```

Backend Phases **1–29** complete.

---

## License

Proprietary — GastroERP project.
