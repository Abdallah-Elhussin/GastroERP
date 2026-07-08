# Phase 27 — Workflow & Approval Engine (Completion)

> **📚 فهرس التوثيق:** [`00 — فهرس التوثيق وملخص المنصة.md`](00%20—%20فهرس%20التوثيق%20وملخص%20المنصة.md)

## Build Target

- 0 Errors · 0 Warnings · Production Ready

---

## الحالة — ✅ مكتمل

| البند | الحالة |
|-------|--------|
| Domain Entities + Enums + Events | ✅ |
| Persistence + EF Configurations | ✅ |
| Application Services (Engine, Approval, Delegate, Escalation) | ✅ |
| CQRS Commands + Queries + Validators | ✅ |
| API Controllers + Routes | ✅ |
| Permissions (`Workflow.*`) | ✅ |
| Dependency Injection | ✅ |
| Domain Events + Handlers | ✅ |
| Notifications (Phase 24) | ✅ |
| Scheduled Jobs | ✅ |
| Health Checks | ✅ |
| Swagger Tags | ✅ |
| Migration `AddWorkflowEngine` | ✅ (لم تُطبَّق) |
| Unit Tests (Domain) | ✅ |

---

## 1. الهدف

محرك **Workflow & Approval** مركزي يخدم جميع وحدات GastroERP (HR، Finance، Purchasing، CRM…) دون تكرار منطق الموافقات داخل كل وحدة.

---

## 2. Domain

| الكيان | المسار |
|--------|--------|
| WorkflowDefinition | `Domain/Entities/Workflow/WorkflowEntities.cs` |
| WorkflowStep | ↑ |
| WorkflowCondition | ↑ |
| WorkflowInstance | ↑ |
| WorkflowApproval | ↑ |
| WorkflowHistory | ↑ |
| ApprovalDelegate | ↑ |
| ApprovalEscalation | ↑ |

**Enums:** `Domain/Enums/WorkflowEnums.cs`  
**Events:** `Domain/Events/Workflow/WorkflowEvents.cs`

---

## 3. Application Services

| الخدمة | الوظيفة |
|--------|---------|
| `WorkflowDefinitionService` | CRUD + Publish + Activate/Deactivate |
| `WorkflowEngine` | بدء سير العمل، انتقال الخطوات، تقييم الشروط |
| `ApprovalService` | اعتماد / رفض |
| `DelegateService` | تفويض الصلاحيات |
| `EscalationService` | تصعيد تلقائي |
| `WorkflowHistoryService` | سجل المراجعة |
| `WorkflowJobExecutor` | مهام خلفية (تصعيد، تذكير، تنظيف، انتهاء تفويض) |

---

## 4. Permissions

```
Workflow.View | Create | Edit | Delete | Publish
Workflow.Start | Approve | Reject | Cancel | Delegate | Admin
```

---

## 5. API — `/api/v1/workflow/*`

| Controller | المسارات |
|------------|----------|
| WorkflowDefinitionsController | CRUD، Publish، Activate، Deactivate |
| WorkflowController | Start، Approve، Reject، Cancel، History، Pending، UserTasks |
| ApprovalDelegationController | Create، Update، Delete، ActiveDelegations |

---

## 6. Background Jobs

| Job | الجدولة |
|-----|---------|
| `WorkflowEscalationJob` | كل ساعة |
| `DelegationExpiryJob` | كل ساعة |
| `WorkflowReminderJob` | كل 4 ساعات |
| `WorkflowCleanupJob` | أحد 05:00 UTC |

---

## 7. Notifications

- WorkflowStarted
- ApprovalRequested
- ApprovalApproved
- ApprovalRejected
- WorkflowCompleted
- WorkflowEscalated
- DelegationAssigned

---

## 8. Migration

```
AddWorkflowEngine — ⏳ مُنشأة، لم تُطبَّق على قاعدة البيانات
```

لتطبيقها عند الطلب:

```bash
cd Backend/src/GastroErp.Persistence
dotnet ef database update --startup-project ../GastroErp.Presentation
```

---

## 9. Unit Tests

`tests/GastroErp.Domain.UnitTests/Workflow/WorkflowEngineTests.cs`

- إنشاء تعريف Workflow
- النشر وزيادة الإصدار
- بدء Instance + Domain Event
- الرفض وتغيير الحالة
- شروط الانتقال
- التفويض

---

## 10. الربط مع الوحدات الأخرى

أي وحدة (مثل HR Leave، Purchase Order، Expense) تستدعي:

1. `StartWorkflow` مع `ReferenceType` + `ReferenceId`
2. تستمع لـ `WorkflowCompletedEvent` / `WorkflowRejectedEvent` لتحديث حالة الكيان المرجعي

> **ملاحظة:** ربط HR Leave بمحرك Workflow يُنفَّذ في مرحلة تكامل لاحقة عند الحاجة.

---

*آخر تحديث: يوليو 2026*
