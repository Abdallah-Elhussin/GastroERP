# GastroERP Backend Roadmap
# Phase 25 — AI & Intelligent Restaurant Platform
# منصة الذكاء الاصطناعي لمطاعم GastroERP

## Current Status | الحالة الحالية

### Completed | مكتمل

- Phase 1–23 — Core through Reporting & BI
- Phase 24 — Background Jobs, Notifications & External Integrations

Build Status

- ✅ 0 Errors
- ✅ 0 Warnings

---

# Mission | الهدف

Transform GastroERP from a traditional multi-tenant ERP into an **intelligent restaurant platform** — built on **reliable data**, **stable architecture**, and **progressive AI capabilities** that improve model quality and business outcomes.

تحويل GastroERP من نظام ERP تقليدي إلى **منصة مطاعم ذكية** — مبنية على **بيانات موثوقة** و**بنية مستقرة** و**قدرات ذكاء اصطناعي تدريجية** لرفع جودة النماذج ودقة النتائج.

---

# Implementation Status | حالة التنفيذ

| Sub-Phase | Status | Migration |
|-----------|--------|-----------|
| **25.1 AI Data Foundation** | ✅ **COMPLETED** | `AddAiDataFoundationModule` |
| **25.2 Predictive Analytics** | ✅ **COMPLETED** | `AddAiPredictionModule` |
| **25.3 Optimization** | ✅ **COMPLETED** | `AddAiRecommendationModule` |
| **25.4 Generative AI** | ✅ **COMPLETED** | `AddAiGenerativeModule` |
| **25.5 Advanced Intelligence** | ✅ **COMPLETED** | `AddAiIntelligenceModule` |

**Current sprint:** Phase 25 Complete ✅

---

# Technical Folder Structure | هيكل المجلدات

```
Backend/src/
├── GastroErp.Domain/
│   ├── Entities/Ai/AiDataEntities.cs
│   └── Enums/AiEnums.cs
├── GastroErp.Application/Features/Ai/
│   ├── DTOs/AiDataDtos.cs
│   ├── Services/IAiDataServices.cs + implementations
│   ├── Queries/ + Commands/ + Validators/
│   └── EventHandlers/AiFeatureEventHandlers.cs
├── GastroErp.Persistence/Configurations/Ai/
└── GastroErp.Presentation/Controllers/Ai/
```

---

> **Golden Rule:** لا تُبنى نماذج التنبؤ أو التوليد قبل اكتمال **AI Data Foundation**.
> كل مرحلة فرعية تعتمد على سابقتها — بهذا التسلسل تكون مرحلة الذكاء الاصطناعي مبنية على بيانات موثوقة وبنية مستقرة.

```
Phase 25.1  AI Data Foundation        ← الأساس (إلزامي أولاً)
     │
     ▼
Phase 25.2  Predictive Analytics      ← التنبؤ
     │
     ▼
Phase 25.3  Optimization              ← التحسين التشغيلي
     │
     ▼
Phase 25.4  Generative AI             ← التوليد والتفاعل
     │
     ▼
Phase 25.5  Advanced Intelligence     ← الذكاء المتقدم
```

---

# Architecture Rules | قواعد المعمارية

- AI/ML logic lives in **Application** (services) and **Infrastructure** (model adapters)
- No ML inference inside Domain entities
- All predictions and recommendations are **advisory** — human or policy engine confirms actions
- **Tenant-scoped** training data, feature stores, and model registry
- Explainability metadata on every AI output (confidence, factors, model version)
- Fallback to rule-based heuristics when models unavailable
- Leverage Phase 24: background jobs for batch feature extraction, event handlers for incremental updates
- Arabic + English localization for all user-facing AI outputs

---

# Phase 25.1 — AI Data Foundation
# أساس بيانات الذكاء الاصطناعي

**Priority:** 🔴 Critical — Must complete before any ML/AI feature  
**الأولوية:** حرجة — يجب إكمالها قبل أي ميزة ML/AI

## Objective | الهدف

Establish a trustworthy, tenant-isolated data layer that feeds all downstream AI modules.

إنشاء طبقة بيانات موثوقة ومعزولة لكل tenant تغذّي جميع وحدات الذكاء الاصطناعي اللاحقة.

## Sub-Modules | الوحدات الفرعية

### 25.1.1 Data Warehouse | مستودع البيانات

