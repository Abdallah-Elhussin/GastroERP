# GastroERP — Inventory Module Architecture Document

# Part 07 — API Design, Frontend Architecture, Dashboard

**Continues from Part 06 · Sections 20–22**

---

# 20. API Design

## 20.1 Conventions

| Topic | Convention |
|-------|------------|
| Base | `/api/v{version}/inventory/...` |
| Versioning | ASP.NET ApiVersioning (`1.0`) |
| Auth | JWT Bearer + `HasPermission` policies |
| Tenant | `ITenantResolver` — not trusted from body |
| Success | `200 OK` with DTO or empty |
| Business failure | `422` with `{ error, code }` |
| Not found | `404` |
| Paging | Body = items[]; headers `X-Pagination-*` |

## 20.2 Controllers

| Controller | Responsibility |
|------------|----------------|
| CatalogController | Categories, Units |
| ItemController | Items + product-details reads |
| WarehouseController | Warehouses |
| SupplierController | Suppliers |
| PurchaseController | Purchase Orders |
| GoodsReceiptController | GRN |
| StockController | Ledger, Transfers, Adjustments, Waste |
| StockCountController | Counts |
| PurchaseReturnController | Returns |
| ReservationController | Reservations |
| RecipeController | Recipes |

## 20.3 Endpoint Map

### Categories & Units (`CatalogController`)

| Method | Path | Permission |
|--------|------|------------|
| GET | `/inventory/categories` | Inventory.View |
| POST | `/inventory/categories` | Inventory.Manage |
| GET/PUT | `/inventory/categories/{id}` | View/Manage |
| POST | `/inventory/categories/{id}/activate\|deactivate` | Manage |
| GET/POST | `/inventory/units` | View/Manage |
| PUT | `/inventory/units/{id}` | Manage |
| POST | `/inventory/units/{id}/activate\|deactivate` | Manage |

### Items (`ItemController`)

| Method | Path | Permission |
|--------|------|------------|
| GET/POST | `/inventory/items` | View/Manage |
| GET/PUT | `/inventory/items/{id}` | View/Manage |
| GET | `/inventory/items/{id}/stock-by-warehouse` | View |
| GET | `/inventory/items/{id}/movements` | View |
| GET | `/inventory/items/{id}/purchase-history` | View |
| GET | `/inventory/items/{id}/sales-history` | View |

### Warehouses

| Method | Path | Permission |
|--------|------|------------|
| GET/POST | `/inventory/warehouses` | Warehouse.View / Create |
| GET/PUT | `/inventory/warehouses/{id}` | View/Update |
| POST | `.../activate\|deactivate` | Activate |

### Purchases

| Method | Path | Permission |
|--------|------|------------|
| GET/POST | `/inventory/purchases` | Purchase.View/Create |
| GET | `/inventory/purchases/{id}` | View |
| POST | `.../approve\|cancel\|reject\|close` | Approve/Cancel |

### Goods Receipts

| Method | Path | Permission |
|--------|------|------------|
| GET/POST | `/inventory/goods-receipts` | Inventory.View/Manage |
| GET | `/inventory/goods-receipts/{id}` | View |
| POST | `.../{id}/lines` | Manage |
| POST | `.../{id}/confirm` | Manage |

### Stock Ops (`StockController`)

| Method | Path | Permission |
|--------|------|------------|
| GET | `/inventory/stock` | Stock.View |
| GET/POST | `/inventory/stock/transfers` | View/Transfer |
| POST | `.../transfers/{id}/lines` | Transfer |
| POST | `.../transfers/{id}/complete` | Transfer |
| GET/POST | `/inventory/stock/adjustments` | View/Adjust |
| POST | `.../adjustments/{id}/confirm` | Adjust |
| GET/POST | `/inventory/stock/waste` | View/Waste |
| POST | `.../waste/{id}/confirm` | Waste |
| POST | `/inventory/stock/transfer\|adjust\|waste` | Legacy aliases |

### Counts / Returns / Reservations / Recipes

Documented analogously under their controllers (`stock-counts`, `purchase-returns`, `reservations`, `recipes`).

## 20.4 Request / Response Models

- Commands accept DTOs in `InventoryDtos.cs` / `InventoryTransactionDtos.cs`  
- `TenantId` on create DTOs overwritten by server  
- Records preferred for immutability  
- Paged lists return arrays  

## 20.5 Error Handling

| Code pattern | HTTP | Example |
|--------------|------|---------|
| `*NotFound` | 404 | GoodsReceiptNotFound |
| Business rule | 422 | NoLines, InvalidStatusTransition |
| Validation | 400 (pipeline) | FluentValidation |

