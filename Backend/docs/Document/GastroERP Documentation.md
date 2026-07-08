# GastroERP Documentation

## الهدف

الدليل الرسمي لمشروع **GastroERP** — منصة ERP/POS للمطاعم (Multi-Tenant، عربي/إنجليزي، .NET 9).

---

## المحتويات

1. [مقدمة](#مقدمة)
2. [Architecture](#architecture)
3. [Technology Stack](#technology-stack)
4. [Modules](#modules)
5. [Phases](#phases)
6. [APIs](#apis)
7. [Database](#database)
8. [Workflow](#workflow)
9. [Reporting & Analytics](#reporting--analytics)
10. [Deployment & Security](#deployment--security)
11. [Roadmap](#roadmap)

---

## مقدمة

GastroERP يغطي:

- **POS** — طلبات، مطبخ، دفع، ورديات
- **ERP** — مخزون، مشتريات، وصفات، محاسبة
- **CRM & Delivery**
- **HR & Payroll**
- **Workflow & Approvals**
- **Reporting & BI** + **AI Platform**

📚 **الفهرس:** [`00 - Documentation Index.md`](00%20-%20Documentation%20Index.md)

---

## Architecture

- **Clean Architecture** — Domain → Application → Infrastructure → Presentation
- **DDD** — كيانات غنية، Domain Events، Multi-Tenant
- **CQRS** — MediatR + FluentValidation
- **EF Core** — SQL Server

تفاصيل: [`06-ARCHITECTURE_DECISIONS.md`](06-ARCHITECTURE_DECISIONS.md)

---

## Technology Stack

| الطبقة | التقنية |
|--------|---------|
| Backend | .NET 9, ASP.NET Core, EF Core 9 |
| Auth | JWT, Permission-based |
| Jobs | ScheduledJobCatalog |
| Frontend | (مجلد `Frontend/`) |

---

## Modules

| الوحدة | Prefix API |
|--------|------------|
| Organization | `/api/v1/organization` |
| Menu | `/api/v1/menu` |
| Inventory | `/api/v1/inventory` |
| Sales / POS | `/api/v1/sales` |
| Finance & Accounting | `/api/v1/finance` |
| HR | `/api/v1/hr` |
| Workflow Engine | `/api/v1/workflow` |
| Reports (Phase 23) | `/api/v1/reports` |
| Reporting Platform (Phase 29) | `/api/v1/reporting` |
| AI | `/api/v1/ai` |
| Jobs / Notifications | `/api/v1/jobs`, `/api/v1/notifications` |

تفاصيل: [`Modules/README.md`](Modules/README.md)

---

## Phases

| # | الموضوع | الحالة |
|---|---------|--------|
| 1–22 | Core ERP/POS/Finance | ✅ |
| 23 | Reporting read-only | ✅ |
| 24 | Jobs & Notifications | ✅ |
| 25 | AI Platform (5 sub-phases) | ✅ |
| 26 | Human Resources | ✅ |
| 27 | Workflow Engine | ✅ |
| 28 | Workflow Integration | ✅ |
| 29 | Reporting & Analytics Platform | ✅ |

تفاصيل: [`Phases/README.md`](Phases/README.md)

---

## APIs

- Swagger: `/swagger` عند تشغيل `GastroErp.Presentation`
- Health: `/health/ready`, `/health/live`, `/health/db`
- توثيق API: [`14-API_IMPLEMENTATION.md`](14-API_IMPLEMENTATION.md)

---

## Database

- **Connection:** `appsettings.json` → `GastroErpDb` (LocalDB افتراضياً)
- **Migrations:** 16 migration مطبّقة (يشمل `AddReportingAnalytics`, `AddReportingForeignKeys`)
- **Seed:** Tenant `default` + Chart of Accounts + Workflow + Master Data

```powershell
cd d:\gastro-erp\Backend
dotnet ef database update --project src/GastroErp.Persistence --startup-project src/GastroErp.Presentation
```

تقرير التحقق: [`Post Phase 29 — Verification Report.md`](Post%20Phase%2029%20—%20Verification%20Report.md)

---

## Workflow

- محرك مركزي: تعريفات، instances، delegations، escalation
- تكامل HR، Purchasing، Inventory، POS
- 📄 [`27 — Workflow & Approval Engine.md`](27%20—%20Workflow%20&%20Approval%20Engine.md)
- 📄 [`28 — Workflow Integration.md`](28%20—%20Workflow%20Integration.md)

---

## Reporting & Analytics

| الطبقة | الوصف |
|--------|--------|
| Phase 23 | ~49 endpoint تقارير تشغيلية read-only |
| Phase 29 | Dashboards، Report Definitions، KPIs، Scheduling، Export، Power BI stub |

📄 [`29 — Reporting & Analytics.md`](29%20—%20Reporting%20&%20Analytics.md)

---

## Deployment & Security

- [`04-SECURITY_LOGGING_AND_PRODUCTION.md`](04-SECURITY_LOGGING_AND_PRODUCTION.md)
- JWT، Permissions، Audit، Rate Limiting

---

## Roadmap

- Phase 30+ (مقترح): Audit & Compliance، Production hardening، Frontend integration

---

## Build Status

```
0 Errors · 0 Warnings
```

---

## النتيجة

GastroERP Backend enterprise جاهز للتطوير والإنتاج على مستوى المعمارية والوحدات — مع توثيق موحّد وقاعدة بيانات محدّثة.