| Item | Description |
|------|-------------|
| **Purpose** | Consolidated analytical read model (star/snowflake schema) from operational DB |
| **Sources** | Sales, Inventory, CRM, Finance, Kitchen, Delivery, Reporting (Phase 23) |
| **Granularity** | Tenant → Company → Branch → Day/Hour |
| **Refresh** | Nightly full + incremental via Phase 24 jobs |
| **Storage** | SQL analytical views / dedicated schema / optional OLAP export |

**Deliverables:**
- `AiDataWarehouse` schema or materialized views
- ETL/ELT pipeline services (`IDataWarehouseSyncService`)
- Data quality checks (completeness, freshness, anomaly flags)
- Migration: `AddAiDataWarehouseModule`

---

### 25.1.2 Feature Store | مخزن الميزات

| Item | Description |
|------|-------------|
| **Purpose** | Reusable, versioned ML features per entity (item, branch, customer, day) |
| **Feature Groups** | Sales velocity, seasonality, stock turnover, customer RFM, kitchen load |
| **Update Mode** | Batch (nightly) + streaming (domain events from Phase 24) |
| **Storage** | DB tables + optional Redis cache for online inference |

**Domain Entities:**
- `FeatureDefinition` — name, type, entity, computation logic version
- `FeatureStoreSnapshot` — tenant/branch/entity/features as of timestamp
- `FeatureLineage` — source tables, last refresh, quality score

**Deliverables:**
- `IFeatureStoreService` — read/write features, point-in-time lookups
- `IFeatureComputationService` — batch + incremental computation
- Event hooks: `OrderCompletedEvent`, `PaymentCompletedEvent`, `StockMovementRecordedEvent`, `LoyaltyPointsEarnedEvent`

---

### 25.1.3 ML Dataset Builder | باني مجموعات بيانات ML

| Item | Description |
|------|-------------|
| **Purpose** | Export labeled, train/validation/test splits for model training |
| **Output Formats** | CSV, Parquet, JSON (tenant-scoped) |
| **Labeling** | Historical outcomes (actual sales, stock-outs, churn events) |
| **Governance** | PII masking, retention policy, export audit log |

**Deliverables:**
- `IMlDatasetBuilderService` — define dataset spec, generate, version
- `MlDatasetDefinition`, `MlDatasetExport` entities
- API: `POST /api/v1/ai/datasets/build`, `GET /api/v1/ai/datasets`

---

## Phase 25.1 — Definition of Done

- [x] Data warehouse synced nightly for all tenants
- [x] Feature store populated with ≥ 5 core feature groups
- [x] Dataset builder exports train/test splits with audit trail
- [x] Data quality dashboard (freshness, completeness)
- [x] Build = 0 Errors / 0 Warnings

---

# Phase 25.2 — Predictive Analytics
# التحليلات التنبؤية

**Depends on:** Phase 25.1 ✅  
**يعتمد على:** أساس البيانات 25.1

## Objective | الهدف

Forecast future demand, sales, and inventory needs using features from the Feature Store.

التنبؤ بالطلب والمبيعات واحتياجات المخزون باستخدام ميزات Feature Store.

## Sub-Modules | الوحدات الفرعية

### 25.2.1 Demand Forecasting | التنبؤ بالطلب

| Input | Output |
|-------|--------|
| Historical orders, seasonality, events, menu mix | Daily/weekly item-level demand forecast |
| Branch, day-of-week, holidays | Confidence intervals + explainability |

**Service:** `IDemandForecastService`  
**API:** `GET /api/v1/ai/forecast/demand`

---

### 25.2.2 Sales Forecasting | التنبؤ بالمبيعات

| Input | Output |
|-------|--------|
| Branch revenue history, channel, trends | Daily/weekly/monthly revenue forecast |
| Optional: weather, promotions | Branch comparison + variance analysis |

**Service:** `ISalesForecastService`  
**API:** `GET /api/v1/ai/forecast/sales`

---

### 25.2.3 Inventory Forecasting | التنبؤ بالمخزون

| Input | Output |
|-------|--------|
| Consumption, recipes, waste, lead times | Stock-out risk per item/warehouse |
| Demand forecast (25.2.1) | Days-until-stockout, suggested safety stock |

**Service:** `IInventoryForecastService`  
**API:** `GET /api/v1/ai/forecast/inventory`

---

## Domain (Phase 25.2)

