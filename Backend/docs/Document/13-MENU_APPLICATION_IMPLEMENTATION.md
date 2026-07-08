# خطة تنفيذ طبقة التطبيق لوحدة Menu (Menu Application Layer)

تهدف هذه المرحلة إلى بناء طبقة Application الخاصة بوحدة Menu باستخدام نمط CQRS وMediatR وربطها مع Domain وPersistence مع الالتزام الكامل بمبادئ Clean Architecture وDDD.

---

# الهدف

تنفيذ جميع عمليات إدارة المنيو داخل طبقة Application بدون أي منطق داخل الـ API.

سيتم استخدام:

- CQRS
- MediatR
- AutoMapper
- FluentValidation
- Result Pattern
- Localization
- Pagination
- Filtering
- Authorization

---

# Proposed Changes

## 1. DTOs

إنشاء DTOs لجميع كيانات المنيو.

يشمل:

- MenuDto
- MenuSectionDto
- MenuItemDto
- ProductDto
- CategoryDto
- ModifierGroupDto
- ModifierOptionDto
- ComboMealDto
- ComboItemDto
- PriceLevelDto
- BranchMenuDto
- MenuAvailabilityDto

وكذلك

CreateDto

UpdateDto

لكل كيان.

---

## 2. Commands

إنشاء Commands لجميع العمليات.

يشمل:

Create

Update

Delete

Activate

Deactivate

Publish

Archive

لكل من:

Category

Product

Menu

MenuSection

MenuItem

ModifierGroup

ModifierOption

ComboMeal

PriceLevel

BranchMenu

MenuAvailability

---

## 3. Command Handlers

كتابة جميع Handlers.

مع:

Business Validation

Repository Access

Domain Methods

Result Pattern

Localization

Domain Events

---

## 4. Queries

إنشاء جميع Queries.

مثل:

GetById

GetAll

GetPaged

Search

Filter

GetActiveMenus

GetMenusByBranch

GetProductsByCategory

GetModifierGroups

GetComboMeals

GetPriceLevels

---

## 5. Query Handlers

تنفيذ جميع Handlers.

مع دعم:

Pagination

Sorting

Filtering

Searching

Projection

AsNoTracking

CancellationToken

---

## 6. Validators

إنشاء FluentValidation.

لكل:

Create Commands

Update Commands

Delete Commands

Publish Commands

Deactivate Commands

ويتم استخدام:

ILocalizationService

ErrorCodes

Validation Codes

بدون أي رسائل ثابتة.

---

## 7. AutoMapper

إنشاء:

MenuMappingProfile

ويحتوي على Mapping لجميع الكيانات.

---

## 8. Authorization

إضافة سياسات الصلاحيات.

مثل:

Menu.Create

Menu.Update

Menu.Delete

Menu.Publish

Product.Create

Product.Update

Category.Manage

Modifier.Manage

Price.Manage

---

## 9. Result Pattern

جميع Handlers يجب أن ترجع:

Result

Result<T>

PagedResult<T>

ولا يتم Throw Exceptions إلا عند أخطاء Domain.

---

## 10. Logging

إضافة Logging داخل العمليات المهمة.

مثل:

Create Menu

Publish Menu

Delete Product

Update Price

Deactivate Category

---

## 11. Performance

استخدام:

CancellationToken

AsNoTracking

ProjectTo

Pagination

حتى تكون جميع الاستعلامات عالية الأداء.

---

## 12. Verification Plan

التأكد من:

- Build ناجح.
- عدم وجود Warnings.
- جميع Validators تعمل.
- جميع AutoMapper Profiles صحيحة.
- جميع Commands مسجلة.
- جميع Queries تعمل.
- جميع DTOs مكتملة.
- جميع العمليات تستخدم Localization.
- عدم وجود Hardcoded Messages.