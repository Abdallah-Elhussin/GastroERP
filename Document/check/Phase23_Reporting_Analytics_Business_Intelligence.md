# GastroERP Backend Roadmap
# Phase 23
# Reporting, Analytics & Business Intelligence

## Current Status

### Completed

- Phase 1–15 — Core Foundation
- Phase 16 — POS Core
- Phase 17 — Payments & Cash Management
- Phase 18 — Kitchen, Dining & KDS
- Phase 19 — Invoicing & Fiscal Compliance
- Phase 20 — Delivery Management
- Phase 21 — CRM & Loyalty Management
- Phase 22 — Finance & Accounting Integration

Build Status

- ✅ 0 Errors
- ✅ 0 Warnings

Database

- Finance module migration created (`AddFinanceAccountingModule`) — not applied automatically.
- Pending migrations may include CRM, Sales Operations, and Finance modules.

---

# Mission

Deliver a unified Reporting & Business Intelligence layer that transforms operational and financial data from GastroERP modules into actionable insights for executives, branch managers, and operational staff.

Reporting must read from existing modules — no duplicate transactional stores.

---

# Architecture Rules

Maintain:

- Clean Architecture
- CQRS for report queries
- Read-optimized projections (views / materialized summaries where needed)
- Multi-Tenancy & Branch Isolation
- Permission-based access
- Arabic/English localization & RTL-ready exports
- Async export for large datasets

Do NOT duplicate source-of-truth data.

---

# Module Scope

## 1. Executive Dashboards

Real-time KPI widgets:

- Net Sales (today / week / month)
- Orders count & average ticket
- Top products & categories
- Payment mix (cash / card / digital)
- Delivery vs dine-in split
- Gross margin indicators (when COGS available)
- Cash position summary (from Finance GL)

Technical:

- `DashboardQueryService` aggregating Sales, Payments, Finance
- Cached snapshots (Redis or in-memory per tenant, configurable TTL)
- Branch / company / tenant drill-down filters

---

## 2. Operational Reports

| Report | Source Modules |
|--------|----------------|
| Sales by Hour / Day / Branch | Sales, Organization |
| Order Status & Void Report | Sales |
| Kitchen Performance (ticket times) | Kitchen, Sales |
| Table Turnover | FloorPlan, Sales |
| Delivery SLA & Driver Performance | Delivery |
| Shift & Cash Reconciliation | Payments, CashRegister |
| Inventory Movement Summary | Inventory |

APIs under `Reports.Operational.*` permissions.

---

## 3. Financial Reports

Build on Phase 22 Finance module:

| Report | Description |
|--------|-------------|
| Trial Balance | Already in Phase 22 — extend with period comparison |
| Profit & Loss | Revenue vs COGS vs Expenses by period |
| Balance Sheet | Assets / Liabilities / Equity snapshot |
| Cash Flow Summary | Cash accounts movement |
| VAT Report | Output vs input tax (ZATCA-ready layout) |
| AR / AP Aging | Receivables & payables buckets |
| Journal Audit Trail | Posted entries with source trace |

Integrate with `AccountingTransaction` for source document drill-back.

---

## 4. Inventory Analytics

- Stock valuation (FIFO / weighted average — align with inventory costing policy)
- Slow / dead stock analysis
- Waste & shrinkage trends
- Purchase price variance
- Recipe cost vs menu price margin

Source: Inventory, Purchasing, Menu, Recipes.

---

## 5. Sales Analytics

- Product mix & contribution
- Modifier upsell analysis
- Discount & promotion effectiveness (CRM coupons)
- Channel analysis (dine-in, takeaway, delivery)
- Hourly heatmaps
- Cohort repeat purchase (linked to CRM Customer)

---

## 6. Customer Analytics

Source: CRM & Loyalty module

- Active customers & new registrations
- Loyalty points issued / redeemed
- Membership tier distribution
- Gift card liability report
- Campaign ROI (PromotionCampaign)

---

## 7. KPI Engine

Configurable KPI definitions:

- Formula-based (e.g. `NetSales / OrderCount`)
- Threshold alerts (green / amber / red)
- Target vs actual by branch & period
- Scheduled KPI snapshot jobs

