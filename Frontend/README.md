# GastroERP SaaS Restaurant Platform Frontend
## Angular 20 • Angular Material 3 • Tailwind CSS • SCSS

Welcome to the frontend application repository of GastroERP, an enterprise-grade Restaurant ERP SaaS platform. The application is built using a pixel-perfect interpretation of Stitch designs as the single source of truth, optimized for scalability, accessibility (WCAG AA), and offline reliability.

مرحباً بك في مستودع الواجهة الأمامية لمنصة **GastroERP** - النظام السحابي المتكامل لإدارة المطاعم والمنشآت الغذائية. تم تطوير الواجهة باعتماد معايير معمارية متطورة تضمن سرعة الأداء وسهولة الوصول والأمان التشغيلي.

---

## 🛠 Technology Stack / التقنيات المستخدمة

- **Core Framework**: Angular 20+ (Standalone Components, Signals, Computed properties, Effects).
- **Styling & Theme Engine**: Tailwind CSS & SCSS Custom Properties (pair compilation).
- **Component UI Library**: Angular Material 3 components.
- **State Management**: Reactive signals, RxJS streams, and local storage state sync.
- **Offline Storage**: Browser IndexedDB database client.
- **Real-Time Gateway**: SignalR WebSocket client wrapper.

---

## 📁 Architecture / الهيكل المعماري للمشروع

We adhere strictly to Clean Architecture and Feature-Based Architecture:

```text
src/
 ├── core/              # Global singletons: interceptors, guards, abstract repositories, services
 ├── shared/            # Reusable elements: UI components library, layouts wrappers
 ├── features/          # Feature slices: POS, Kitchen KDS, Dashboard, Inventory, Workflow, Reports, Settings
 ├── assets/            # Static assets
 ├── styles.scss        # Global style overrides, keyframe animations, typography, and dark/light themes variables
```

---

## 🚀 Phases 1 - 17 Roadmap / خريطة تنفيذ مراحل المشروع

### 1. Design System & Layouts (السمات والقوالب المشتركة)
Integrated 15 reusable atomic UI components (`AppButton`, `AppTable`, `AppCard`, etc.) and 6 layouts wrappers to prevent template duplication.

### 2. Authentication & Onboarding (الأمان وإعداد الحساب)
Designed functional route guards, automated wizard setup paths, and secure JWT login storage.

### 3. Customizable Dashboard (لوحة تحكم تفاعلية)
Interactive widget dashboard layout editor (size toggles, visibility settings, drag grid states cached in local storage).

### 4. Advanced Component Libraries (المكتبات المتقدمة للبيانات)
Created custom `AppTableComponent` supporting server pagination, column selectors, density settings, and debounced reactive form autosave draft manager buffers.

### 5. Business Modules (وحدات العمل الرئيسية)
Scaffolded POS Cashout drawers, Kitchen Display order views, Inventory listings, Employees directory logs, and SaaS currency localization preferences.

### 6. Universal Command Search (البحث العام الذكي)
Built global command palette wrapper (`Ctrl + K`) to search across views, products, active tickets, and staff members dynamically.

### 7. Media Picker & Branding Live Customization (إدارة الوسائط وهوية النظام)
Integrated custom drag & drop media directories, image cropping dialogs, and a live CSS variables brand engine updating colors reactively.

### 8. Kanban Workflows & Approval Center (إدارة مسارات العمل والاعتمادات)
Integrated interactive Kanban pipelines, Accept/Reject approval drawers, rule condition flowchart builders, and active comment feeds.

### 9. Analytics & KPI Pivots (التقارير الإحصائية المتقدمة)
Scaffolded pivot tables showing category sale distributions, expandable rows, automated scheduler configuration triggers, and KPI widgets.

### 10. Tenant Settings & RBAC Access Matrix (الصلاحيات المتعددة والتراخيص)
Built reactive tenant metadata inputs, fiscal year setups, branch locations registries, audit log timelines, and subscription licensing meters.

### 11. Http Authorization Interceptors (ملتقط طلبات الشبكة للتوثيق)
Registered functional authorization headers injection and global HTTP status code catcher (intercepting 401, 403, 500 error alerts).

### 12. Real-Time SignalR updates (التحديث الحي الفوري للعمليات)
Integrated `SignalrService` presence monitoring hooks, order dispatch events, and KDS tickets sync without page reloading.

### 13. Offline First IndexedDB Caching (التشغيل التلقائي دون اتصال بالإنترنت)
Built local browser-side database tables caching offline transactions and enqueuing background synchronizations when network status changes.

### 14. Performance Chunks & Deferrable View Optimizations (سرعة الأداء ورندر الصفحات)
Applied `@defer` templates to operational trends charts and sales tables to ensure faster viewport paint cycles.

### 15. Accessibility AA Standards (سهولة الوصول وقارئات الشاشة)
Darkened/lightened `--text-muted` contrast ratios, included semantic HTML ARIA bindings, and enabled full keyboard-nav index selectors.

### 16. Security Hardening & Sanitization (أمن البيانات ومصادقة الطلبات)
Integrated automatic silent JWT refresh rotations retrying failed 401 requests, role-based `*appHasPermission` directive, and text sanitizers.

### 17. Custom Error Routing & Telemetry Logging (المراقبة وجاهزية الإنتاج)
Integrated custom recovery pages (NotFound 404, generic 403 and 500 pages), registered routing exceptions, and linked system telemetry diagnostics logger.

---

## 🛠 Local Development & Build Commands / تشغيل وبناء النظام

### Run Development Server
```bash
ng serve
```
Navigate to `http://localhost:4200/`.

### Build Production Package
```bash
npm run build
```
Build outputs are optimized, chunked, and stored under `dist/gastro-erp-client/` with **0 errors and 0 warnings**.
