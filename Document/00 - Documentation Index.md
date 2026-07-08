# GastroERP — Documentation Index

## الهدف

المرجع الرئيسي لجميع وثائق GastroERP — معمارية، تطوير، مراحل، ووحدات.

---

## المحتويات

- [ابدأ هنا](#ابدأ-هنا)
- [Architecture](#architecture)
- [Development](#development)
- [Phases](#phases)
- [Modules](#modules)
- [API & Database](#api--database)
- [Guides & Standards](#guides--standards)
- [Build Status](#build-status)

---

## ابدأ هنا

| المستند | الوصف |
|---------|--------|
| [`GastroERP Documentation.md`](GastroERP%20Documentation.md) | الدليل الرسمي الشامل |
| [`00 — فهرس التوثيق وملخص المنصة.md`](00%20—%20فهرس%20التوثيق%20وملخص%20المنصة.md) | ملخص المنصة بالعربية |
| [`01-Project-Verssion.md`](01-Project-Verssion.md) | الرؤية والإصدار |
| [`Post Phase 29 — Verification Report.md`](Post%20Phase%2029%20—%20Verification%20Report.md) | تقرير التحقق من DB |

---

## Architecture

| المستند |
|---------|
| [`00-DOMAIN_MODEL_AND_CONTEXTS.md`](00-DOMAIN_MODEL_AND_CONTEXTS.md) |
| [`06-ARCHITECTURE_DECISIONS.md`](06-ARCHITECTURE_DECISIONS.md) |
| [`Architecture/ADR-000-Bilingual-System.md`](Architecture/ADR-000-Bilingual-System.md) |

---

## Development

| المستند |
|---------|
| [`10-APPLICATION_LAYER_IMPLEMENTATION.md`](10-APPLICATION_LAYER_IMPLEMENTATION.md) |
| [`11-INFRASTRUCTURE_IMPLEMENTATION.md`](11-INFRASTRUCTURE_IMPLEMENTATION.md) |
| [`14-API_IMPLEMENTATION.md`](14-API_IMPLEMENTATION.md) |
| [`15-DATABASE_AND_SYSTEM_INTEGRATION.md`](15-DATABASE_AND_SYSTEM_INTEGRATION.md) |
| [`04 - Documentation Standards.md`](04%20-%20Documentation%20Standards.md) |

---

## Phases

| Phase | المستند |
|-------|---------|
| 23 — Reporting (read-only) | [`23- Reporting, Analytics & Business Intelligence.md`](23-%20Reporting,%20Analytics%20&%20Business%20Intelligence.md) |
| 24 — Automation | [`24 — Background Jobs, Notifications & External Integrations.md`](24%20—%20Background%20Jobs,%20Notifications%20&%20External%20Integrations.md) |
| 25 — AI Platform | [`25 — AI & Intelligent Restaurant Platform.md`](25%20—%20AI%20&%20Intelligent%20Restaurant%20Platform.md) |
| 26 — HR | [`26 — Human Resources & Workforce Management (Completion).md`](26%20—%20Human%20Resources%20&%20Workforce%20Management%20(Completion).md) |
| 27 — Workflow | [`27 — Workflow & Approval Engine.md`](27%20—%20Workflow%20&%20Approval%20Engine.md) |
| 28 — Workflow Integration | [`28 — Workflow Integration.md`](28%20—%20Workflow%20Integration.md) |
| 29 — Reporting Platform | [`29 — Reporting & Analytics.md`](29%20—%20Reporting%20&%20Analytics.md) |

📁 [`Phases/README.md`](Phases/README.md) — فهرس تفصيلي + roadmaps في `check/`

---

## Modules

📁 [`Modules/README.md`](Modules/README.md) — Finance، HR، Workflow، Reporting، Inventory، …

---

## API & Database

| الموضوع | المرجع |
|---------|--------|
| API Routes | `Backend/src/GastroErp.Presentation/Common/ApiRoutes.cs` |
| Migrations | `Backend/src/GastroErp.Persistence/Migrations/` |
| Seed | `Backend/src/GastroErp.Persistence/Seeders/` |

**Migrations:** ✅ مطبّقة على LocalDB (`GastroErpDb`) — انظر تقرير التحقق.

---

## Guides & Standards

| المستند |
|---------|
| [`04-SECURITY_LOGGING_AND_PRODUCTION.md`](04-SECURITY_LOGGING_AND_PRODUCTION.md) |
| [`05-GLOSSARY.md`](05-GLOSSARY.md) |
| [`check/`](check/) — Roadmaps تفصيلية |

---

## Build Status

```
0 Errors · 0 Warnings
```

Backend Phases **1–29** مكتملة.

---

## النتيجة

نقطة دخول واحدة لجميع وثائق GastroERP — محدّثة بعد Phase 29 وPost-Verification.