Domain:

- `KpiDefinition`, `KpiSnapshot`, `KpiAlert`

---

## 8. Power BI Integration

- Semantic model documentation (star schema mapping)
- OData or dedicated read API endpoints for BI tools
- Service principal auth for embedded dashboards
- Incremental refresh friendly date partitions
- Export dataset definitions (tables, relationships, measures)

Tables to expose (read-only):

- FactSales, FactPayments, FactInventoryMovement, FactJournalLines
- DimDate, DimBranch, DimProduct, DimCustomer, DimAccount

---

## 9. Data Export

Formats:

- Excel (ClosedXML / EPPlus)
- CSV
- PDF (summary reports)

Features:

- Async job queue for large exports
- Email delivery when complete
- Export audit log (who, what, when)

---

## 10. Scheduled Reports

- Cron-based scheduler (Hangfire or native .NET hosted service)
- Report subscription per user / role
- Parameters: branch, date range, format
- Failure retry & notification

---

# Application Layer

Generate:

- `Features/Reporting/Queries/` — all report queries
- `Features/Reporting/Services/` — aggregation, caching, export
- DTOs per report with filter objects
- Validators for date ranges & branch scope

Services:

- `ReportAggregationService`
- `DashboardService`
- `KpiCalculationService`
- `ReportExportService`
- `ScheduledReportService`

---

# REST APIs

## ReportsController (Operational)

- GET sales-summary
- GET kitchen-performance
- GET delivery-sla
- GET shift-reconciliation

## FinancialReportsController

- Extend Phase 22 reports with P&L, Balance Sheet, VAT, Aging

## DashboardController

- GET executive-dashboard
- GET branch-dashboard

## ExportController

- POST export (async job)
- GET export/{jobId}/status
- GET export/{jobId}/download

## KpiController

- CRUD KPI definitions
- GET kpi-snapshots

---

# Permissions

- `Reports.Operational.View`
- `Reports.Financial.View`
- `Reports.Dashboard.View`
- `Reports.Export`
- `Reports.Schedule.Manage`
- `Kpi.View`, `Kpi.Manage`

---

# Persistence

Optional read models:

- SQL views: `vw_DailySalesSummary`, `vw_ProductSales`, `vw_JournalSummary`
- Materialized summary tables refreshed by background jobs
- Indexes on report filter columns (TenantId, BranchId, PostingDate, CreatedAt)

Migration: `AddReportingAnalyticsModule` — do NOT apply automatically.

---

# Performance

- AsNoTracking() on all report queries
- Pagination mandatory for detail reports
- Projection to DTOs (no full entity load)
- Date-range limits (max 365 days default)
- Read replica support (future)

---

# Security

- Tenant & branch filters enforced in every query handler
- Row-level security via query filters
- Export files scoped to requesting tenant
- Audit log for financial report access

---

# Integration Points

| Module | Data Provided |
|--------|---------------|
| Sales | Orders, items, discounts |
| Payments | Payment methods, shifts |
| Finance | GL balances, journals |
| Inventory | Stock, movements, COGS |
| CRM | Customers, loyalty, campaigns |
| Delivery | SLA, drivers |
| Invoicing | Tax lines, fiscal data |

---

# Final Validation

Verify:

- Build = 0 Errors / 0 Warnings
- No duplicate report DTOs
- No N+1 in aggregation queries
- Export jobs are idempotent
- RTL PDF/Excel headers for Arabic tenants

---

# Estimated Phases Within Phase 23

1. **Milestone 1** — Executive Dashboard + Sales Summary reports
2. **Milestone 2** — Financial reports (P&L, Balance Sheet, VAT)
3. **Milestone 3** — Inventory & Customer analytics
4. **Milestone 4** — KPI Engine + Scheduled Reports
5. **Milestone 5** — Power BI API + Data Export pipeline

---

# Out of Scope (Future)

- Real-time streaming analytics (Kafka / Event Hub)
- ML demand forecasting
- External data warehouse ETL (dbt / Azure Synapse)

Do NOT implement Phase 23 until explicitly requested.
