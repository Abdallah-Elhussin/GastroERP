# GastroERP Enterprise Software Architecture & Development Guide

# Volume I — Foundation

## 03-BUSINESS_RULES.md

Version: 1.0

Status: Draft

---

# Introduction

This document defines the official business rules governing GastroERP. These rules are the source of truth for Domain-Driven Design, application services, APIs, validation, and testing.

---

# 1. Tenant Rules

- Every Tenant owns its own business data.
- Users cannot access another Tenant's data.
- Each Tenant has one active subscription.
- Subscription limits determine available modules, branches, users and devices.

---

# 2. Company Rules

- A Tenant may own multiple Companies.
- Every Company belongs to one Tenant.
- Company VAT settings are independent.

---

# 3. Branch Rules

- Every Branch belongs to one Company.
- Branches have independent warehouses, kitchens, menus, printers and devices.
- Branches can be disabled without deleting history.

---

# 4. User Rules

- Every user belongs to one Tenant.
- Users may work in multiple branches if authorized.
- Roles and permissions determine access.
- All user activities are audited.

---

# 5. POS Rules

- Orders can be Dine-In, Takeaway, Delivery, Drive-Thru or QR Ordering.
- Offline sales are always allowed.
- Closed orders cannot be modified.
- Refunds require permission.

---

# 6. Kitchen Rules

- Orders are routed to stations according to recipe configuration.
- Kitchen status changes are timestamped.
- Preparation time is recorded for KPI calculations.

---

# 7. Inventory Rules

- Stock movements are immutable.
- Recipes consume ingredients automatically.
- Waste must be recorded with a reason.
- Negative inventory depends on branch configuration.

---

# 8. Accounting Rules

- Every financial operation creates journal entries.
- Chart of Accounts follows double-entry accounting.
- VAT is calculated using configured tax profiles.

---

# 9. Synchronization Rules

- SQLite is the local operational database.
- SQL Server is the master cloud database.
- POS sales are the source of truth while offline.
- Configuration changes originate from the cloud.
- Every synchronized record has timestamps, device ID and synchronization status.

---

# 10. Security Rules

- JWT authentication.
- Refresh Tokens.
- RBAC permissions.
- Audit logs for sensitive operations.
- Soft delete for business entities whenever applicable.

---

# 11. Reporting Rules

Reports are generated from synchronized transactional data and include:
- Sales
- Inventory
- Food Cost
- Profitability
- Staff Performance
- Kitchen KPIs
- Customer Analytics

---

# Architecture Decision

Business rules are implemented inside the Domain layer and enforced by Aggregates. Infrastructure must never bypass these rules.

---

End of Document