- `AiModelRegistry` — model id, version, provider, tenant scope, metrics
- `PredictionRun` — input snapshot, output, confidence, explainability JSON

## Phase 25.2 — Definition of Done

- [x] 3 forecast services operational with heuristic fallback
- [x] Predictions stored with explainability metadata
- [x] Scheduled refresh via Phase 24 jobs
- [x] API documented in Swagger
- [x] Build = 0 Errors / 0 Warnings

---

# Phase 25.3 — Optimization
# التحسين التشغيلي

**Depends on:** Phase 25.2 ✅  
**يعتمد على:** التحليلات التنبؤية 25.2

## Objective | الهدف

Turn forecasts into actionable operational recommendations — always advisory, never auto-executed without approval.

تحويل التنبؤات إلى توصيات تشغيلية قابلة للتنفيذ — استشارية دائماً، لا تُنفَّذ تلقائياً دون موافقة.

## Sub-Modules | الوحدات الفرعية

### 25.3.1 Purchase Recommendations | توصيات الشراء

| Input | Output |
|-------|--------|
| Inventory forecast + reorder levels + supplier lead time | Draft PO line suggestions |
| Cost, MOQ, preferred supplier | Priority ranking + estimated savings |

**Service:** `IPurchaseRecommendationService`  
**API:** `GET /api/v1/ai/recommendations/purchase`

---

### 25.3.2 Recipe Cost Optimization | تحسين تكلفة الوصفات

| Input | Output |
|-------|--------|
| Ingredient prices, yields, substitutions | Alternative recipe suggestions |
| Target margin, menu price | Cost delta + margin impact |

**Service:** `IRecipeCostOptimizationService`  
**API:** `GET /api/v1/ai/recommendations/recipe-cost`

---

### 25.3.3 Staff Scheduling | جدولة الموظفين

| Input | Output |
|-------|--------|
| Sales forecast peaks, labor rules, shift history | Shift coverage recommendations |
| Branch capacity, role requirements | Under/over-staffing alerts |

**Service:** `IStaffSchedulingAdvisorService`  
**API:** `GET /api/v1/ai/recommendations/staff-scheduling`

---

### 25.3.4 Dynamic Pricing | التسعير الديناميكي

| Input | Output |
|-------|--------|
| Demand elasticity, margin, competitor signals (optional) | Suggested price adjustments |
| Menu item, time slot, channel | Revenue impact simulation |

**Service:** `IDynamicPricingService`  
**API:** `GET /api/v1/ai/recommendations/pricing`

---

## Domain (Phase 25.3)

- `RecommendationAction` — suggested action, status (pending / applied / dismissed), appliedBy, audit

## Phase 25.3 — Definition of Done

- [x] 4 optimization services with advisory-only outputs
- [x] User can apply/dismiss recommendations with audit trail
- [x] No auto PO / price change without explicit permission
- [x] Build = 0 Errors / 0 Warnings

---

# Phase 25.4 — Generative AI
# الذكاء الاصطناعي التوليدي

**Depends on:** Phase 25.1 ✅ + Phase 25.2 (recommended)  
**يعتمد على:** أساس البيانات 25.1 + التنبؤات 25.2 (مُستحسن)

## Objective | الهدف

Natural language interaction with operational data — management assistant, insights, queries, and voice ordering.

التفاعل بلغة طبيعية مع البيانات التشغيلية — مساعد إداري، رؤى، استعلامات، وطلب صوتي.

## Sub-Modules | الوحدات الفرعية

### 25.4.1 AI Assistant for Management | مساعد AI للإدارة

| Capability | Description |
|------------|-------------|
| Scope | Tenant-aware Q&A over KPIs, reports, alerts |
| Context | Branch, date range, role-based data access |
| Output | Arabic + English, structured + narrative |

**Service:** `IManagementAiAssistantService`  
**API:** `POST /api/v1/ai/chat` (streaming)

---

### 25.4.2 AI Dashboard Insights | رؤى لوحة التحكم الذكية

| Capability | Description |
|------------|-------------|
| Auto-narrative | Daily/weekly executive summary |
| Anomaly highlights | Unusual sales, waste, voids |
| Proactive alerts | Linked to Phase 24 notifications |

**Service:** `IAiDashboardInsightsService`  
**API:** `GET /api/v1/ai/insights/dashboard`

---

### 25.4.3 Natural Language Queries | استعلامات اللغة الطبيعية

