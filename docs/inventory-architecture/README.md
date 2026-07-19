# GastroERP Inventory Architecture — Index

Complete enterprise architecture documentation for the Inventory Module.

**Immutable rule:** `InventoryItem → Recipe → Product` (ProductCatalogDefinition = coordinator only).

## Parts

| Part | File | Sections |
|------|------|----------|
| 01 | [PART-01-Executive-Maturity-CurrentState.md](./PART-01-Executive-Maturity-CurrentState.md) | 1–3 |
| 02 | [PART-02-ModuleMap-BoundedContext-DomainModel.md](./PART-02-ModuleMap-BoundedContext-DomainModel.md) | 4–6 |
| 03 | [PART-03-ER-Product-Warehouse.md](./PART-03-ER-Product-Warehouse.md) | 7–9 |
| 04 | [PART-04-Balance-Transactions-Pipeline.md](./PART-04-Balance-Transactions-Pipeline.md) | 10–12 |
| 05 | [PART-05-Cost-Batch-Serial-Reservation.md](./PART-05-Cost-Batch-Serial-Reservation.md) | 13–16 |
| 06 | [PART-06-PhysicalInventory-CQRS-Events.md](./PART-06-PhysicalInventory-CQRS-Events.md) | 17–19 |
| 07 | [PART-07-API-Frontend-Dashboard.md](./PART-07-API-Frontend-Dashboard.md) | 20–22 |
| 08 | [PART-08-Reports-Security-Integration.md](./PART-08-Reports-Security-Integration.md) | 23–25 |
| 09 | [PART-09-Performance-Gaps-SOLID.md](./PART-09-Performance-Gaps-SOLID.md) | 26–28 |
| 10 | [PART-10-Debt-Roadmap-FinalAssessment.md](./PART-10-Debt-Roadmap-FinalAssessment.md) | 29–31 |

**Overall maturity:** ~6.5/10 — strong architecture; complete Pipeline + Costing to reach 8.5+.
