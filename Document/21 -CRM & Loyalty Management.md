# GastroERP Backend Roadmap
# Phase 21
# CRM & Loyalty Management

## Current Status

### Completed

- Phase 1–15 — Backend Foundation
- Phase 16 — POS Core
- Phase 17 — Payments & Cash Management
- Phase 18 — Kitchen, Dining & KDS
- Phase 19 — Invoicing, Taxation & Fiscal Compliance
- Phase 20 — Delivery Management

Build

- ✅ 0 Errors
- ✅ 0 Warnings

Pending Migrations

- AddSalesOrderTables
- AddPaymentsAndCashManagement
- AddKitchenDiningAndKDS
- AddInvoicingTaxationAndFiscalCompliance
- AddDeliveryManagement

Do NOT apply migrations yet.

---

# Mission

Implement the complete Customer Relationship Management (CRM) and Loyalty module.

This module must integrate with:

- Sales Orders
- Payments
- Delivery
- Invoicing
- Reporting (future)
- Finance (future)

Do not duplicate customer data already stored elsewhere.

---

# Mandatory Rules

Before implementing:

Review the entire solution.

Reuse existing:

- BaseEntity
- Aggregate patterns
- Domain Events
- ErrorCodes
- AutoMapper Profiles
- Validators
- CQRS conventions
- Authorization model

The Application Layer remains the Single Source of Truth.

Maintain:

- Clean Architecture
- DDD
- CQRS
- SOLID
- Multi-Tenancy

Target:

- Build = 0 Errors
- Build = 0 Warnings

---

# Aggregate Roots

Implement the following aggregates.

## Customer

Track:

- Customer Number
- Full Name
- Mobile
- Email
- Date of Birth
- Gender
- Preferred Language
- Notes
- Status

Statistics:

- Total Orders
- Total Spending
- Average Ticket
- Last Visit
- Last Order
- Favorite Branch

---

## LoyaltyAccount

Track:

- Current Points
- Earned Points
- Redeemed Points
- Expired Points
- Tier

Support:

- Earn
- Redeem
- Adjust
- Expire

---

## LoyaltyTransaction

Track every point movement.

Support:

- Earn
- Redeem
- Manual Adjustment
- Expiration
- Refund Adjustment

---

## MembershipTier

Support:

- Bronze
- Silver
- Gold
- Platinum
- VIP

Each tier defines:

- Required Points
- Discount
- Benefits
- Priority

---

## Coupon

Support:

- Fixed Discount
- Percentage Discount
- Free Item
- Free Delivery

Track:

- Validity
- Usage Limit
- Remaining Uses
- Customer Restrictions

---

## PromotionCampaign

Support:

- Order Discount
- Product Discount
- Category Discount
- Combo Promotion
- Happy Hour
- Buy X Get Y

Track:

- Active Period
- Branches
- Products
- Categories
- Priority
- Stackability

---

## GiftCard

Support:

- Stored Value
- Balance
- Expiration
- Recharge
- Redemption

---

# Enumerations

Create only missing enums.

Examples:

- CustomerStatus
- LoyaltyTier
- LoyaltyTransactionType
- CouponType
- PromotionType
- GiftCardStatus

---

# Domain Events

Generate only missing events.

Examples:

- CustomerCreated
- CustomerUpdated
- LoyaltyPointsEarned
- LoyaltyPointsRedeemed
- TierUpgraded
- CouponIssued
- CouponRedeemed
- PromotionActivated
- GiftCardRedeemed

---

# Business Rules

Implement:

- Customer mobile number must be unique per Tenant.
- Loyalty points are earned only after completed orders.
- Cancelled orders do not generate points.
- Refunded orders reverse earned points.
- Expired points cannot be redeemed.
- Coupon expiration must be validated.
- Coupon usage limit must be enforced.
- Promotions cannot overlap unless explicitly stackable.
- Gift Card balance cannot become negative.

---

# Integration

SalesOrder

- Assign Customer
- Calculate loyalty points
- Apply promotions
- Validate coupons

Payment

- Support Gift Card as payment method.
- Redeem Store Credit.

Invoice

- Print customer information.
- Show earned/redeemed points.

Delivery

- Customer delivery history.

Future Reporting

Prepare read models.

Do NOT implement Reporting.

---

# CQRS

Generate only missing:

- Commands
- Queries
- Handlers
- DTOs
- Validators

Do not duplicate existing CQRS.

---

# Application Services

Implement:

- LoyaltyCalculationService
- PromotionEngine
- CouponValidationService
- GiftCardService
- CustomerStatisticsService

---

# AutoMapper

Register every missing mapping.

---

# Validation

Create FluentValidation validators for:

- Customer
- Loyalty
- Coupon
- Promotion
- Membership
- Gift Card

---

# REST API

Create:

## CustomerController

- CRUD
- Search
- Statistics
- Order History

---

## LoyaltyController

- Balance
- Transactions
- Redeem
- Adjust
- Expire

---

## PromotionController

- CRUD
- Activate
- Deactivate

---

## CouponController

- CRUD
- Validate
- Redeem

---

## GiftCardController

- Issue
- Recharge
- Redeem
- Balance

---

# Permissions

Customer.*

Loyalty.*

Promotion.*

Coupon.*

GiftCard.*

---

# Persistence

Implement:

- EF Configurations
- Indexes
- Unique Constraints
- Check Constraints
- Decimal Precision
- Delete Behaviors
- Global Query Filters

Generate one migration.

Do NOT apply the migration.

---

# Performance

Optimize all queries.

Use:

- AsNoTracking()
- Projection
- Pagination
- Filtering
- Sorting

---

# Security

Enforce:

- Authentication
- Authorization
- Tenant Isolation
- Branch Isolation
- Permission-based Access

---

# Deliverables

Generate only missing:

- Domain
- Enums
- Events
- CQRS
- DTOs
- Validators
- AutoMapper
- Services
- Controllers
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
- No TODO placeholders.
- No NotImplementedException.

Build Target:

- 0 Errors
- 0 Warnings

Migration must compile successfully.

Do NOT apply migrations.

---

# Completion Report

Generate a detailed implementation report including:

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

After successfully completing Phase 21:

Generate a new markdown document:

**Phase22_Finance_Accounting_Integration.md**

This document must contain the complete implementation plan for:

- Chart of Accounts
- Journal Entries
- General Ledger
- Cost Centers
- Fiscal Periods
- Account Posting
- Financial Closing
- Automatic Posting from Sales, Payments, Inventory, and Payroll

Do NOT implement Phase 22.

Generate only the implementation roadmap, then stop.