| Capability | Description |
|------------|-------------|
| NL → SQL/Query | Safe, read-only query generation |
| Guardrails | Tenant filter enforced, no destructive ops |
| Examples | "ما أكثر منتج مبيعاً هذا الأسبوع؟" |

**Service:** `INaturalLanguageQueryService`  
**API:** `POST /api/v1/ai/query`

---

### 25.4.4 Voice Ordering | الطلب الصوتي

| Capability | Description |
|------------|-------------|
| Pipeline | Speech → intent → menu item mapping → order draft |
| Context | POS / kiosk / drive-through |
| Fallback | Manual confirmation before submit |

**Service:** `IVoiceOrderingService`  
**API:** `POST /api/v1/ai/voice/order`

---

## Infrastructure (Phase 25.4)

- Model providers: Azure OpenAI / OpenAI API (plug & play)
- Local ONNX for offline POS (optional)
- Rate limiting + token budget per tenant
- Prompt templates with Arabic RTL support

## Phase 25.4 — Definition of Done

- [x] Management assistant answers KPI questions accurately
- [x] NL queries are read-only and tenant-scoped
- [x] Voice ordering produces confirmable order drafts
- [x] Streaming responses for chat endpoint
- [x] Build 0 Errors / 0 Warnings

---

# Phase 25.5 — Advanced Intelligence
# الذكاء المتقدم

**Depends on:** Phase 25.1 ✅ + Phase 25.2 ✅  
**يعتمد على:** أساس البيانات + التحليلات التنبؤية

## Objective | الهدف

Deep customer and risk intelligence using mature feature store and historical labels.

ذكاء عميق للعملاء والمخاطر باستخدام Feature Store ناضج وبيانات تاريخية موسومة.

## Sub-Modules | الوحدات الفرعية

### 25.5.1 Fraud Detection | كشف الاحتيال

| Input | Output |
|-------|--------|
| Payments, voids, discounts, shift patterns | Anomaly score + reason codes |
| Real-time + batch | Alert via Phase 24 notifications |

**Service:** `IFraudDetectionService`  
**API:** `GET /api/v1/ai/intelligence/fraud`

---

### 25.5.2 Customer Segmentation | تقسيم العملاء

| Input | Output |
|-------|--------|
| RFM, loyalty, order frequency, AOV | Segments (VIP, at-risk, new, dormant) |
| Feature store customer group | Campaign targeting suggestions |

**Service:** `ICustomerSegmentationService`  
**API:** `GET /api/v1/ai/intelligence/segments`

---

### 25.5.3 Churn Prediction | التنبؤ بفقدان العملاء

| Input | Output |
|-------|--------|
| Visit frequency decay, last order, engagement | Churn probability per customer |
| Segment (25.5.2) | Retention action suggestions |

**Service:** `IChurnPredictionService`  
**API:** `GET /api/v1/ai/intelligence/churn`

---

### 25.5.4 Recommendation Engine | محرك التوصيات

| Input | Output |
|-------|--------|
| Order history, menu graph, co-purchase patterns | Upsell / cross-sell item suggestions |
| Customer segment, time of day | POS + mobile + delivery integration points |

**Service:** `IRecommendationEngineService`  
**API:** `GET /api/v1/ai/intelligence/recommendations`

---

## Phase 25.5 — Definition of Done

- [x] Fraud scoring on payment events (advisory alerts)
- [x] Customer segments refreshed daily
- [x] Churn model with heuristic scoring + recommendations
- [x] Recommendation engine integrated (suggest only)
- [x] Build 0 Errors / 0 Warnings

---

# Cross-Cutting Concerns | اعتبارات مشتركة

## Permissions | الصلاحيات

| Permission | Scope |
|------------|-------|
| `Ai.Data.View` | View warehouse/feature metadata |
| `Ai.Data.Manage` | Trigger sync, dataset exports |
| `Ai.Forecast.View` | Predictive analytics |
| `Ai.Recommendations.View` | Optimization suggestions |
| `Ai.Recommendations.Apply` | Apply approved recommendations |
| `Ai.Chat.Use` | Generative AI assistant |
| `Ai.Voice.Use` | Voice ordering |
| `Ai.Intelligence.View` | Advanced intelligence modules |
| `Ai.Admin.ManageModels` | Model registry, versioning |

## Security & Compliance | الأمان والامتثال

