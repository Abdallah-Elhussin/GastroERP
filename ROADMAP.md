# GastroERP Frontend Master Roadmap / مخطط الواجهة الأمامية الشامل لـ GastroERP

---

## English Version

### General Rules
- The Stitch designs are the **single source of truth**.
- Recreate every screen pixel-perfect. Do not simplify or redesign.
- Technical Stack: Angular 20+, Standalone Components, Signals, RxJS, Angular Material 3, Tailwind CSS, SCSS, Strict TypeScript (No NgModules).
- Architecture: Clean Architecture, SOLID, Feature-Based structure, Repository pattern.
- Caching/State: OnPush Change Detection, TrackBy loop optimization, Signals for local component states.
- Multi-lingual: Full support for English (LTR) and Arabic (RTL) with dynamic language translation.

---

### Implementation Phases

#### Phase 1: Design System & Foundation (Completed)
- Create design tokens, typography scales, spacing, shadows, animations, and CSS variables.
- Build 15 reusable UI components (`AppButton`, `AppCard`, `AppTable`, `AppSearch`, etc.).
- Build 6 layouts (`PublicLayout`, `AuthLayout`, `PortalLayout`, etc.).

#### Phase 2: Authentication & Onboarding (Completed)
- Develop Auth flows, JWT/refresh tokens readiness, functionally-bound Router Guards, and sessionStorage tokens caching.
- Onboarding Multi-step setup wizard and branch select.

#### Phase 3: Customizable Dashboard Builder (Completed)
- Drag and drop grid widgets, layout editors, and caching structures using localStorage.

#### Phase 4: Shared Core Extension Libraries (Completed)
- Advanced tables with multi-column sorting, compact density switch, column chooser toggles, CSV exports, and reactive form draft buffers with undo/redo capabilities.

#### Phase 5: Business Modules Shell (Completed)
- Standalone components for POS, Kitchen (KDS), Inventory, CRM, HR, Finance, Reporting, and Settings.

#### Phase 6: Enterprise UX (Completed)
- Command Palette dialog (`Ctrl + K`), search over database files, bell notification alarms dropdown feed, and page bookmarks.

#### Phase 7: Media Library & Branding live updates (Completed)
- Media Browser (crop modal overlays, WebP converter tags, folders directory, multi-upload drag-drop slots).
- Branding Live Engine: binds inputs directly to CSS properties, reloading page styling reactively without full tab refreshes.

#### Phase 8: Workflow & Productivity (Pending)
- Kanban board, Approval Center, task list, comments feeds, deadlines, and activity logs.

#### Phase 9: Reporting & Analytics (Pending)
- KPI Engine, scheduled reports, pivot tables, and exports (PDF, Excel, CSV).

#### Phase 10: Administration Settings (Pending)
- Fiscal years, taxes configurations, roles and permission management, audit logs, feature flags, and email templates.

#### Phase 11: Backend API Integration (Pending)
- Replace Mock repositories with concrete REST/HTTP repositories, mapper DTO structures, and centralized HTTP interceptor handles.

#### Phase 12: Real-time Synchronizations (Pending)
- SignalR or WebSocket events channels for KDS order feeds, live POS drawer sales, and notification streams.

#### Phase 13: Offline-First Experience (Pending)
- IndexedDB storage, offline queues, conflict resolution systems, and background re-connect loops.

#### Phase 14: Performance Tuning (Pending)
- Deferrable views, responsive asset sizes, virtual scroll list pages, bundle size audits, and pre-fetching paths.

#### Phase 15: WCAG Accessibility Audits (Pending)
- Focus traps, screen reader ARIA labels, and high contrast options.

#### Phase 16: System-wide Security (Pending)
- Secure token storage, token auto-refresh triggers, and session timeouts.

#### Phase 17: Production Readiness (Pending)
- Telemetry, logging hooks, error boundary pages, and maintenance gate flags.

---

## النسخة العربية (Arabic Version)

### القواعد العامة للمشروع
- تصميمات Stitch هي **المصدر الوحيد والمطلق للحقيقة**.
- إعادة بناء كل شاشة بشكل مطابق تماماً وبدقة البكسل دون تبسيط أو إعادة تصميم.
- التقنيات المستخدمة: Angular 20+، المكونات المستقلة (Standalone)، الإشارات (Signals)، مكتبة RxJS، ماتيريال 3 (Angular Material 3)، وتنسيقات Tailwind CSS و SCSS مع الالتزام بالأنماط البرمجية الصارمة (Strict TypeScript).
- الهيكل المعماري: البنية النظيفة (Clean Architecture)، مبادئ SOLID، الهيكلية القائمة على الميزات (Feature-Based)، ونمط المستودعات (Repository Pattern).
- توافق الأنظمة: دعم كامل للغتين الإنجليزية (LTR) والعربية (RTL)، والمظهرين الداكن والفاتح والمطابقة التلقائية لسمات المتصفح.

---

### مراحل التنفيذ التفصيلية

#### المرحلة 1: نظام التصميم والتأسيس (مكتملة)
- إعداد المتغيرات الرسومية، والخطوط، والمسافات، والظلال، ومتغيرات CSS.
- بناء 15 مكون واجهة مستخدم قابل لإعادة الاستخدام (أزرار، بطاقات، جداول، محركات بحث، لوحات تحميل وغيرها).
- بناء 6 قوالب تخطيط أساسية (قالب التوثيق، لوحة الإدارة، لوحة الكاشير، لوحة المطبخ).

