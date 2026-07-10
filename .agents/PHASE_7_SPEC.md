# Phase 7 Spec: Enterprise Media Manager & Branding Engine

This specification is the visual and functional source of truth for **Phase 7: Media & Branding**.

---

# Part 1: Media Module

## Architecture Structure
Create folders under `src/app/features/media/`:
- `MediaBrowserComponent`
- `MediaGridComponent`
- `MediaListComponent`
- `MediaUploadComponent`
- `MediaPickerComponent`
- `MediaFoldersComponent`
- `MediaDetailsComponent`
- `MediaPreviewComponent`
- `MediaCropDialogComponent`
- `MediaReplaceDialogComponent`
- `MediaDeleteDialogComponent`
- `MediaToolbarComponent`
- `MediaFiltersComponent`

## Folder Management
- Nested folders tree.
- Create, rename, delete, and move folders.
- Favorite and recent folder shortcuts.

## File Management
- Drag & drop single/multi-file uploads.
- Rename, duplicate, copy, move, download, delete, and restore from trash.
- Bulk operations: delete, download, move, compress, convert to WebP, and bulk tagging.
- Search by name, type, tags, folder, date, and size.

## Image Processing
- Crop, rotate, resize, flip, compress, WebP conversion, zoom, and EXIF/metadata previews.
- Auto-generate variants: Thumbnail, Small, Medium, Large, Original.
- Supported extensions: Images, Videos, PDF, Word, Excel, ZIP, SVG, JSON, Documents.
- Permission-based access controls.

## Repository Layer
Create `MediaRepository` and `MockMediaRepository`. Prepared for REST API, S3, Azure Blob, and Local Storage.

---

# Part 2: Branding Engine

## Settings & Controls
Create folders under `src/app/features/branding/`:
- `BrandingRepository`
- `MockBrandingRepository`
- `ThemeRepository`

## Configuration Options
- **Company Branding**: Logos for company, branch, POS, KDS, receipt, invoice, email, application icon, and favicon.
- **Login Branding**: Login background image, overlays, logo, title, subtitle, buttons, and illustrations.
- **Portal Branding**: Sidebar, toolbar, cards, buttons, shadows, radius, and animations.
- **Color System**: CSS variable updates for primary, secondary, success, warning, danger, info, backgrounds, surfaces, and borders (applied without page refresh).
- **Typography**: Arabic and English font bindings, headings scales, body sizes, and line-heights.
- **Receipt/Invoice customization**: Logos, headers, footers, signature areas, and barcode/QR positions.

---

# Part 3: Integration
- Replace all static image assets on Landing, Login, POS, Kitchen, Inventory, CRM, HR, and settings to load dynamically via `MediaRepository`.

---

# Part 4: Verification Checklist
- [ ] Build solution compiles cleanly with 0 Errors and 0 Warnings.
- [ ] Theme changes instantly using CSS variables.
- [ ] Logos and branding updates persist after tab reload.
- [ ] Media manager handles folder uploads and mock image croppers.
