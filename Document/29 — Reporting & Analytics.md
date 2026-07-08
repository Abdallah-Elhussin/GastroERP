# Phase 29 — Reporting & Analytics

## الهدف

إنشاء منصة تقارير وتحليلات متقدمة (Reporting & Analytics Platform) تخدم جميع وحدات GastroERP، وتوفر لوحات معلومات تفاعلية (Dashboards)، وتقارير ديناميكية، ومؤشرات أداء (KPIs)، مع إمكانية التصدير والتكامل مع Power BI.

---

# قواعد التنفيذ

- الالتزام بـ Clean Architecture.
- الالتزام بـ Domain Driven Design (DDD).
- دعم Multi-Tenant بالكامل.
- جميع الكيانات ترث من BaseEntity.
- جميع البيانات مرتبطة بـ TenantId.
- دعم CQRS باستخدام MediatR.
- دعم Dependency Injection.
- استخدام FluentValidation.
- جميع الخدمات Async.
- عدم وضع Business Logic داخل Controllers.
- دعم Domain Events عند الحاجة.
- دعم Unit Tests.
- Build يجب أن ينتهي بـ 0 Errors / 0 Warnings.

---

# 1. Domain Layer

إنشاء الكيانات التالية:

## Dashboard

يمثل لوحة معلومات.

يشمل:

- Name
- Description
- IsDefault
- IsPublic
- LayoutJson

---

## DashboardWidget

يمثل عنصر داخل Dashboard.

يشمل:

- DashboardId
- WidgetType
- Title
- Position
- Width
- Height
- ConfigurationJson

---

## ReportDefinition

تعريف التقرير.

يشمل:

- Name
- Code
- Module
- Category
- DataSource
- QueryDefinition
- ParametersJson
- IsPublished

---

## ReportExecution

يمثل تشغيل تقرير.

يشمل:

- ReportDefinitionId
- ExecutedBy
- ExecutionDate
- Duration
- Status

---

## ScheduledReport

جدولة التقارير.

يشمل:

- ReportDefinitionId
- CronExpression
- ExportFormat
- EmailRecipients
- IsEnabled

---

## KpiDefinition

تعريف مؤشر الأداء.

يشمل:

- Name
- Code
- Formula
- Module
- TargetValue
- WarningValue
- CriticalValue

---

## KpiSnapshot

تخزين نتائج KPI.

يشمل:

- KpiDefinitionId
- Value
- SnapshotDate

---

# 2. Enums

إنشاء:

- ReportModule
- ReportCategory
- ReportStatus
- WidgetType
- ChartType
- ExportFormat
- KpiTrend
- ScheduleFrequency

---

# 3. Domain Events

إنشاء:

- ReportGeneratedEvent
- ScheduledReportExecutedEvent
- DashboardCreatedEvent
- DashboardUpdatedEvent
- KpiCalculatedEvent

---

# 4. Application Layer

## Services

إنشاء:

- DashboardService
- ReportService
- ReportExecutionService
- KpiEngine
- ExportService
- ChartService
- PowerBiIntegrationService
- ScheduledReportService

---

## Commands

إنشاء:

- CreateDashboard
- UpdateDashboard
- DeleteDashboard
- CreateReport
- UpdateReport
- PublishReport
- ExecuteReport
- ScheduleReport
- CreateKpi
- CalculateKpi

---

## Queries

إنشاء:

- GetDashboard
- GetDashboards
- GetReports
- GetReport
- ExecuteReport
- GetKpiValues
- GetReportHistory
- GetScheduledReports

---

## Validators

إنشاء FluentValidation لجميع Commands.

---

# 5. Dashboard Framework

إنشاء نظام Dashboards يدعم:

- Drag & Drop Layout
- Widgets
- Responsive Layout
- Personal Dashboards
- Shared Dashboards
- Tenant Dashboards
- Favorite Dashboards

---

# 6. Dynamic Reports

دعم:

- Dynamic Filters
- Dynamic Columns
- Grouping
- Sorting
- Aggregation
- Totals
- Subtotals
- Drill Down
- Drill Through

---

# 7. Report Designer

إنشاء Report Designer يدعم:

- Query Builder
- Column Selection
- Calculated Fields
- Parameters
- Conditional Formatting
- Preview

---

# 8. KPI Engine

دعم:

- Revenue KPIs
- Sales KPIs
- HR KPIs
- Inventory KPIs
- Finance KPIs
- Purchasing KPIs
- Customer KPIs

مع:

- Targets
- Trends
- Thresholds
- Historical Tracking

---

# 9. Interactive Charts

دعم الرسوم التالية:

- Bar Chart
- Line Chart
- Pie Chart
- Area Chart
- Donut Chart
- Scatter Chart
- Heat Map
- Gauge
- KPI Cards

مع:

- Zoom
- Filtering
- Drill-down
- Export

---

# 10. Export Engine

دعم التصدير إلى:

- Excel (.xlsx)
- PDF
- CSV
- JSON

مع:

- Branding
- Company Logo
- Header
- Footer
- Page Numbers

---

# 11. Scheduled Reports

إنشاء نظام جدولة يدعم:

- Daily
- Weekly
- Monthly
- Cron Expression

والإرسال عبر:

- Email
- Notification Center

---

# 12. Power BI Integration

إنشاء خدمة:

PowerBiIntegrationService

تدعم:

- Dataset Refresh
- Embedded Reports
- Workspace Configuration
- Secure Token Support

---

# 13. API

إنشاء Controllers:

## DashboardController

يشمل:

- CRUD
- Widgets
- Layout
- Share

---

## ReportsController

يشمل:

- CRUD
- Execute
- Export
- Publish
- Preview

---

## KPIController

يشمل:

- List
- Calculate
- History
- Trends

---

## ScheduledReportsController

يشمل:

- CRUD
- Enable
- Disable
- Execute Now

---

# 14. Permissions

إضافة:

Reporting.*

وتشمل:

- Reporting.View
- Reporting.Create
- Reporting.Edit
- Reporting.Delete
- Reporting.Execute
- Reporting.Export
- Reporting.Publish
- Reporting.Schedule
- Reporting.KPI
- Reporting.Admin

وربطها مع جميع Controllers.

---

# 15. Notifications

إضافة:

- ReportReady
- ScheduledReportCompleted
- ScheduledReportFailed
- KPIThresholdExceeded
- DashboardShared

---

# 16. Background Jobs

إنشاء Jobs:

- ScheduledReportJob
- KpiCalculationJob
- DashboardCacheRefreshJob
- ReportCleanupJob

---

# 17. Health Check

إنشاء:

ReportingHealthCheck

للتحقق من:

- Report Engine
- Scheduler
- Export Engine
- KPI Engine

---

# 18. Swagger

إضافة:

- Tags مستقلة
- وصف لجميع Endpoints
- أمثلة Requests / Responses

---

# 19. Database Migration

إنشاء Migration باسم:

AddReportingAnalytics

**دون تطبيقها على قاعدة البيانات.**

---

# 20. Unit Tests

تغطية:

- Dashboard Tests
- Report Execution Tests
- KPI Calculation Tests
- Export Tests
- Schedule Tests
- Validation Tests

---

# 21. Documentation

إنشاء:

Document/29 — Reporting & Analytics.md

وتحديث:

- Document/README.md
- Document/00 — فهرس التوثيق وملخص المنصة.md

---

# معايير القبول

- Build ناجح.
- 0 Errors.
- 0 Warnings.
- جميع Controllers تعمل.
- جميع Permissions مربوطة.
- جميع الخدمات مسجلة في Dependency Injection.
- Dashboard Framework يعمل.
- Dynamic Reports تعمل.
- KPI Engine يعمل.
- Export إلى Excel وPDF يعمل.
- Scheduled Reports تعمل.
- Power BI Integration تعمل.
- Background Jobs تعمل.
- Health Check يعمل.
- Swagger مكتمل.
- Migration منشأة وغير مطبقة.
- Unit Tests ناجحة.
- Documentation محدثة.

---

# حالة التنفيذ ✅

| الطبقة | الحالة | الملفات الرئيسية |
|--------|--------|------------------|
| Domain | ✅ | `Domain/Entities/Reporting/`, `Domain/Enums/ReportingEnums.cs`, `Domain/Events/Reporting/` |
| Persistence | ✅ | `Persistence/Configurations/Reporting/`, Migration `AddReportingAnalytics` |
| Application | ✅ | `Features/ReportingPlatform/` — Services، CQRS، Validators، EventHandlers |
| API | ✅ | `Controllers/ReportingPlatform/` — `/api/v1/reporting/*` |
| Permissions | ✅ | `Reporting.View/Create/Edit/Delete/Execute/Export/Publish/Schedule/KPI/Admin` |
| Notifications | ✅ | ReportReady، ScheduledReportCompleted/Failed، KPIThresholdExceeded، DashboardShared |
| Jobs | ✅ | ScheduledReportJob، KpiCalculationJob، DashboardCacheRefreshJob، ReportCleanupJob |
| Health | ✅ | `ReportingHealthCheck` |
| Tests | ✅ | `tests/.../Reporting/ReportingPlatformTests.cs` |
| Build | ✅ | 0 Errors / 0 Warnings |

## APIs

| Controller | Routes |
|------------|--------|
| ReportingDashboardsController | CRUD، share، favorite |
| ReportingDefinitionsController | CRUD، publish، execute، preview، export، history |
| ReportingKpisController | CRUD، calculate، history |
| ReportingScheduledController | CRUD، enable/disable، execute now |
| ReportingAnalyticsController | charts، Power BI config/embed/refresh |

## العلاقة مع Phase 23

- **Phase 23** (`/api/v1/reports/*`): ~49 endpoint تقارير read-only من البيانات التشغيلية.
- **Phase 29** (`/api/v1/reporting/*`): منصة metadata — تعريفات، لوحات، جدولة، KPIs — وتنفيذ التقارير عبر `ReportDataResolver` المرتبط بخدمات Phase 23.

## Migrations

```
AddReportingAnalytics — منشأة ⏳ غير مطبقة
```

---

# النتيجة المتوقعة

بعد إكمال **Phase 29** سيحتوي GastroERP على منصة تقارير وتحليلات احترافية متكاملة توفر لوحات معلومات تفاعلية، وتقارير ديناميكية قابلة للتخصيص، ومؤشرات أداء لحظية، ورسوم بيانية تفاعلية، مع دعم التصدير إلى Excel وPDF، وجدولة التقارير، والتكامل مع Power BI، مما يمنح الإدارة رؤية شاملة لاتخاذ القرارات المبنية على البيانات.