- PII minimization and masking in datasets
- Tenant data isolation in warehouse and feature store
- Audit log for every AI action (prediction, recommendation apply, query, export)
- No auto-execution of financial or inventory mutations
- GDPR/local retention policies on exported datasets

## Phase 24 Integration | التكامل مع Phase 24

| Job / Event | AI Usage |
|-------------|----------|
| Nightly recurring jobs | Warehouse sync, feature batch compute |
| `OrderCompletedEvent` | Update sales features, recommendation refresh |
| `PaymentCompletedEvent` | Fraud scoring, revenue features |
| `StockMovementRecordedEvent` | Inventory consumption features |
| `LoyaltyPointsEarnedEvent` | Customer feature refresh |
| Notifications | AI alerts (anomaly, churn, low forecast accuracy) |

---

# API Summary | ملخص API

| Base Path | Phase | Purpose |
|-----------|-------|---------|
| `/api/v1/ai/datasets` | 25.1 | ML dataset builder |
| `/api/v1/ai/features` | 25.1 | Feature store read/metadata |
| `/api/v1/ai/forecast/*` | 25.2 | Demand, sales, inventory forecasts |
| `/api/v1/ai/recommendations/*` | 25.3 | Purchase, recipe, staff, pricing |
| `/api/v1/ai/chat` | 25.4 | Management assistant |
| `/api/v1/ai/insights/*` | 25.4 | Dashboard insights |
| `/api/v1/ai/query` | 25.4 | Natural language queries |
| `/api/v1/ai/voice/*` | 25.4 | Voice ordering |
| `/api/v1/ai/intelligence/*` | 25.5 | Fraud, segments, churn, recommendations |

---

# Migrations | الهجرات

| Migration | Phase | When |
|-----------|-------|------|
| `AddAiDataWarehouseModule` | 25.1 | Data warehouse schema |
| `AddAiFeatureStoreModule` | 25.1 | Feature store tables |
| `AddAiMlDatasetModule` | 25.1 | Dataset builder |
| `AddAiPredictionModule` | 25.2 | Model registry, prediction runs |
| `AddAiRecommendationModule` | 25.3 | Recommendation actions |
| `AddAiGenerativeModule` | 25.4 | Generative logs, voice order drafts |
| `AddAiIntelligenceModule` | 25.5 | Segments, fraud scores (if persisted) |

> Create migrations per sub-phase — do NOT apply until explicitly requested.

---

# Overall Phase 25 — Definition of Done

- [x] **25.1** Data foundation operational (warehouse + feature store + dataset builder)
- [x] **25.2** Three forecast services with explainability
- [x] **25.3** Four optimization advisors (advisory-only)
- [x] **25.4** Generative AI (assistant, insights, NL query, voice)
- [x] **25.5** Advanced intelligence (fraud, segmentation, churn, recommendations)
- [ ] All sub-phases: Build = 0 Errors / 0 Warnings
- [ ] Arabic + English AI outputs
- [ ] Full audit trail for AI actions

---

# After Phase 25 | بعد Phase 25

## Phase 26 — Mobile Apps & Omnichannel (TBD)

- Customer mobile app
- Driver app enhancements
- Kiosk / self-order
- Unified notification preferences
- AI recommendations on mobile order flow

---

# Recommended Execution Order | ترتيب التنفيذ الموصى به

```
1. 25.1.1 Data Warehouse
2. 25.1.2 Feature Store
3. 25.1.3 ML Dataset Builder
        ─── Foundation Complete ───
4. 25.2.1 Demand Forecasting
5. 25.2.2 Sales Forecasting
6. 25.2.3 Inventory Forecasting
        ─── Predictive Complete ───
7. 25.3.1 Purchase Recommendations
8. 25.3.2 Recipe Cost Optimization
9. 25.3.3 Staff Scheduling
10. 25.3.4 Dynamic Pricing
        ─── Optimization Complete ───
11. 25.4.1 AI Assistant (Management)
12. 25.4.2 AI Dashboard Insights
13. 25.4.3 Natural Language Queries
14. 25.4.4 Voice Ordering
        ─── Generative Complete ───
15. 25.5.1 Fraud Detection
16. 25.5.2 Customer Segmentation
17. 25.5.3 Churn Prediction
18. 25.5.4 Recommendation Engine
        ─── Phase 25 Complete ───
```

---

*Updated: Phase 25.1 implementation started — see `Document/25 — AI & Intelligent Restaurant Platform.md`*
