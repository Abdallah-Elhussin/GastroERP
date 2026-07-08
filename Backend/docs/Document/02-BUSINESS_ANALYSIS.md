# GastroERP Enterprise Software Architecture & Development Guide

# Volume I — Foundation

## Document: 02-BUSINESS_ANALYSIS.md

Version: 1.0

Status: Draft

---

# 1. Business Analysis

## Purpose

This document defines the business requirements, operational workflows, business rules, and stakeholders for GastroERP. It serves as the foundation for Domain-Driven Design (DDD), database design, APIs, and implementation.

---

# 2. Business Goals

- Operate restaurants with or without Internet connectivity.
- Unify ERP and POS in one platform.
- Support SaaS and On-Premise deployments.
- Support single-branch and enterprise restaurant chains.
- Ensure Saudi ZATCA compliance.
- Provide accurate financial and inventory tracking.
- Support future AI integration.

---

# 3. Stakeholders

## Business Owner
- Manage companies, subscriptions, branches, pricing, reports.

## Branch Manager
- Configure branch settings.
- Monitor sales and staff.

## Cashier
- Create orders.
- Receive payments.
- Print receipts.
- Continue working offline.

## Waiter
- Manage tables.
- Send orders to kitchen.
- Split and merge bills.

## Kitchen Staff
- View Kitchen Display System (KDS).
- Update order status.
- Track preparation time.

## Inventory Officer
- Manage warehouses.
- Receive stock.
- Transfers.
- Stock counts.
- Waste management.

## Accountant
- Journals.
- General Ledger.
- VAT.
- Financial statements.

## Customer
- Dine-in.
- Takeaway.
- Delivery.
- QR Ordering.
- Loyalty.

---

# 4. Restaurant Business Processes

1. Customer arrives.
2. Table assigned.
3. Order created.
4. Kitchen receives order.
5. Food prepared.
6. Waiter serves.
7. Payment processed.
8. Invoice generated.
9. Inventory deducted.
10. Accounting entries created.
11. Reports updated.

---

# 5. Sales Channels

- Dine-In
- Takeaway
- Delivery
- Drive-Thru
- QR Self Ordering
- Mobile Ordering
- Third-party Delivery Platforms

---

# 6. Business Rules

## Orders

- Every order belongs to one Tenant.
- Every order belongs to one Company.
- Every order belongs to one Branch.
- Every order has one Order Type.
- Order status changes are audited.
- Closed orders cannot be edited.
- Cancelled orders require permission.

## Inventory

- Recipes consume ingredients.
- Negative inventory is configurable.
- Waste must be recorded.
- Stock movements are immutable.

## Kitchen

- Orders are prioritized.
- Preparation times are measured.
- Station routing is recipe-driven.

## Accounting

- Every financial transaction generates journal entries.
- VAT is calculated according to configured tax rules.

---

# 7. Offline First Strategy

## Local Database

SQLite stores:

- Orders
- Payments
- Customers
- Menu
- Tables
- Users
- Devices

## Synchronization

POS -> SQLite -> Local Queue -> REST API -> SQL Server

Conflict Resolution:

- POS is source of truth for sales.
- Cloud is source of truth for configuration.

---

# 8. Hybrid Multi-Tenant

Platform Database

- Tenants
- Subscriptions
- Licensing
- Global Configuration

Tenant Database

- POS
- Inventory
- Kitchen
- Accounting
- CRM
- HR

---

# 9. Success KPIs

- Average order time
- Kitchen preparation time
- Table turnover
- Food cost %
- Waste %
- Sales per hour
- Customer retention
- Inventory accuracy
- Synchronization success rate
- API response time

---

# 10. Deliverables

This document will drive:

- Domain Model
- Context Map
- Aggregates
- Database Design
- API Contracts
- Frontend Design
- Testing Strategy

---

End of Document
