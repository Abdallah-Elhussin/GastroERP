# GastroERP — Inventory Module Roadmap

## Architecture (immutable)
```
InventoryItem → Recipe → Product
```
`ProductCatalogDefinition` = coordinator only. Do not merge entities.

## Phase A — Infrastructure ✅
- [x] Lazy inventory feature routes (`inventory.routes.ts`)
- [x] Permission guards (`Inventory.View` / `Manage`, `Warehouse.View`, `Stock.View`)
- [x] Nested sidebar navigation + favorites
- [x] Breadcrumb page shell
- [x] Skeleton / empty / error states
- [x] AR/EN i18n keys
- [x] Dashboard entry + placeholders for B–I screens

## Phase B — Master Data ✅
- [x] Categories CRUD (unlimited levels via ParentCategoryId)
- [x] Units CRUD (code, decimals, base unit)
- [x] Warehouses CRUD + types + operation permissions
- [x] API endpoints on CatalogController / WarehouseController
- [x] EF migration `ExpandInventoryMasterDataPhaseB`

## Phase C — Product Master (14 tabs) ✅
- [x] Product Master shell at `/catalog/master` (+ `/:id`)
- [x] 14 tabs orchestrated via ProductCatalogDefinition
- [x] Wired to InventoryItem / Recipe / Product through existing catalog section APIs
- [x] Type-aware tab visibility (RequiresInventory / Recipe / Product / Pricing)
- [x] Taxes / Logistics / Accounting extras on coordinator JSON (domain fields later)
- [x] Catalog list + sidebar entry points

## Phase D — Product Details ✅
- [x] Overview card (image, barcode, QR)
- [x] Per-warehouse stock (On Hand / Reserved / Available / Ordered / Incoming)
- [x] Stock movement timeline
- [x] Purchase / Sales / Price history
- [x] Attachments gallery from catalog media URLs
- [x] APIs: stock-by-warehouse, movements, purchase-history, sales-history, catalog-by-item, structured price-history

## Phase E — Inventory Operations ✅
- [x] Operations hub at `/inventory/transactions` (ledger / transfer / adjust / waste / GRN / count / purchase return)
- [x] GoodsReceipt controller + list/create/line/confirm APIs
- [x] Transfer / Adjust / Waste list + confirm/complete endpoints on StockController
- [x] Query handlers for GR / transfers / adjustments / waste
- [x] Frontend repository + service wiring
- [ ] Deferred: Sales Return, Production issue

## Phase F — Dashboard (full KPIs/charts) ✅
- [x] Dedicated `GET /inventory/dashboard` summary API
- [x] KPI cards (items, warehouses, open transfers/counts, reservations, draft GRN)
- [x] Alerts strip + warehouse summary + recent activity
- [x] Grouped quick links (reorganized nav)
- [ ] Advanced charts (value trends) — optional enhancement later

## Phase G — Reports ✅
- [x] Reports hub at `/inventory/reports`
- [x] Wired to existing InventoryReports APIs (`/reports/inventory/*`)
- [x] Tabs: balance, valuation, movements, waste, adjustments, consumption, purchases, suppliers
- [x] Date filters + CSV export
- [x] Permission `InventoryReports.View` (+ auth aliases)

## Phase H — UX Enhancements ✅
- [x] Barcode/SKU scan mode on items list (Enter → details)
- [x] Keyboard shortcuts: `/` search, `Ctrl+N` new item, `Ctrl+B` barcode mode
- [x] Real CSV export for filtered items
- [x] Reservation tab in Operations hub (list / create / release / expire)

## Phase I — Inventory Configuration
- [x] GET/PUT `/api/v1/inventory/settings` (InventorySetting aggregate)
- [x] Costing method, default warehouse, negative stock, auto-reserve/issue/SKU
- [x] Batch & expiry tracking flags
- [x] Settings UI at `/inventory/settings`

## Phase J — Master Data Extensions
- [x] Warehouse zones / shelves / bins API + structure UI on warehouses page
- [x] Brands CRUD (`/inventory/brands`)
- [x] Manufacturers CRUD (`/inventory/manufacturers`)
- [x] Attributes CRUD + list values (`/inventory/attributes`)
- [x] Price lists CRUD + lines (`/inventory/price-lists`)
- [x] Tax groups UX reusing `/sales/tax-groups`
- [x] Extensions hub at `/inventory/extensions` + sidebar entry
- [x] EF migration `InventoryMasterDataPhaseJ`

## Inventory roadmap complete (A–J)
## Phase K — Unified Inventory Movement Pipeline ✅
- [x] Single posting entry: `IInventoryMovementPipeline.ApplyMovementAsync`
- [x] Positive quantities only; direction via `InventoryMovementType` (IN/OUT/TRO/TRI/ADJ/REV)
- [x] Weighted Average costing engine only (FIFO/StandardCost enums retained, not executed)
- [x] Confirm/Complete/Approve handlers post via pipeline (GR, GI, Transfer, Adjust, Waste, Count, Purchase Return, Opening Balance)
- [x] Transfer ship (TRO) / complete (TRI) / cancel (REV)
- [x] Reservation reserve/release/fulfill via pipeline
- [x] Sales auto-reserve wired to pipeline
- [x] EF migration `InventoryMovementPipeline` (+ balance seed / movement backfill)
- [x] Immutable ledger — no deletes; cancellation creates REV

Deferred outside this roadmap: Sales Return, Production issue/receipt UI, Angular screens for Goods Issue / Opening Balance.
