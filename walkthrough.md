# Product Catalog Engine — Walkthrough

## Overview

The **Enterprise Product Catalog Engine** is the unified entry point for defining all products in GastroERP:

```
ProductCatalogDefinition  →  InventoryItem  →  Recipe  →  Product
```

All 8 phases are implemented. The 7-step wizard adapts by product type.

## API Endpoints

| Method | Route | Phase |
|--------|-------|-------|
| GET | `/api/v1/catalog/types` | 1 |
| GET/POST | `/api/v1/catalog/definitions` | 1 |
| GET | `/api/v1/catalog/definitions/export` | 8 |
| POST | `/api/v1/catalog/definitions/import` | 8 |
| GET | `/api/v1/catalog/definitions/{id}` | 1 |
| GET | `/api/v1/catalog/definitions/{id}/audit` | 8 |
| GET | `/api/v1/catalog/definitions/{id}/price-history` | 5 |
| PUT | `/api/v1/catalog/definitions/{id}/general` | 1 |
| PUT | `/api/v1/catalog/definitions/{id}/inventory` | 2 |
| PUT | `/api/v1/catalog/definitions/{id}/recipe` | 3 |
| PUT | `/api/v1/catalog/definitions/{id}/pos` | 4 |
| PUT | `/api/v1/catalog/definitions/{id}/pricing` | 5 |
| PUT | `/api/v1/catalog/definitions/{id}/extensions` | 6 |
| PUT | `/api/v1/catalog/definitions/{id}/relationships` | 7 |
| POST | `/api/v1/catalog/definitions/{id}/activate` | 8 |

## Wizard Steps

| Step | Creates/Updates |
|------|-----------------|
| Type | `ProductCatalogDefinition` draft + auto code |
| General | Names, SKU, categories, image |
| Inventory | `InventoryItem` linked |
| Recipe | `Recipe` + ingredients (creates stub `Product` if needed) |
| POS | `Product` visibility, category, kitchen station |
| Pricing | Base price, price levels, `ProductPriceHistory` |
| Review | Extensions, relationships, audit timeline, activate |

## Frontend Routes

| Route | Screen |
|-------|--------|
| `/catalog` | List + export/import |
| `/catalog/wizard` | New product wizard |
| `/catalog/wizard/:id` | Continue wizard |

POS loads products from `/api/v1/menu/products` when available (fallback to mock data).

## Migrations

```powershell
dotnet ef database update --project Backend/src/GastroErp.Persistence --startup-project Backend/src/GastroErp.Presentation
```

Required migrations:
- `AddProductCatalogEngine`
- `AddCatalogInventoryFields`
- `AddCatalogPhases3To8`

## Test Flow

1. Login: `admin@gastroerp.com` / `admin`
2. **المخزون → كatalog المنتجات → منتج جديد**
3. Complete wizard for a **Menu Item** type
4. Verify linked IDs on review step
5. **Activate & finish**
6. Open POS — menu products from API appear when catalog products exist
7. Export/import CSV from catalog list

## Deferred Enhancements

- Multi-unit row UI (ItemUnit)
- Media library picker integration
- Product 360 dashboard
- AI price/demand suggestions in wizard
