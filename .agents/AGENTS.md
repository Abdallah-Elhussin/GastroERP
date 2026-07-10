# GastroERP Project Rules & Implementation Plan

## General Rules
1. **Visual Accuracy (Stitch Design)**: Match the Stitch designs pixel-by-pixel. Do not simplify or redesign. Spacing, typography, border-radius, shadows, animations, and sizes must be respected as the visual source of truth.
2. **Modular Architecture & Standalone Components**: Avoid UI duplication. Build reusable layouts and UI library elements.
3. **No Hardcoded Data**: All data must be served by Angular Services using Angular Signals.
4. **OnPush Change Detection**: Optimize performance for enterprise-scale UI.
5. **Localization**: Translating all text. Support English (LTR) and Arabic (RTL) with dynamic runtime switching.
6. **No Placeholder Images**: All images must come from the Media Manager.

---

## 5-Phase Incremental Execution Plan
Do not generate all features at once. Each phase must compile successfully before moving to the next.

### Phase 1: Design System, Layouts, Theme & Navigation
- **Complete Design System**: Create tokens and standard styling rules (colors, typography, spacing, border-radius, shadows, elevation, animation keyframes, and icons).
- **Reusable UI Library (`src/app/shared/ui/`)**:
  - `AppButton`
  - `AppCard`
  - `AppTable`
  - `AppSearch`
  - `AppStatCard`
  - `AppDialog`
  - `AppToolbar`
  - `AppSidebar`
  - `AppBreadcrumb`
  - `AppAvatar`
  - `AppUpload`
  - `AppMediaPicker`
  - `AppChart`
  - `AppEmptyState`
  - `AppLoading`
- **Reusable Layout Wrapper Components**:
  - `PublicLayout`
  - `AuthLayout`
  - `PortalLayout`
  - `POSLayout`
  - `KitchenLayout`
  - `FullscreenLayout`
- **Theme & Direction switcher service**: Configure signals for light/dark theme switching and RTL/LTR layout adjustment.

### Phase 2: Authentication & Onboarding
- **Public & Auth screens**:
  - Landing / Marketing page using `PublicLayout`.
  - Split-screen Login screen using `AuthLayout`.
  - Multi-step Setup Wizard using `FullscreenLayout`.
- **Route Guards**: Secure auth-dependent pages.

### Phase 3: Customizable Dashboard
- **Customizable Widgets Grid**:
  - Drag & drop sorting and resizing.
  - Options to hide/pin/show dashboard elements.
  - Save and restore layout functions from storage.
  - Widget RBAC permissions.

### Phase 4: Shared Advanced Component Libraries
- **Shared Table component**:
  - Support server-side pagination, multi-column sorting, filter panels, row grouping.
  - Export (CSV, print), density controls, column chooser, saved custom views, virtual scroll.
- **Shared Reactive Form validators**:
  - Reactive forms, field level errors, auto-save state drafts, undo/redo buffers.

### Phase 5: Business Modules
- **POS Terminal Layout (`POSLayout`)**: Category menus, customization overlays, check drawers, billing splits.
- **Kitchen Display System (`KitchenLayout`)**: Ticket columns, time delay alerts, bump buttons.
- **HR Employees Section**: Attendance timelines, leave metrics, salary contract details.
- **Inventory Management**: Stock level tables, batch expiration details, quick-edit dialogs.
- **Branding & Logo Settings**: Live surface preview panels.
- **Media Manager & Picker**: Image cropping, scaling, compression to WebP, directory structure folders.

---

## Technical Performance & Accessibility Standards
- **Performance**: Virtual Scroll, Deferrable Views, Tree Shaking, Image Lazy Loading.
- **Accessibility**: WCAG AA standards (Keyboard navigation focus traps, ARIA labels, high contrast, reduced motion checks).
- **State Management**: Computed signals, local feature states.
