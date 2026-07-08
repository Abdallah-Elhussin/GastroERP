# Phase 25 — AI & Intelligent Restaurant Platform
# منصة الذكاء الاصطناعي لمطاعم GastroERP

> **📚 فهرس التوثيق الكامل:** [`00 — فهرس التوثيق وملخص المنصة.md`](00%20—%20فهرس%20التوثيق%20وملخص%20المنصة.md)

## Build Target

- 0 Errors
- 0 Warnings
- Clean Architecture · DDD · CQRS · MediatR · Multi-Tenant · Production Ready

---

## الحالة الحالية

| المرحلة | الحالة |
|---------|--------|
| Phase 1–24 | ✅ مكتمل |
| **Phase 25.1** AI Data Foundation | ✅ **مكتمل** |
| **Phase 25.2** Predictive Analytics | ✅ **مكتمل** |
| **Phase 25.3** Optimization | ✅ **مكتمل** |
| **Phase 25.4** Generative AI | ✅ **مكتمل** |
| **Phase 25.5** Advanced Intelligence | ✅ **مكتمل** |

---

## الهدف

تحويل GastroERP إلى **منصة مطاعم ذكية** مبنية على:
1. **بيانات موثوقة** (Data Foundation)
2. **تنبؤات دقيقة** (Predictive Analytics)
3. **توصيات تشغيلية** (Optimization)
4. **تفاعل ذكي** (Generative AI)
5. **ذكاء متقدم** (Fraud, Segmentation, Churn, Recommendations)

---

## القاعدة الذهبية

> **لا تُبنى أي ميزة ML/AI قبل اكتمال Phase 25.1**

```
25.1 AI Data Foundation     ← الأساس (إلزامي)
  ↓
25.2 Predictive Analytics   ← التنبؤ
  ↓
25.3 Optimization           ← التحسين
  ↓
25.4 Generative AI          ← التوليد
  ↓
25.5 Advanced Intelligence  ← الذكاء المتقدم
```

---

## Phase 25.1 — AI Data Foundation (قيد التنفيذ)

### 25.1.1 Data Warehouse
- جداول حقائق يومية: مبيعات، مخزون
- `WarehouseSyncRun` — سجل المزامنة
- `IDataWarehouseSyncService` — ETL من الجداول التشغيلية
- مهمة ليلية عبر Phase 24

### 25.1.2 Feature Store
- `FeatureDefinition`, `FeatureStoreSnapshot`, `FeatureLineage`
- 5 مجموعات ميزات: SalesVelocity, Seasonality, StockTurnover, CustomerRfm, KitchenLoad
- تحديث batch + incremental (Domain Events)

### 25.1.3 ML Dataset Builder
- `MlDatasetDefinition`, `MlDatasetExport`
- تصدير CSV/JSON مع PII masking
- train/validation/test splits

### API
- `GET/POST /api/v1/ai/warehouse/*`
- `GET /api/v1/ai/features/*`
- `GET/POST /api/v1/ai/datasets/*`
- `GET /api/v1/ai/data-quality`

### Migration
- `AddAiDataFoundationModule`

---

## المراحل اللاحقة (Roadmap)

راجع التفاصيل الكاملة في:
`Document/check/Phase25_AI_Intelligent_Restaurant_Platform.md`

| المرحلة | الوحدات |
|---------|---------|
| 25.2 | Demand / Sales / Inventory Forecasting |
| 25.3 | Purchase, Recipe, Staff, Pricing |
| 25.4 | Assistant, Insights, NL Query, Voice |
| 25.5 | Fraud, Segmentation, Churn, Recommendations |

---

## الصلاحيات

- `Ai.Data.View` · `Ai.Data.Manage`
- `Ai.Forecast.View` · `Ai.Recommendations.*`
- `Ai.Chat.Use` · `Ai.Voice.Use` · `Ai.Intelligence.View`
- `Ai.Admin.ManageModels`

---

## Phase 25.2 — Predictive Analytics (مكتمل)

### 25.2.1 Demand Forecasting
- تنبؤ الطلب يومي لكل منتج (Moving Average + Day-of-Week)

### 25.2.2 Sales Forecasting
- تنبؤ الإيرادات لكل فرع (Trend + Moving Average)

