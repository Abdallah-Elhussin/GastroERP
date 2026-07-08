# GastroERP Backend Roadmap
# Phase 24
# Background Jobs, Notifications & Integrations

## Current Status

### Completed

- Phase 1–22 — Core through Finance & Accounting
- Phase 23 — Reporting, Analytics & Business Intelligence

Build Status

- ✅ 0 Errors
- ✅ 0 Warnings

---

# Mission

Implement asynchronous infrastructure for long-running tasks, real-time notifications, and third-party integrations without blocking operational APIs.

---

# Architecture Rules

- Clean Architecture — background workers in Infrastructure
- No business logic in Hangfire/job classes — delegate to Application services
- Idempotent job handlers
- Multi-tenancy context propagation in every job
- Retry with exponential backoff
- Dead-letter queue for failed jobs

---

# Background Jobs

## Job Engine

Choose: **Hangfire** (recommended) or .NET `IHostedService` + persistent queue.

Register in `GastroErp.Infrastructure`.

## Job Types

| Job | Trigger | Purpose |
|-----|---------|---------|
| `GenerateScheduledReportJob` | Cron | Run report + email export |
| `FiscalPeriodAutoCloseJob` | Monthly cron | Close expired fiscal periods |
| `LoyaltyPointsExpiryJob` | Daily | Expire unused loyalty points |
| `InventoryReorderAlertJob` | Daily | Notify when stock below reorder level |
| `DeliveryProviderSyncJob` | Every 5 min | Poll external delivery APIs |
| `DatabaseBackupJob` | Nightly | Backup tenant databases |
| `AuditLogArchiveJob` | Weekly | Archive old audit entries |

## Domain

Optional read models:

- `ScheduledJobDefinition`
- `JobExecutionLog`

Migration: `AddBackgroundJobsModule` — only if persistence required.

---

# Notifications

## Channels

- Email (existing `IEmailSender`)
- SMS (`ISmsSender`)
- Push (Firebase / OneSignal adapter)
- In-app (`NotificationInbox` entity)

## Events to Notify

| Event | Recipients |
|-------|-----------|
| Order Completed | Customer (receipt email) |
| Low Stock | Branch manager |
| Shift Variance | Finance manager |
| Delivery Failed | Operations |
| Fiscal Period Closed | Accountant |
| Report Ready | Requesting user |

## Services

- `INotificationDispatcher` (extend existing)
- `NotificationTemplateService` — Arabic/English templates
- `NotificationPreferenceService` — per user/role opt-in

---

# Integrations

## Payment Gateways

- Mada / HyperPay / Tap Payments adapters
- Implement `IPaymentGatewayAdapter` pattern (similar to `IDeliveryProviderAdapter`)

## Delivery Providers

- Extend Phase 20 adapters: Jahez, HungerStation, Ninja
- Webhook endpoints for status updates

## Accounting Export

- Export journals to external ERP (SAP, Odoo) via CSV/API
- `IAccountingExportAdapter`

## ZATCA (Phase 19 extension)

- Async clearance polling job
- Retry failed fiscal submissions

## Webhooks (Outbound)

- `WebhookSubscription` entity
- Dispatch on domain events (OrderCompleted, PaymentCompleted, etc.)
- HMAC signature verification

## Webhooks (Inbound)

- `IntegrationController` for provider callbacks
- Idempotency via `ExternalEventLog`

---

# API Layer

## Controllers

- `ScheduledJobsController` — CRUD job definitions, manual trigger
- `NotificationController` — inbox, mark read, preferences
- `IntegrationController` — webhook subscriptions, provider config
- `WebhookController` — inbound callbacks (no auth / API key)

Routes: `/api/v1/integrations/*`, `/api/v1/notifications/*`

---

# Permissions

- `Jobs.View`, `Jobs.Manage`
- `Notifications.View`, `Notifications.Manage`
- `Integrations.View`, `Integrations.Manage`
- `Webhooks.Manage`

---

# Security

- Encrypt integration credentials (Azure Key Vault / DPAPI)
- Rate limit webhook endpoints
- Validate HMAC on inbound webhooks
- Audit all integration config changes

---

# Performance

- Separate worker process optional (scale Hangfire workers)
- Job concurrency limits per tenant
- Circuit breaker for external API calls

---

# Final Validation

- Build = 0 Errors / 0 Warnings
- Jobs are idempotent
- Tenant context restored in every job scope
- Failed jobs logged and alertable

---

# Estimated Milestones

1. Hangfire setup + Scheduled Report job
2. Notification inbox + email templates
3. Payment gateway adapter (Tap/Mada)
4. Outbound webhooks
5. Inbound provider webhooks + ZATCA polling

Do NOT implement Phase 24 until explicitly requested.
