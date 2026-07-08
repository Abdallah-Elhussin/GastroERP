# GastroERP Backend Roadmap
# Phase 17
# Payments & Cash Management

> ## Current Project Status

### ✅ Completed

- Phase 11A — Backend Foundation
  - Identity
  - Organization
  - Menu
  - Inventory
  - Database Hardening
  - Security & Production Hardening

- Phase 16 — POS Core
  - SalesOrder Aggregate
  - Order Lifecycle
  - CQRS
  - REST API
  - DTOs
  - Validators
  - AutoMapper
  - EF Core Configurations

Build Status

- ✅ Build Succeeded
- ✅ 0 Errors
- ✅ 0 Warnings

---

# Mission

Implement the complete Payments & Cash Management module for GastroERP.

This module will become the financial engine of the POS.

The implementation must integrate naturally with:

- POS Orders
- Inventory
- Identity
- Organization

It must also be fully compatible with future modules:

- Accounting
- CRM
- Loyalty
- Reporting
- Analytics

---

# Mandatory Rules

Before generating any code:

Review the entire solution.

Inspect every existing:

- Entity
- Aggregate Root
- Domain Event
- Value Object
- Enum
- DTO
- Command
- Query
- Handler
- Validator
- Mapping Profile
- Controller
- EF Configuration

Reuse existing implementations.

Never duplicate business concepts.

Never create parallel models.

Always extend existing code first.

The Application Layer remains the Single Source of Truth.

Maintain:

- Clean Architecture
- DDD
- CQRS
- SOLID
- Multi-Tenancy
- Repository Pattern
- Unit of Work

Target:

Build = 0 Errors

Build = 0 Warnings

---

# Aggregate Roots

Implement the following Aggregate Roots.

## Payment

Payment supports:

- Full Payment
- Partial Payment
- Split Payment
- Multiple Payments
- Refund
- Void
- Cancel

A Payment cannot exist without a SalesOrder.

---

## CashRegister

Support:

- Open
- Close
- Suspend
- Resume

Track:

- Opening Balance
- Closing Balance
- Expected Balance
- Actual Balance
- Difference

---

## CashierShift

Support:

- Open
- Active
- Suspended
- Closing
- Closed
- Reconciled

Business Rule:

Only one active shift is allowed per cashier.

---

## CashMovement

Support:

- Cash In
- Cash Out
- Safe Deposit
- Safe Withdrawal
- Petty Cash
- Expense
- Float

Track:

- Amount
- Reason
- User
- Register
- Shift
- Branch
- Tenant

---

## Refund

Support:

- Full Refund
- Partial Refund
- Item Refund

Track:

- Original Payment
- Reason
- User
- Approval
- Timestamp

---

## Payment Allocation

Support:

- Multiple payments for one order.
- One payment allocated across multiple balances if required.

---

# Payment Methods

Support:

- Cash
- Credit Card
- Debit Card
- Mada
- Apple Pay
- Google Pay
- STC Pay
- Bank Transfer
- Gift Card
- Voucher
- Store Credit

Design the solution so future payment gateways can be added without breaking changes.

---

# Enumerations

Review existing enums.

Create only missing enums.

Examples:

- PaymentStatus
- PaymentMethodType
- ShiftStatus
- RegisterStatus
- RefundStatus
- CashMovementType
- ReconciliationStatus

---

# Domain Events

Generate only missing events.

Examples:

- PaymentCompleted
- PaymentFailed
- PaymentVoided
- PaymentRefunded
- RegisterOpened
- RegisterClosed
- ShiftOpened
- ShiftClosed
- CashMovementCreated
- ReconciliationCompleted

---

# Business Rules

Implement and validate:

- Payment amount cannot exceed remaining balance.
- Refund cannot exceed original payment.
- Cancelled orders cannot receive payments.
- Completed orders cannot be cancelled through payment.
- Closed shifts cannot receive payments.
- Closed cash registers cannot receive transactions.
- Only one active register session per device.
- Cash differences above configured limits require manager approval.
- Split payment total must equal outstanding balance.
- Every refund must reference an existing payment.
- Every payment must generate an audit record.

