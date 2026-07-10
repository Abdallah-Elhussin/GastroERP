# GastroERP — Product Catalog Engine Tasks

## Phase 1 — Foundation ✅
- [x] `ProductCatalogDefinition` aggregate + `CatalogCodeSequence`
- [x] 13 extensible `ProductCatalogType` values
- [x] Configurable code generator (RAW-000001, MEN-000001, …)
- [x] CQRS: create draft, update general info, list, get by id, list types
- [x] REST API `/api/v1/catalog/*`
- [x] RBAC permissions `Catalog.*`
- [x] Angular catalog list + 7-step wizard
- [x] Sidebar: Product Catalog under Inventory
- [x] EF migration `AddProductCatalogEngine`
- [x] Backend + Frontend build verified

## Phase 2 — Inventory & Units ✅
- [x] Wizard step 3: inventory policy (units, min/max, reorder, costing method)
- [x] Auto-create `InventoryItem` from catalog draft
- [x] Link catalog ↔ inventory on save (`PUT .../inventory`)
- [x] EF migration `AddCatalogInventoryFields`
- [ ] Multi-unit UI (ItemUnit equivalent) — deferred to later enhancement

## Phase 3 — Recipe / BOM ✅
- [x] Wizard step 4: ingredients, yield, waste, prep/cook time
- [x] `SaveCatalogRecipeCommand` + auto-create/link `Recipe`
- [x] Stub `Product` created when recipe requires it (Recipe FK)

## Phase 4 — POS & Product ✅
- [x] Wizard step 5: POS visibility, kitchen station, prep time
- [x] Auto-create/update `Product` for sellable types
- [x] POS wired to Menu API (`MenuService` → `/menu/products`)

## Phase 5 — Pricing ✅
- [x] Wizard step 6: base price + price level overrides
- [x] `ProductPriceHistory` entity + audit on price change
- [x] `GET .../price-history` API

## Phase 6 — Media, Suppliers, Variants ✅
- [x] Extensions panel on review step (suppliers, media URLs, variant JSON)
- [x] `SaveCatalogExtensionsCommand` syncs media to linked `Product`
- [ ] Full media library picker — deferred

## Phase 7 — Relationships & AI ✅
- [x] Relationships panel on review (cross-sell, upsell, alternative)
- [x] `SaveCatalogRelationshipsCommand`
- [ ] Product 360 dashboard — deferred
- [ ] AI suggestions integration — deferred (AI API exists separately)

## Phase 8 — Bulk & Audit ✅
- [x] CSV export (`GET .../definitions/export`)
- [x] CSV import (`POST .../definitions/import`)
- [x] Audit timeline (`GET .../definitions/{id}/audit`)
- [x] Activate catalog (`POST .../definitions/{id}/activate`)

## Architecture Rules (Never Violate)
- Do NOT merge `InventoryItem` and `Product`
- Flow: `InventoryItem → Recipe → Product`
- Clean Architecture + DDD + CQRS + MediatR
