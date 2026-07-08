# Phase 28 — Workflow Integration Across Modules

## الهدف

دمج Workflow Engine مع جميع وحدات GastroERP بحيث تصبح الموافقات مركزية وقابلة لإعادة الاستخدام، مع إزالة أي منطق موافقات موجود داخل الوحدات نفسها.

---

# قواعد التنفيذ

- عدم وضع أي منطق Approval داخل الوحدات.
- جميع الوحدات تستخدم WorkflowEngine.
- جميع الأحداث تعتمد على Domain Events.
- الحفاظ على Clean Architecture وDDD.
- دعم Multi-Tenant.
- Build يجب أن ينتهي بـ 0 Errors / 0 Warnings.

---

# 1. HR Integration

ربط العمليات التالية:

- Leave Request
- Overtime Request
- Loan Request
- Salary Advance
- Resignation
- Promotion
- Transfer
- Recruitment Approval
- Performance Review Approval

عند إنشاء الطلب:

- StartWorkflow()

عند اكتمال Workflow:

- تنفيذ العملية الفعلية
- تحديث الحالة
- إرسال Notification

---

# 2. Purchasing Integration

ربط:

- Purchase Request
- Purchase Order
- Supplier Approval
- Vendor Registration

---

# 3. Finance Integration

ربط:

- Journal Approval
- Payment Voucher
- Receipt Voucher
- Budget Approval
- Expense Approval

---

# 4. Inventory Integration

ربط:

- Stock Adjustment
- Stock Transfer
- Stock Count
- Item Creation

---

# 5. CRM Integration

ربط:

- Customer Credit Limit
- Customer Registration
- Discount Approval
- Refund Approval

---

# 6. POS Integration

ربط:

- Refund
- Void Invoice
- Price Override
- Cash Drawer Adjustment

---

# 7. Notifications

إضافة:

- WorkflowAssigned
- WorkflowCompleted
- WorkflowCancelled
- WorkflowReturned

---

# 8. Domain Events

إنشاء أحداث تكامل مثل:

- LeaveWorkflowCompleted
- PurchaseWorkflowCompleted
- JournalWorkflowCompleted
- RefundWorkflowCompleted

---

# 9. Background Jobs

إضافة:

- WorkflowRetryJob
- WorkflowTimeoutJob

---

# 10. API

إضافة Endpoints:

- GetWorkflowStatus
- GetWorkflowTimeline
- RestartWorkflow
- ReturnToPreviousStep

---

# 11. Permissions

إضافة صلاحيات:

- Workflow.Restart
- Workflow.Return
- Workflow.ViewTimeline

---

# 12. Health Check

تحديث WorkflowEngineHealthCheck ليتحقق من:

- Queue
- Pending Workflows
- Escalations
- Delegations

---

# 13. Unit Tests

اختبارات تكامل تغطي:

- HR + Workflow
- Purchasing + Workflow
- Finance + Workflow
- Inventory + Workflow
- CRM + Workflow
- POS + Workflow

---

# 14. Documentation

إنشاء:

Document/28 — Workflow Integration.md

وتحديث:

- Document/README.md
- Document/00 — فهرس التوثيق وملخص المنصة.md

---

# معايير القبول

- جميع الوحدات تستخدم Workflow Engine.
- عدم وجود منطق موافقات مكرر داخل الوحدات.
- جميع Domain Events تعمل.
- جميع Notifications تعمل.
- جميع Background Jobs تعمل.
- جميع Unit Tests ناجحة.
- Build:
  - 0 Errors
  - 0 Warnings

---

# النتيجة المتوقعة

بعد إكمال Phase 28 ستصبح جميع وحدات GastroERP (HR، Purchasing، Finance، Inventory، CRM، POS) تعتمد على محرك Workflow مركزي، مما يوفر نظام موافقات موحد، قابل للتخصيص، وسهل التوسع دون تكرار منطق الأعمال داخل كل وحدة.