#### المرحلة 2: نظام المصادقة والتهيئة (مكتملة)
- بناء شاشات تسجيل الدخول والتهيئة الأولية، وتفعيل التحقق من الجلسة (Auth Guard)، وتهيئة نظام الرموز الأمنية (JWT Tokens) وحفظ خيارات الفروع النشطة.

#### المرحلة 3: لوحة التحكم المخصصة (مكتملة)
- بناء لوحة تحكم تفاعلية تدعم سحب وإفلات العناصر (Drag & Drop)، وتغيير حجم اللوحات، وحفظ واسترجاع أماكنها عبر الذاكرة المحلية.

#### المرحلة 4: المكتبات البرمجية المشتركة المتقدمة (مكتملة)
- تطوير الجداول لدعم الترتيب المتعدد، والفلترة، والتحكم في كثافة البيانات، وإمكانية إخفاء الأعمدة، وتصدير البيانات كملفات CSV.
- تطوير نظام المسودات للاستمارات لدعم التراجع وإعادة التراجع تلقائياً (Undo & Redo).

#### المرحلة 5: هيكل الوحدات الأساسية للمشروع (مكتملة)
- بناء وتوصيل الصفحات الأساسية للنظام: نقطة البيع (POS)، المطبخ (KDS)، المخازن، العملاء، شؤون الموظفين، الحسابات، التقارير والإعدادات.

#### المرحلة 6: تجربة المستخدم للمؤسسات الكبرى (مكتملة)
- إعداد لوحة الأوامر الشاملة بالضغط على زر `Ctrl + K` للبحث السريع عبر الوجبات والطلبات والزبائن والموظفين، بالإضافة إلى قائمة الصفحات المفضلة وعداد الإشعارات.

#### المرحلة 7: مكتبة الوسائط وإدارة العلامة التجارية (مكتملة)
- **مكتبة الوسائط**: إنشاء مستكشف الملفات والمجلدات المتداخلة، ودعم السحب والإفلات للرفع الجماعي، وقص وتدوير الصور وتحويلها لصيغة WebP لتسريع الأداء.
- **محرك العلامة التجارية**: دعم تغيير ألوان المؤسسة وخطوطها وشعاراتها فورياً وتطبيقها عبر متغيرات CSS دون إعادة بناء المشروع أو تحديث الصفحة.

#### المرحلة 8: تدفق العمل وإدارة الإنتاجية (قيد التنفيذ)
- بناء لوحة المهام التفاعلية (Kanban Board)، ونظام اعتماد الموافقات والمهام، والتعليقات والخطوط الزمنية للنشاطات.

#### المرحلة 9: التقارير والتحليلات المتقدمة (قيد الانتظار)
- بناء لوحات تحكم المؤشرات الحيوية (KPIs)، وإإعداد جدولة التقارير التلقائية، وتصدير التقارير بصيغة PDF و Excel.

#### المرحلة 10: الإدارة الشاملة وإعدادات النظام (قيد الانتظار)
- إدارة السنوات المالية، والضرائب، والعملات، ونظام الصلاحيات والأدوار للمستخدمين (RBAC)، وسجلات المراجعة الأمنية (Audit Logs).

#### المرحلة 11: الربط البرمجي الفعلي بالخوادم (قيد الانتظار)
- استبدال المستودعات الوهمية بمستودعات ربط حقيقية (REST Repositories) للاتصال بالخادم، ومعالجة الأخطاء المركزية وعمليات الفلترة والترتيب.

#### المرحلة 12: التحديث الفوري والربط المتزامن (قيد الانتظار)
- تفعيل القنوات الفورية (SignalR or WebSockets) لتحديث شاشات المطبخ، ومبيعات الكاشير، والتنبيهات الحية فور حدوثها.

#### المرحلة 13: تفعيل العمل دون اتصال بالشبكة (قيد الانتظار)
- حفظ وتخزين البيانات الهامة محلياً في المتصفح (IndexedDB)، وتهيئة طابور المهام المعلقة لإعادة رفعها تلقائياً عند عودة الاتصال.

#### المرحلة 14: تدقيق وتحسين الأداء البرمجي (قيد الانتظار)
- تفعيل التحميل المؤجل للواجهات (Deferrable Views)، والتحميل الافتراضي للجداول الضخمة، وضغط المكونات البرمجية لتسريع التحميل الأولي للموقع.

#### المرحلة 15: معايير سهولة الوصول لذوي الاحتياجات (قيد الانتظار)
- مطابقة شروط سهولة الوصول (WCAG AA)، ودعم قارئات الشاشة، والملاحة بالكامل عبر لوحة المفاتيح.

#### المرحلة 16: حماية وأمن الواجهات (قيد الانتظار)
- تأمين حفظ الرموز البرمجية، وتنشيطها تلقائياً، وتفعيل خيارات تسجيل الخروج التلقائي عند عدم النشاط.

#### المرحلة 17: جاهزية إطلاق خوادم الإنتاج الفعلي (قيد الانتظار)
- بناء معالج الأخطاء العام، وصفحات الصيانة، وإعداد أدوات التتبع وجمع الإحصائيات البرمجية للموقع.
