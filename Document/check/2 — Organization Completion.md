# Milestone 2 — Organization Completion

## Objective

Complete the Organization module to enterprise-grade quality.

The project already follows:

- Clean Architecture
- DDD
- CQRS
- MediatR
- Repository Pattern
- Unit of Work
- AutoMapper
- FluentValidation
- Multi-Tenant
- SQL Server
- ASP.NET Core .NET 9

Do NOT change architecture.

Do NOT introduce new patterns.

Follow existing coding style.

Maintain Build = 0 Errors / 0 Warnings.

---

# Scope

Review the entire Organization bounded context.

Inspect all existing entities including (but not limited to):

- Tenant
- Organization
- Company
- Branch
- Department
- BranchDevice
- OrganizationSettings
- Subscription
- SubscriptionPlan
- Feature
- FeatureLimit

Determine which entities already exist and which require completion.

Do NOT duplicate existing entities.

---

# Organization Settings

Implement complete management for:

- Company Name
- Legal Name
- Commercial Registration
- Tax Number
- Currency
- Language
- Time Zone
- Date Format
- Number Format
- Logo
- Theme
- Address
- Contact Information
- Business Hours

Create:

- Commands
- Queries
- DTOs
- Validators
- Handlers
- API Endpoints

---

# Branch Management

Review Branch module.

Complete missing functionality:

- Activate Branch
- Deactivate Branch
- Archive Branch
- Restore Branch

Support:

- Default Branch

- Branch Status

- Branch Code

- Contact Information

- Working Hours

---

# Department Management

Complete:

CRUD

Search

Filtering

Sorting

Pagination

Validation

---

# Device Management

Review BranchDevice.

Support:

- Assign Device

- Unassign Device

- Activate Device

- Deactivate Device

- Device Identifier

- Device Type

- Device Status

---

# Subscription Management

Review subscription system.

Support:

- Trial

- Active

- Suspended

- Expired

- Cancelled

Implement:

Create Subscription

Renew Subscription

Suspend Subscription

Resume Subscription

Cancel Subscription

Extend Subscription

---

# Subscription Plans

Support:

CRUD

Features

Limits

Maximum Branches

Maximum Users

Maximum Devices

Storage Limit

API Limit

---

# Feature Management

Support feature flags.

Examples:

POS

Inventory

CRM

HR

Reports

Kitchen Display

Delivery

Reservations

Finance

Analytics

Enable / Disable

Limits

---

# Queries

Generate missing queries.

Examples:

GetOrganization

GetOrganizationSettings

GetSubscription

GetPlans

GetBranches

SearchBranches

SearchDepartments

SearchDevices

---

# DTOs

Generate DTOs for every public API.

No Entity should be returned directly.

---

# Validators

Every Create and Update Command must use FluentValidation.

---

# AutoMapper

Review all mapping profiles.

Generate missing mappings.

---

# Controllers

Expose REST endpoints.

Follow conventions:

GET

GET/{id}

POST

PUT/{id}

DELETE/{id}

---

# Persistence

Review Entity Framework configurations.

Add if missing:

Indexes

Unique Constraints

Check Constraints

Delete Behaviors

Precision

Concurrency

Default Values

---

# Performance

Optimize all read queries.

Use:

AsNoTracking()

Projection

Pagination

Filtering

Searching

Sorting

---

# Security

Every endpoint must enforce:

Authentication

Authorization

Tenant Isolation

Branch Isolation

Permission-based access

---

# Final Validation

Verify:

- No Dummy Handlers

- No NotImplementedException

- No duplicate CQRS

- No missing DTOs

- No missing Validators

- No missing AutoMapper mappings

- No missing Controllers

- No missing EF Configurations

- Build succeeds

Target:

Build = 0 Errors

Build = 0 Warnings

At the end, generate a completion report listing:

- Completed Features

- Remaining Gaps (if any)

- Architectural Notes

Do NOT start Menu module.

Stop after Organization module is fully completed.