---

# CQRS

Generate only missing components.

Implement:

- Commands
- Queries
- Handlers
- DTOs
- Validators

Never duplicate existing CQRS components.

---

# Application Services

Create services for:

- Payment Processing
- Refund Processing
- Payment Allocation
- Cash Register Management
- Shift Management
- Cash Reconciliation
- Receipt Number Generation

---

# AutoMapper

Review all profiles.

Register every missing mapping.

---

# Validation

Create FluentValidation validators for:

- Create Payment
- Refund Payment
- Void Payment
- Open Register
- Close Register
- Open Shift
- Close Shift
- Cash Movement
- Reconciliation

---

# REST API

Create the following controllers.

## PaymentController

Endpoints:

GET

GET/{id}

GET/order/{orderId}

POST

POST/{id}/refund

POST/{id}/void

---

## CashRegisterController

Endpoints:

Open Register

Close Register

Current Register

Register History

---

## ShiftController

Endpoints:

Open Shift

Close Shift

Suspend Shift

Resume Shift

Current Shift

Shift History

---

## CashMovementController

Endpoints:

Cash In

Cash Out

Expense

Safe Deposit

Safe Withdrawal

Movement History

---

# Permissions

Create new permissions.

Payments:

- Payments.View
- Payments.Create
- Payments.Refund
- Payments.Void
- Payments.Cancel

Cash Registers:

- CashRegister.View
- CashRegister.Open
- CashRegister.Close

Shifts:

- Shift.View
- Shift.Open
- Shift.Close
- Shift.Suspend
- Shift.Resume

Cash Movements:

- CashMovement.View
- CashMovement.Create
- CashMovement.Approve

---

# Persistence

Review Entity Framework configurations.

Ensure:

- Indexes
- Unique Constraints
- Check Constraints
- Foreign Keys
- Delete Behaviors
- Decimal Precision
- RowVersion
- Default Values
- Global Query Filters

Generate only one migration.

Do NOT apply it to SQL Server.

---

# Performance

Optimize queries using:

- AsNoTracking()
- Projection
- Pagination
- Filtering
- Sorting

Avoid unnecessary Include() statements.

---

# Security

Every endpoint must enforce:

- Authentication
- Authorization
- Permission-based Access
- Tenant Isolation
- Branch Isolation

---

# Integration

Integrate with SalesOrder.

Successful payment must:

- Update payment status.
- Update remaining balance.
- Raise Domain Events.
- Prepare future Accounting integration.
- Prepare future Reporting integration.

Do NOT implement Accounting or Reporting yet.

---

# Deliverables

Generate only missing:

- Domain Entities
- Aggregate Roots
- Value Objects
- Enums
- Domain Events
- Commands
- Queries
- Handlers
- DTOs
- Validators
- Application Services
- Controllers
- AutoMapper Profiles
- EF Configurations
- Migration

---

# Final Validation

Verify:

- No duplicate entities.
- No duplicate CQRS.
- No duplicate DTOs.
- No duplicate validators.
- No duplicate controllers.
- No temporary implementations.
- No TODO placeholders.
- No NotImplementedException.

Build Target:

- 0 Errors
- 0 Warnings

Migration must compile successfully.

Do NOT apply the migration.

---

# Completion Report

Generate a detailed report including:

1. Domain Components
2. CQRS Components
3. REST APIs
4. Services
5. Security
6. Database Changes
7. Integration Points
8. Remaining Work

---

# Next Phase

After successfully completing Phase 17:

Generate a new markdown document:

**Phase18_Kitchen_Dining_and_KDS.md**

This document must contain the complete implementation plan for:

- Kitchen Display System (KDS)
- Kitchen Tickets
- Kitchen Stations
- Dining Areas
- Restaurant Tables
- Floor Plans
- Reservations
- Table Management
- Kitchen Routing
- Kitchen Workflow

Do NOT implement Phase 18.

Generate only the implementation instructions, then stop.