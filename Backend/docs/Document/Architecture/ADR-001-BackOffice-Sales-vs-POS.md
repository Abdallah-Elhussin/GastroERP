# ADR-001 — فصل المبيعات الإدارية عن نقطة البيع (POS)

Status: Approved  
Date: 2026-07-18  

## Context

GastroERP يحتوي على كيان `SalesOrder` مخصص لطلبات POS (كاشير، جهاز، مطبخ، وردية).  
خطة وحدة المبيعات الإدارية تتطلب دورة مستندات مستقلة: عرض سعر، أمر بيع، تسليم، فاتورة، مرتجع.

## Decision

1. وحدة **Back Office Sales** مستقلة تماماً عن POS.
2. لا يُعاد استخدام `SalesOrder` (POS) كأمر بيع إداري.
3. Aggregates الإدارية تُنشأ تحت `Domain.Entities.Sales.BackOffice`.
4. بادئة API: `/api/v1/back-office-sales`.
5. صلاحيات: `BackOfficeSales.*` منفصلة عن `Sales.*` (POS).
6. التحليل القديم يُستخدم لقواعد العمل فقط — بدون نقل كود قديم.

## Aggregates المعتمدة (Phase 0)

| المفهوم | الاسم |
|---------|--------|
| فاتورة مبيعات | `BackOfficeSalesInvoice` |
| أمر بيع | `BackOfficeSalesOrder` *(لاحقاً)* |
| عرض سعر | `SalesQuotation` *(لاحقاً)* |
| سند تسليم | `SalesDeliveryNote` *(لاحقاً)* |
| مرتجع | `BackOfficeSalesReturn` *(لاحقاً)* |

## Consequences

- ترحيل محاسبي/مخزني عند `Post` فقط بعد `Approved`.
- التكامل مع POS عبر Interfaces مشتركة (أصناف، عملاء، مخزون) فقط.
- المستند المرجعي: `Modules/BackOffice-Sales-Module-Plan.md`.
