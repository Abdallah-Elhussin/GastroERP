# Phase 26 — Human Resources & Workforce Management (Completion)

## الهدف

استكمال وحدة الموارد البشرية (HR) لتصبح جاهزة للإنتاج، مع دمجها بالكامل مع الوحدات الحالية في GastroERP، والمحافظة على مبادئ:

- Clean Architecture
- Domain-Driven Design (DDD)
- CQRS
- SOLID
- Multi-Tenant

> هذه المرحلة تستكمل ما تم بناؤه مسبقًا، ولا تعيد تصميم الكود الحالي.

---

# Build Target

- Build = 0 Errors
- Build = 0 Warnings
- Production Ready

---

# الحالة الحالية

## تم إنجازه

### Domain

- Employee
- Employment Contracts
- Employment History
- Employee Documents
- Emergency Contacts
- Attendance
- Leave Management
- Scheduling
- Payroll
- Performance
- Recruitment
- Training

---

### Business Rules

- قاعدة الاسم الثلاثي
- Domain Validation
- FluentValidation
- Multi-Tenant Support

---

### Persistence

- DbSets
- Entity Configurations
- ApplicationDbContext Integration

---

### Application

- DTOs
- Services
- CQRS
- Validators

---

### Finance Integration

ترحيل الرواتب تلقائياً إلى المحاسبة

```
Salary Expense (5100)

↓

Salaries Payable (2200)
```

---

### API

تم إنشاء Controllers لجميع وحدات HR.

---

# المطلوب استكماله

---

# 1. Permissions

إضافة جميع صلاحيات الموارد البشرية.

## Employees

- Hr.Employee.View
- Hr.Employee.Create
- Hr.Employee.Update
- Hr.Employee.Delete

---

## Attendance

- Hr.Attendance.View
- Hr.Attendance.Manage

---

## Scheduling

- Hr.Schedule.View
- Hr.Schedule.Manage

---

## Leave

- Hr.Leave.View
- Hr.Leave.Request
- Hr.Leave.Approve
- Hr.Leave.Reject

---

## Payroll

- Hr.Payroll.View
- Hr.Payroll.Generate
- Hr.Payroll.Approve
- Hr.Payroll.Post

---

## Performance

- Hr.Performance.View
- Hr.Performance.Manage

---

## Recruitment

- Hr.Recruitment.View
- Hr.Recruitment.Manage

---

## Training

- Hr.Training.View
- Hr.Training.Manage

---

## Self Service

- Hr.SelfService.Use

---

## Dashboard

- Hr.Dashboard.View

---

# 2. Dependency Injection

تسجيل جميع الخدمات داخل Infrastructure.

يشمل:

- EmployeeService
- AttendanceService
- SchedulingService
- LeaveService
- PayrollService
- PerformanceService
- RecruitmentService
- TrainingService
- DashboardService
- SelfService

---

# 3. Domain Events Integration

ربط جميع أحداث HR مع MediatR.

## Events

- EmployeeHiredEvent
- EmployeeTerminatedEvent
- LeaveRequestedEvent
- LeaveApprovedEvent
- LeaveRejectedEvent
- PayrollGeneratedEvent
- PayrollPostedEvent
- PerformanceEvaluatedEvent
- TrainingCompletedEvent

---

## Event Handlers

- Notifications
- Audit Logs
- Dashboard Refresh
- Finance Posting
- Scheduled Jobs

---

# 4. Notifications

الربط مع Phase 24.

## Employee

- Welcome Employee
- Contract Expiry
- Birthday
- Probation Completed

---

## Leave

- Leave Requested
- Leave Approved
- Leave Rejected

---

## Payroll

- Payslip Ready
- Payroll Posted

---

## Training

- Training Assigned
- Certification Expired

---

# 5. Background Jobs

إضافة المهام المجدولة.

## Daily

- Attendance Summary
- Leave Balance Update

---

## Weekly

- Missing Attendance Report
- Overtime Summary

---

## Monthly

- Payroll Generation
- Payroll Posting
- Performance Reminder

---

## Alerts

- Contract Expiry
- Probation Expiry
- Certification Expiry

---

# 6. Audit

تسجيل العمليات التالية:

- Employee Created
- Employee Updated
- Employee Deleted
- Attendance Changes
- Leave Approval
- Payroll Posting
- Performance Evaluation

---

# 7. Health Checks

إضافة فحوصات النظام:

- HR Services
- Payroll Services
- Notification Services

---

# 8. API Documentation

تحديث Swagger.

يشمل:

- Endpoints
- Examples
- Response Codes
- Authorization

---

# 9. Migration

إنشاء Migration فقط.

```
AddHrWorkforceModule
```

> لا يتم تطبيق Migration تلقائياً.

---

# 10. Testing

## Unit Tests

- Employee
- Attendance
- Leave
- Payroll
- Performance
- Recruitment
- Training

---

## Integration Tests

- HR APIs
- Finance Integration
- Notifications
- Scheduled Jobs

---

# 11. Performance

- AsNoTracking Queries
- Pagination
- Caching for Dashboard
- Async Processing

---

# 12. Security

- Permission Validation
- Tenant Isolation
- Audit Logging
- Encrypted Sensitive Data

---

# Deliverables

## Domain

- HR Domain Complete

---

## Application

- Services Complete
- CQRS Complete
- Validators Complete

---

## Infrastructure

- DI Registration
- Notifications
- Scheduled Jobs
- Health Checks

---

## Persistence

- Migration
- Configurations
- DbContext

---

## Presentation

- Controllers
- Swagger
- Permissions

---

# Definition of Done

- جميع صلاحيات HR مكتملة.
- جميع الخدمات مسجلة في Dependency Injection.
- جميع Domain Events مرتبطة بـ MediatR.
- جميع Notifications تعمل.
- جميع Scheduled Jobs تعمل.
- Swagger محدث.
- Migration منشأة.
- Unit Tests ناجحة.
- Integration Tests ناجحة.
- Build = 0 Errors.
- Build = 0 Warnings.

---

# Phase 26 Complete

بعد إكمال هذه العناصر تصبح وحدة الموارد البشرية في GastroERP مكتملة وتشمل:

- Employee Management
- Attendance Management
- Shift Scheduling
- Leave Management
- Payroll
- Performance Management
- Recruitment
- Training Management
- Employee Self-Service
- HR Dashboard
- Finance Integration
- Notifications
- Background Jobs
- Audit Trail
- Multi-Tenant Support

وتصبح جاهزة للانتقال إلى المرحلة التالية دون وجود أي نواقص معمارية أو تشغيلية.