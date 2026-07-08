# GastroERP Backend Roadmap
# Phase 11A – Milestone 4
# Inventory Completion

> **Current Status**
>
> ✅ Milestone 1 completed — Identity Completion
>
> ✅ Milestone 2 completed — Organization Completion
>
> ✅ Milestone 3 completed — Menu Completion
>
> 🚧 Current Milestone — Inventory Completion

---

# General Instructions

You are the Lead Software Architect of the GastroERP project.

The Backend architecture is considered stable.

The project follows:

- Clean Architecture
- Domain Driven Design (DDD)
- CQRS
- MediatR
- Repository Pattern
- Unit of Work
- AutoMapper
- FluentValidation
- Multi-Tenancy
- SQL Server
- ASP.NET Core .NET 9

Do NOT change the architecture.

Do NOT introduce new design patterns.

Follow the existing coding conventions.

Preserve backward compatibility.

Maintain Build = 0 Errors / 0 Warnings.

---

# IMPORTANT

Before generating ANY code:

Scan the entire solution.

Reuse every existing:

- Entity
- DTO
- Command
- Query
- Handler
- Validator
- AutoMapper Profile
- Controller
- EF Configuration

Never duplicate existing implementations.

Always extend existing functionality before creating new files.

The Application Layer remains the Single Source of Truth.

---

# Objective

Complete the Inventory module to Enterprise Restaurant ERP standards.

No temporary implementations.

No placeholders.

No Dummy Handlers.

No NotImplementedException.

---

# Review Existing Domain

Inspect every Inventory entity.

Including:

- InventoryItem
- Warehouse
- Supplier
- PurchaseOrder
- Recipe
- RecipeItem
- UnitConversion
- AdjustmentReason
- WasteReason
- InventorySettings

Detect missing business rules.

Detect incomplete relationships.

Detect missing API functionality.

---

# Unit Conversion

Complete support for:

- CRUD operations for `UnitConversion`
- Basic conversions (e.g. kg → g, Box → Piece, Bottle → ml)
- Multi-unit measurements per inventory item (purchase unit, stock unit, recipe unit)

---

# Adjustment & Waste Reasons

Complete:

- CRUD operations for `AdjustmentReason` and `WasteReason`
- Default reasons (Damaged, Lost, Expired, Correction)

---

# Inventory Settings

Complete:

- CRUD for `InventorySettings`
- Support for:
  - Negative Stock (Allow/Disallow)
  - Auto Deduction on POS Sale
  - Auto Reservation on Order
  - Costing Method (FIFO, LIFO, Weighted Average)
  - Low Stock Alerts

---

# Queries

Generate advanced queries.

Support:

- Search Inventory Items
- Search Suppliers
- Search Warehouses

Include:

- Filtering
- Pagination
- Sorting
- SearchTerm search

---

# CQRS

Review every Aggregate Root.

Generate missing:

- Commands
- Queries
- DTOs
- Validators
- Handlers

Only when missing.

Never duplicate existing implementations.

---

# Controllers

Every Aggregate Root must expose REST endpoints.

Follow REST standards.

GET

GET/{id}

POST

PUT/{id}

DELETE/{id}

---

# AutoMapper

Review all mapping profiles.

Generate missing mappings.

No DTO without mapping.

---

# FluentValidation

Every Create and Update command must have Validator.

---

# Persistence

Review Entity Framework configurations.

Complete:

- Indexes
- Unique Constraints
- Check Constraints
- Delete Behaviors
- Precision
- Default Values
- Concurrency Tokens

---

# Performance

Optimize every read query.

Use:

- AsNoTracking()
- Projection
- Pagination
- Filtering
- Sorting

Avoid loading unnecessary navigation properties.

---

# Security

Every endpoint must respect:

- Authentication
- Authorization
- Permission-based access
- Tenant Isolation
- Branch Isolation

---

# Deliverables

Generate only missing items.

Deliver:

- Commands
- Queries
- DTOs
- Validators
- Handlers
- Controllers
- AutoMapper Profiles
- EF Configurations
- Migrations (only if required)

---

# Final Validation

Verify:

- No Dummy Handlers
- No NotImplementedException
- No duplicate CQRS
- No duplicate DTOs
- No duplicate Validators
- No duplicate Controllers
- No missing AutoMapper mappings
- No missing EF Configurations
- Build succeeds

Target:

Build = 0 Errors

Build = 0 Warnings

---

# AFTER COMPLETING THIS MILESTONE

Do NOT stop.

Immediately start the next milestone.

Generate a new markdown document named:

Phase11A_Milestone5_SystemSettingsCompletion.md

The document must contain the complete implementation instructions for the System Settings Completion milestone.

The generated document must follow the same structure, level of detail, validation rules, architecture constraints, coding standards, and completion checklist used in this milestone.

Continue this chaining approach so that every completed milestone automatically prepares the next milestone, ensuring the implementation roadmap progresses without requiring additional user prompts.