## 20.6 Versioning Strategy

- Breaking DTO changes → `v2`  
- Additive fields → stay `v1`  
- Deprecate legacy `/stock/transfer` aliases after clients migrate  

## 20.7 Idempotency (Target Headers)

`Idempotency-Key` on confirm endpoints for offline/POS gateways.

---

# 21. Frontend Architecture

## 21.1 Stack

- Angular 18 standalone components  
- Signals for local state (`InventoryService` signals)  
- Lazy routes `INVENTORY_ROUTES`  
- Material icons  
- i18n via `LanguageService` + `I18N_TRANSLATIONS`  
- Repository pattern: `InventoryRepository` ← `RestInventoryRepository`  

## 21.2 Feature Structure

```text
features/inventory/
  inventory.routes.ts
  inventory.component.*          # items list
  inventory-item-form.*
  pages/
    inventory-dashboard.page.*
    inventory-categories.page.*
    inventory-units.page.*
    inventory-warehouses.page.*
    inventory-product-details.page.*
    inventory-operations.page.*  # Phase E hub
    inventory-placeholder.page.* # reports/settings
  shared/
    inventory-page-shell.component.ts
    inventory-skeleton|empty|error
    inventory-favorites.service.ts
```

## 21.3 Routing & RBAC

- `permissionGuard` + `data.requiredPermission`  
- Auth aliases map coarse UI permissions to fine-grained API permissions  

## 21.4 State Management Approach

| Concern | Pattern |
|---------|---------|
| Master lists | `InventoryService` signals + load* methods |
| Operations hub | Page-local signals per tab |
| Product Master | Page orchestrates catalog section APIs |
| Cross-feature | HTTP repositories; no global inventory store required |

## 21.5 Localization & RTL

- Keys `inv.*`, `inv.ops.*`, `inv.wh.type.*`  
- AR/EN dictionaries  
- Shell layout uses logical `start` alignment for RTL readiness  

## 21.6 Theming

- CSS variables (`--bg-surface`, `--text-primary`, `--border-color`)  
- Dark mode via existing app theme tokens (inventory pages inherit)  

## 21.7 Lazy Loading

Inventory feature loaded from app routes under `/inventory` — reduces initial bundle.

## 21.8 UX Building Blocks

| Block | Role |
|-------|------|
| Page shell | Breadcrumb + title + actions slot |
| Skeleton | Loading |
| Empty / Error | Zero and failure states |
| Favorites | Dashboard pins |

## 21.9 Product Master (Catalog Feature)

- `/catalog/master` — 14 tabs  
- Type-aware visibility  
- Links back to inventory details  

## 21.10 Frontend ↔ API Mapping

`RestInventoryRepository` encapsulates:

- Empty tenant GUID placeholder (server overwrites)  
- Multi-step create (transfer: create→line→complete)  
- Warehouse type number↔string mapping  

---

# 22. Dashboard

## 22.1 Current State (Phase A)

- Title/subtitle, quick links, favorites, roadmap hint  
- Basic KPIs: total/active products, low stock, warehouses (from client aggregates)  

## 22.2 Target Dashboard (Phase F)

### KPIs

| KPI | Definition |
|-----|------------|
| Total SKUs | Active InventoryItems |
| Low Stock | Available ≤ ReorderLevel |
| On Hand Value | Σ OnHand * UnitCost (valuation method) |
| Waste Value MTD | Σ Waste movements * cost |
| Open Transfers | Status Draft/InTransit |
| Open Counts | Not Completed |
| Reservations Expiring 24h | Active near expiry |

### Charts

- Stock value by warehouse  
- Waste trend (30 days)  
- Top moving items  
- Receipts vs issues  

### Alerts

- Reorder reached  
- Batch expiring 7/30 days  
- Negative available (if allowed)  
- Count overdue  

### Widgets

- Warehouse summary cards  
- Product analytics (fast/slow)  
- Quick actions (New GRN, Transfer, Count, Waste)  
- Recent activities (last N ledger transactions)  

## 22.3 API Needs (Phase F)

`GET /inventory/dashboard/summary` returning typed DTO — avoid N+1 client fan-out.

## 22.4 Permissions

`Inventory.View` for summary; sensitive valuation may require `InventoryReports.View`.

## 22.5 Part 07 Conclusion

API surface is broad and versioned; Angular inventory feature is modular and permission-aware; Dashboard must evolve from shortcuts to a dedicated analytical API in Phase F.

---

> **Continue with Part 08**