### 25.2.3 Inventory Forecasting
- مخاطر نفاد المخزون + أيام حتى النفاد + Safety Stock

### API
- `GET /api/v1/ai/forecast/demand|sales|inventory`
- `POST /api/v1/ai/forecast/refresh`
- `GET /api/v1/ai/predictions`

### Migration
- `AddAiPredictionModule`

---

## Phase 25.3 — Optimization (مكتمل)

### الخدمات
- **Purchase Recommendations** — اقتراح أوامر شراء للأصناف عالية المخاطر
- **Recipe Cost Optimization** — تحسين هامش الوصفات
- **Staff Scheduling** — تنبيهات نقص/زيادة مو staff
- **Dynamic Pricing** — اقتراحات تعديل الأسعار (استشارية)

### API
- `GET /api/v1/ai/recommendations/purchase|recipe-cost|staff-scheduling|pricing`
- `GET /api/v1/ai/recommendations` — قائمة التوصيات
- `POST /api/v1/ai/recommendations/refresh`
- `POST /api/v1/ai/recommendations/{id}/apply|dismiss`

### Migration
- `AddAiRecommendationModule`

---

## Phase 25.4 — Generative AI (مكتمل)

### الخدمات
- **Management AI Assistant** — مساعد إداري يجيب على أسئلة KPIs
- **Dashboard Insights** — ملخص تنفيذي + تنبيهات
- **Natural Language Query** — استعلامات read-only (عربي/إنجليزي)
- **Voice Ordering** — تحويل النص الصوتي إلى مسودة طلب للتأكيد

### Domain
- `AiGenerativeLog` — سجل تدقيق للتفاعلات
- `VoiceOrderDraft` — مسودة طلب صوتي

### Infrastructure
- `IGenerativeAiAdapter` — Heuristic (افتراضي) + OpenAI stub
- `AiOptions` — إعدادات المزود

### API
- `POST /api/v1/ai/chat` + `POST /api/v1/ai/chat/stream`
- `GET /api/v1/ai/insights/dashboard`
- `POST /api/v1/ai/query`
- `POST /api/v1/ai/voice/order` + `POST /api/v1/ai/voice/order/confirm`

### Migration
- `AddAiGenerativeModule`

---

## Phase 25.5 — Advanced Intelligence (مكتمل)

### الخدمات
- **Fraud Detection** — Risk Score 0–100 (خصومات، void، refund، duplicate payments)
- **Customer Segmentation** — VIP, Loyal, Active, New, AtRisk, Dormant, Lost
- **Churn Prediction** — احتمال مغادرة + توصيات retention
- **Recommendation Engine** — Upsell, CrossSell, Similar, Frequently Bought Together

### API
- `GET/POST /api/v1/ai/intelligence/fraud|segments|churn|recommendations`
- `GET /api/v1/ai/intelligence/dashboard|monitoring`

### Migration
- `AddAiIntelligenceModule`

---

## Definition of Done — Phase 25.5

- [x] 4 intelligence services (advisory-only, read-only)
- [x] Jobs + notifications + audit + dashboard
- [x] Build 0 Errors / 0 Warnings

---

## Definition of Done — Phase 25.4

- [x] Management assistant answers KPI questions
- [x] NL queries read-only + tenant-scoped
- [x] Voice → order draft for confirmation
- [x] Streaming chat endpoint (SSE)
- [x] Build 0 Errors / 0 Warnings

---

## Definition of Done — Phase 25.3

- [x] 4 optimization services (advisory-only)
- [x] Apply/Dismiss with audit trail
- [x] No auto-execution without permission
- [x] Build 0 Errors / 0 Warnings

---

- [x] 3 forecast services with heuristic fallback
- [x] Predictions stored with explainability metadata
- [x] Scheduled refresh via Phase 24 jobs
- [x] Build 0 Errors / 0 Warnings

---

- [x] Warehouse synced nightly per tenant
- [x] Feature store with ≥ 5 feature groups
- [x] Dataset builder with audit trail
- [x] Data quality endpoint
- [x] Build 0 Errors / 0 Warnings

---

*للتفاصيل التقنية الكاملة: `Document/check/Phase25_AI_Intelligent_Restaurant_Platform.md`*
