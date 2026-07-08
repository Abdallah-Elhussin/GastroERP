# Phase 28 — Workflow Integration Across Modules (Completion)

> **📚 فهرس التوثيق:** [`00 — فهرس التوثيق وملخص المنصة.md`](00%20—%20فهرس%20التوثيق%20وملخص%20المنصة.md)

## Build Target

- 0 Errors · 0 Warnings · Production Ready

---

## الحالة — ✅ مكتمل

| البند | الحالة |
|-------|--------|
| Workflow Integration Service + Outcome Router | ✅ |
| HR Integration (Leave, Payroll, Performance, Recruitment, HrWorkflowRequest) | ✅ |
| Purchasing Integration (PO Submit → Workflow) | ✅ |
| Inventory Integration (StockCount, Adjustment, Transfer) | ✅ |
| POS Integration (Refund) | ✅ |
| Domain Events (Submit + Completion) | ✅ |
| Notifications (Assigned, Cancelled, Returned) | ✅ |
| API (Status, Timeline, Restart, Return) | ✅ |
| Permissions (Restart, Return, ViewTimeline) | ✅ |
| Background Jobs (Retry, Timeout) | ✅ |
| Health Check (Queue, Escalations, Delegations) | ✅ |
| Migration `AddWorkflowIntegration` | ✅ (لم تُطبَّق) |
| Unit Tests | ✅ |

---

## 1. نمط التكامل

```
Module Submit → Domain Event → WorkflowSubmissionHandler → StartWorkflow
Workflow Complete/Reject/Cancel → WorkflowOutcomeHandler → ModuleOutcomeService → Entity.Approve/Reject
```

**لا يوجد منطق موافقات مباشر** في:
- `ApproveLeaveCommand` → يوجّه لـ Workflow API
- `ApprovePayrollRunCommand` → يوجّه لـ Workflow API
- `ApprovePurchaseOrderCommand` → يوجّه لـ Workflow API

---

## 2. الوحدات المربوطة

| الوحدة | ReferenceType | Workflow Code |
|--------|---------------|---------------|
| HR Leave | `LeaveRequest` | `LEAVE-APPROVAL` |
| HR Overtime/Loan/... | `HrWorkflowRequest` | `OVERTIME-APPROVAL`, `LOAN-APPROVAL`, ... |
| HR Payroll | `PayrollRun` | `PAYROLL-APPROVAL` |
| HR Performance | `PerformanceRecord` | `PERFORMANCE-APPROVAL` |
| HR Recruitment | `JobApplicant` | `RECRUITMENT-APPROVAL` |
| Purchasing | `PurchaseOrder` | `PO-APPROVAL` |
| Inventory | `StockCount`, `StockAdjustment`, `StockTransfer` | `STOCK-*-APPROVAL` |
| POS | `Refund` | `POS-REFUND-APPROVAL` |

> Finance / CRM endpoints جاهزة في `WorkflowIntegrationCodes` — تُفعَّل عند إضافة كيانات الموافقة.

---

## 3. API الجديدة

| Method | Path | Permission |
|--------|------|------------|
| GET | `/api/v1/workflow/instances/status?referenceType=&referenceId=` | `Workflow.View` |
| GET | `/api/v1/workflow/instances/{id}/timeline` | `Workflow.ViewTimeline` |
| POST | `/api/v1/workflow/instances/restart` | `Workflow.Restart` |
| POST | `/api/v1/workflow/instances/return` | `Workflow.Return` |
| POST | `/api/v1/hr/workflow-requests` | `Hr.Leave.Request` |

---

## 4. HR Workflow Requests

كيان `HrWorkflowRequest` يغطي:
- Overtime · Loan · SalaryAdvance · Resignation · Promotion · Transfer

---

## 5. Background Jobs

| Job | الوظيفة |
|-----|---------|
| `WorkflowRetryJob` | تذكير بالموافقات العالقة (+48h) |
| `WorkflowTimeoutJob` | إلغاء workflows أقدم من 30 يوم |

---

## 6. Migration

```
AddWorkflowIntegration — HrWorkflowRequests + Refunds DbSet exposure
```

---

## 7. Master Data Seed (Phase 28+)

عند تشغيل التطبيق (`SeedAsync`) يُنشأ تلقائياً للمستأجر `default`:

| المجال | البيانات |
|--------|----------|
| Organization | شركة، فرع، مستودع + مناطق |
| Finance | دليل حسابات كامل (20+ حساب) + ضريبة 15% + فترة مالية |
| Inventory | 8 وحدات قياس + تحويلات + 5 تصنيفات + أسباب تسوية |
| Menu | 4 تصنيفات + 3 مستويات تسعير + 4 منتجات بمقاسات |
| Workflow | 18 تعريف workflow منشور ومفعّل |

**المسارات:** `Backend/src/GastroErp.Persistence/Seeders/`

---

*آخر تحديث: يوليو 2026*
