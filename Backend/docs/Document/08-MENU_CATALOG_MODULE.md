# GastroERP — وحدة المنيو والكتالوج
# Menu & Catalog Module — التوثيق الكامل

الإصدار: 1.0
الحالة: قيد التنفيذ

---

# الكيانات المشمولة (14 كيان)

| # | الكيان | النوع | الوصف |
|---|--------|-------|-------|
| 1 | `Category` | Aggregate Root | التصنيف الرئيسي للمنتجات |
| 2 | `Menu` | Aggregate Root | المنيو الكامل للفرع أو الوقت |
| 3 | `MenuSection` | Entity داخل Menu | قسم فرعي داخل المنيو |
| 4 | `MenuItem` | Entity داخل Menu | ارتباط منتج بقسم في المنيو |
| 5 | `ModifierGroup` | Aggregate Root | مجموعة إضافات (مثل: حجم، إضافات) |
| 6 | `Modifier` | Entity داخل ModifierGroup | إضافة واحدة (مثل: كبير، صغير) |
| 7 | `OptionGroup` | Aggregate Root | مجموعة خيارات إلزامية أو اختيارية |
| 8 | `Option` | Entity داخل OptionGroup | خيار واحد (مثل: بدون بصل) |
| 9 | `PriceLevel` | Aggregate Root | مستوى تسعير مستقل (محلي / سفري / توصيل) |
| 10 | `BranchMenu` | Entity | ربط منيو بفرع محدد |
| 11 | `MenuAvailability` | Entity | أوقات توفر المنيو أو المنتج |
| 12 | `ComboMeal` | Aggregate Root | وجبة كومبو مجمّعة |
| 13 | `ComboItem` | Entity داخل ComboMeal | عنصر داخل وجبة الكومبو |
| 14 | `ProductImage` | Entity | صورة مرتبطة بمنتج أو منيو |

---

# 1. تحليل الأعمال (Business Analysis)

## لماذا هذه الوحدة؟

وحدة المنيو هي **قلب التجربة التشغيلية** في نظام المطاعم.
تحدد ما يُعرض على العميل، بأي سعر، وفي أي وقت، وعبر أي قناة بيع.

## التدفق العام

```
Category
  └── لكل فئة → عدة MenuItems (منتجات)
      └── كل MenuItem يرتبط بـ:
          ├── ModifierGroups (إضافات اختيارية/إلزامية)
          ├── OptionGroups (خيارات الطلب)
          ├── PriceLevel (أسعار متعددة)
          └── ProductImages (صور)

Menu
  └── MenuSections (أقسام)
      └── MenuItems (منتجات داخل القسم)

BranchMenu → يربط Menu بـ Branch مع MenuAvailability

ComboMeal → يجمع عدة ComboItems من MenuItems
```

## السياقات والقنوات

- **Dine-In (محلي)**: منيو الطاولات مع أسعار مختلفة
- **Take Away (سفري)**: نفس المنيو أو منيو مستقل
- **Delivery (توصيل)**: منيو مع أسعار توصيل مختلفة
- **Kiosk (كيوسك)**: منيو مبسّط للطلب الذاتي
- **Online (تطبيق)**: منيو رقمي مع صور وأوصاف مفصّلة

---

# 2. قواعد العمل (Business Rules)

## Category
- الفئة يمكن أن تكون لها فئة أم (Hierarchy)
- الفئة المحذوفة لا تظهر في المنيو
- الفئة المعطّلة تُخفي جميع منتجاتها

## Menu
- كل فرع يمكنه ربط أكثر من منيو (محلي، توصيل، صباحي، مسائي)
- المنيو المعطّل لا يظهر للعملاء أو الكاشير
- المنيو له تاريخ بداية ونهاية (موسمي)

## MenuItem
- المنتج في المنيو قد يكون له سعر مختلف عن السعر الأساسي
- المنتج قد يكون متوفراً في بعض الأوقات فقط (Availability)
- المنتج يمكن إخفاؤه من المنيو مؤقتاً دون حذفه

## ModifierGroup
- كل مجموعة إضافات إما إلزامية (MinSelection ≥ 1) أو اختيارية
- MaxSelection يحدد الحد الأقصى للاختيارات
- كل Modifier له سعر إضافي (قد يكون صفراً)

## PriceLevel
- كل فرع يمكنه تعيين مستوى سعر مختلف
- المنتج يمكن أن يكون له سعر مختلف لكل مستوى

## ComboMeal
- الكومبو يتكون من عدة عناصر محددة
- كل عنصر يمكن أن يكون له بدائل من نفس الفئة
- سعر الكومبو أقل من مجموع الأفراد

---

# 3. مخطط ERD

```
┌─────────────┐
│  Categories │◄──────────────────────────┐
│─────────────│                           │
│ Id          │                           │ ParentId (self-ref)
│ TenantId    │                           │
│ Name        │                      ┌────┘
│ ParentId    │──────────────────────>│
│ SortOrder   │
└──────┬──────┘
       │ 1..*
       │
┌──────▼──────┐       ┌────────────────┐       ┌─────────────────┐
│  MenuItems  │──────>│ ModifierGroups │──────>│    Modifiers    │
│─────────────│       │────────────────│       │─────────────────│
│ Id          │  *..* │ Id             │  1..* │ Id              │
│ TenantId    │       │ Name           │       │ Name            │
│ CategoryId  │       │ MinSelection   │       │ ExtraPrice      │
│ Name        │       │ MaxSelection   │       │ IsDefault       │
│ BasePrice   │       │ IsRequired     │       └─────────────────┘
│ SKU         │       └────────────────┘
└──────┬──────┘
       │           ┌──────────────────┐       ┌─────────────────┐
       │           │   OptionGroups   │──────>│     Options     │
       │──────────>│──────────────────│  1..* │─────────────────│
       │       *..* │ Id              │       │ Id              │
       │           │ Name            │       │ Name            │
       │           │ IsRequired      │       │ ExtraPrice      │
       │           └──────────────────┘       └─────────────────┘
       │
       │           ┌─────────────────┐
       │──────────>│  ProductImages  │
       │       1..* │─────────────────│
       │           │ Id              │
       │           │ ImageUrl        │
       │           │ IsPrimary       │
       │           └─────────────────┘
       │
       │           ┌─────────────────┐
       │──────────>│  PriceLevels    │
               1..* │─────────────────│
                   │ Id              │
                   │ Name            │
                   │ PriceMultiplier │
                   └─────────────────┘

┌──────────┐       ┌───────────────┐       ┌─────────────────────┐
│  Menus   │──────>│ MenuSections  │──────>│     MenuItems       │
│──────────│  1..* │───────────────│  1..* │─────────────────────│
│ Id       │       │ Id            │       │ MenuSectionId       │
│ TenantId │       │ Name          │       │ ProductId           │
│ Name     │       │ SortOrder     │       │ OverridePrice       │
│ Type     │       └───────────────┘       │ IsVisible           │
└──────┬───┘                               └─────────────────────┘
       │
       │ *..* via BranchMenu
┌──────▼────────┐       ┌────────────────────────┐
│  BranchMenus  │──────>│    MenuAvailability    │
│───────────────│  1..* │────────────────────────│
│ BranchId      │       │ DayOfWeek              │
│ MenuId        │       │ StartTime              │
│ IsActive      │       │ EndTime                │
└───────────────┘       └────────────────────────┘

┌──────────────┐       ┌────────────────┐
│  ComboMeals  │──────>│   ComboItems   │
│──────────────│  1..* │────────────────│
│ Id           │       │ Id             │
│ Name         │       │ ComboMealId    │
│ ComboPrice   │       │ MenuItemId     │
│ StartDate    │       │ Quantity       │
│ EndDate      │       │ CategoryFilter │
└──────────────┘       └────────────────┘
```

---

# 4. جداول SQL Server

## جدول Categories (التصنيفات)

```sql
CREATE TABLE Categories (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    ParentCategoryId    UNIQUEIDENTIFIER    NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    Description         NVARCHAR(500)       NULL,
    ImageUrl            NVARCHAR(500)       NULL,
    Color               NVARCHAR(7)         NULL,  -- HEX Color للتمييز البصري
    Icon                NVARCHAR(100)       NULL,
    SortOrder           INT                 NOT NULL DEFAULT 0,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    DeletedAt           DATETIMEOFFSET      NULL,
    DeletedBy           NVARCHAR(200)       NULL,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_Categories PRIMARY KEY (Id),
    CONSTRAINT FK_Categories_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentCategoryId) REFERENCES Categories(Id)
);
CREATE INDEX IX_Categories_TenantId ON Categories(TenantId) WHERE IsDeleted = 0;
CREATE INDEX IX_Categories_ParentId ON Categories(ParentCategoryId) WHERE IsDeleted = 0;
CREATE INDEX IX_Categories_SortOrder ON Categories(TenantId, SortOrder) WHERE IsDeleted = 0;
```

## جدول Products / MenuItems (المنتجات)

```sql
CREATE TABLE Products (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    CategoryId          UNIQUEIDENTIFIER    NOT NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    Description         NVARCHAR(1000)      NULL,
    DescriptionAr       NVARCHAR(1000)      NULL,
    SKU                 NVARCHAR(50)        NULL,
    Barcode             NVARCHAR(50)        NULL,
    BasePrice           DECIMAL(18,2)       NOT NULL,
    Currency            NVARCHAR(3)         NOT NULL DEFAULT 'SAR',
    CaloriesMin         INT                 NULL,
    CaloriesMax         INT                 NULL,
    PrepTimeMinutes     INT                 NULL DEFAULT 0,
    IsAvailable         BIT                 NOT NULL DEFAULT 1,
    IsFeatured          BIT                 NOT NULL DEFAULT 0,
    HasModifiers        BIT                 NOT NULL DEFAULT 0,
    SortOrder           INT                 NOT NULL DEFAULT 0,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    DeletedAt           DATETIMEOFFSET      NULL,
    DeletedBy           NVARCHAR(200)       NULL,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_Products PRIMARY KEY (Id),
    CONSTRAINT FK_Products_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT UQ_Products_SKU UNIQUE (TenantId, SKU)
);
CREATE INDEX IX_Products_TenantId ON Products(TenantId) WHERE IsDeleted = 0;
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId) WHERE IsDeleted = 0;
```

## جدول ModifierGroups (مجموعات الإضافات)

```sql
CREATE TABLE ModifierGroups (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    ProductId           UNIQUEIDENTIFIER    NOT NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    MinSelection        INT                 NOT NULL DEFAULT 0,
    MaxSelection        INT                 NOT NULL DEFAULT 1,
    IsRequired          BIT                 NOT NULL DEFAULT 0,
    SortOrder           INT                 NOT NULL DEFAULT 0,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_ModifierGroups PRIMARY KEY (Id),
    CONSTRAINT FK_ModifierGroups_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT FK_ModifierGroups_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT CK_ModifierGroups_Selection CHECK (MaxSelection >= MinSelection AND MaxSelection >= 1)
);
CREATE INDEX IX_ModifierGroups_ProductId ON ModifierGroups(ProductId) WHERE IsDeleted = 0;
```

## جدول Modifiers (الإضافات)

```sql
CREATE TABLE Modifiers (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    ModifierGroupId     UNIQUEIDENTIFIER    NOT NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    ExtraPrice          DECIMAL(18,2)       NOT NULL DEFAULT 0,
    CaloriesExtra       INT                 NULL,
    IsDefault           BIT                 NOT NULL DEFAULT 0,
    SortOrder           INT                 NOT NULL DEFAULT 0,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_Modifiers PRIMARY KEY (Id),
    CONSTRAINT FK_Modifiers_Groups FOREIGN KEY (ModifierGroupId) REFERENCES ModifierGroups(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Modifiers_ExtraPrice CHECK (ExtraPrice >= 0)
);
CREATE INDEX IX_Modifiers_GroupId ON Modifiers(ModifierGroupId) WHERE IsDeleted = 0;
```

## جدول OptionGroups (مجموعات الخيارات)

```sql
CREATE TABLE OptionGroups (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    ProductId           UNIQUEIDENTIFIER    NOT NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    IsRequired          BIT                 NOT NULL DEFAULT 0,
    SortOrder           INT                 NOT NULL DEFAULT 0,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_OptionGroups PRIMARY KEY (Id),
    CONSTRAINT FK_OptionGroups_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
CREATE INDEX IX_OptionGroups_ProductId ON OptionGroups(ProductId) WHERE IsDeleted = 0;
```

## جدول Options (الخيارات)

```sql
CREATE TABLE Options (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    OptionGroupId       UNIQUEIDENTIFIER    NOT NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    ExtraPrice          DECIMAL(18,2)       NOT NULL DEFAULT 0,
    IsDefault           BIT                 NOT NULL DEFAULT 0,
    SortOrder           INT                 NOT NULL DEFAULT 0,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_Options PRIMARY KEY (Id),
    CONSTRAINT FK_Options_OptionGroups FOREIGN KEY (OptionGroupId) REFERENCES OptionGroups(Id) ON DELETE CASCADE
);
CREATE INDEX IX_Options_GroupId ON Options(OptionGroupId) WHERE IsDeleted = 0;
```

## جدول PriceLevels (مستويات الأسعار)

```sql
CREATE TABLE PriceLevels (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    Name                NVARCHAR(100)       NOT NULL,
    NameAr              NVARCHAR(100)       NULL,
    SalesChannel        TINYINT             NOT NULL, -- DineIn, TakeAway, Delivery, Kiosk
    IsDefault           BIT                 NOT NULL DEFAULT 0,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_PriceLevels PRIMARY KEY (Id),
    CONSTRAINT FK_PriceLevels_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);

-- جدول أسعار المنتج لكل مستوى
CREATE TABLE ProductPriceLevels (
    ProductId           UNIQUEIDENTIFIER    NOT NULL,
    PriceLevelId        UNIQUEIDENTIFIER    NOT NULL,
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    Price               DECIMAL(18,2)       NOT NULL,
    CONSTRAINT PK_ProductPriceLevels PRIMARY KEY (ProductId, PriceLevelId),
    CONSTRAINT FK_PPL_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PPL_PriceLevels FOREIGN KEY (PriceLevelId) REFERENCES PriceLevels(Id) ON DELETE CASCADE,
    CONSTRAINT CK_ProductPriceLevels_Price CHECK (Price >= 0)
);
```

## جدول Menus (المنيوهات)

```sql
CREATE TABLE Menus (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    Description         NVARCHAR(500)       NULL,
    MenuType            TINYINT             NOT NULL, -- Standard, Seasonal, Digital, Kiosk
    SalesChannel        TINYINT             NOT NULL, -- DineIn, TakeAway, Delivery, All
    StartDate           DATE                NULL,
    EndDate             DATE                NULL,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    DeletedAt           DATETIMEOFFSET      NULL,
    DeletedBy           NVARCHAR(200)       NULL,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_Menus PRIMARY KEY (Id),
    CONSTRAINT FK_Menus_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT CK_Menus_Dates CHECK (EndDate IS NULL OR EndDate >= StartDate)
);
CREATE INDEX IX_Menus_TenantId ON Menus(TenantId) WHERE IsDeleted = 0;
```

## جدول MenuSections (أقسام المنيو)

```sql
CREATE TABLE MenuSections (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    MenuId              UNIQUEIDENTIFIER    NOT NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    Description         NVARCHAR(500)       NULL,
    ImageUrl            NVARCHAR(500)       NULL,
    SortOrder           INT                 NOT NULL DEFAULT 0,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_MenuSections PRIMARY KEY (Id),
    CONSTRAINT FK_MenuSections_Menus FOREIGN KEY (MenuId) REFERENCES Menus(Id) ON DELETE CASCADE
);
CREATE INDEX IX_MenuSections_MenuId ON MenuSections(MenuId) WHERE IsDeleted = 0;
```

## جدول MenuItems (عناصر المنيو)

```sql
CREATE TABLE MenuItems (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    MenuSectionId       UNIQUEIDENTIFIER    NOT NULL,
    ProductId           UNIQUEIDENTIFIER    NOT NULL,
    OverridePrice       DECIMAL(18,2)       NULL, -- NULL = استخدام السعر الأساسي
    SortOrder           INT                 NOT NULL DEFAULT 0,
    IsVisible           BIT                 NOT NULL DEFAULT 1,
    IsOutOfStock        BIT                 NOT NULL DEFAULT 0,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_MenuItems PRIMARY KEY (Id),
    CONSTRAINT FK_MenuItems_Sections FOREIGN KEY (MenuSectionId) REFERENCES MenuSections(Id) ON DELETE CASCADE,
    CONSTRAINT FK_MenuItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT UQ_MenuItems_Section_Product UNIQUE (MenuSectionId, ProductId)
);
CREATE INDEX IX_MenuItems_SectionId ON MenuItems(MenuSectionId) WHERE IsDeleted = 0;
CREATE INDEX IX_MenuItems_ProductId ON MenuItems(ProductId) WHERE IsDeleted = 0;
```

## جدول BranchMenus (ربط المنيو بالفرع)

```sql
CREATE TABLE BranchMenus (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    BranchId            UNIQUEIDENTIFIER    NOT NULL,
    MenuId              UNIQUEIDENTIFIER    NOT NULL,
    PriceLevelId        UNIQUEIDENTIFIER    NULL,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    SortOrder           INT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    CONSTRAINT PK_BranchMenus PRIMARY KEY (Id),
    CONSTRAINT FK_BranchMenus_Branches FOREIGN KEY (BranchId) REFERENCES Branches(Id),
    CONSTRAINT FK_BranchMenus_Menus FOREIGN KEY (MenuId) REFERENCES Menus(Id),
    CONSTRAINT UQ_BranchMenus_Branch_Menu UNIQUE (BranchId, MenuId)
);
CREATE INDEX IX_BranchMenus_BranchId ON BranchMenus(BranchId);
```

## جدول MenuAvailability (أوقات توفر المنيو)

```sql
CREATE TABLE MenuAvailabilities (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    BranchMenuId        UNIQUEIDENTIFIER    NOT NULL,
    DayOfWeek           TINYINT             NOT NULL, -- 0=Sun, 6=Sat
    StartTime           TIME                NOT NULL,
    EndTime             TIME                NOT NULL,
    CONSTRAINT PK_MenuAvailabilities PRIMARY KEY (Id),
    CONSTRAINT FK_MA_BranchMenus FOREIGN KEY (BranchMenuId) REFERENCES BranchMenus(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_MA_BranchMenu_Day UNIQUE (BranchMenuId, DayOfWeek),
    CONSTRAINT CK_MA_Times CHECK (EndTime > StartTime)
);
CREATE INDEX IX_MenuAvailabilities_BranchMenuId ON MenuAvailabilities(BranchMenuId);
```

## جدول ComboMeals (وجبات الكومبو)

```sql
CREATE TABLE ComboMeals (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    Name                NVARCHAR(200)       NOT NULL,
    NameAr              NVARCHAR(200)       NULL,
    Description         NVARCHAR(500)       NULL,
    ComboPrice          DECIMAL(18,2)       NOT NULL,
    Currency            NVARCHAR(3)         NOT NULL DEFAULT 'SAR',
    StartDate           DATE                NULL,
    EndDate             DATE                NULL,
    ImageUrl            NVARCHAR(500)       NULL,
    IsActive            BIT                 NOT NULL DEFAULT 1,
    IsDeleted           BIT                 NOT NULL DEFAULT 0,
    DeletedAt           DATETIMEOFFSET      NULL,
    DeletedBy           NVARCHAR(200)       NULL,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    UpdatedAt           DATETIMEOFFSET      NULL,
    UpdatedBy           NVARCHAR(200)       NULL,
    RowVersion          ROWVERSION          NOT NULL,
    CONSTRAINT PK_ComboMeals PRIMARY KEY (Id),
    CONSTRAINT FK_ComboMeals_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT CK_ComboMeals_Price CHECK (ComboPrice >= 0),
    CONSTRAINT CK_ComboMeals_Dates CHECK (EndDate IS NULL OR EndDate >= StartDate)
);
CREATE INDEX IX_ComboMeals_TenantId ON ComboMeals(TenantId) WHERE IsDeleted = 0;
```

## جدول ComboItems (عناصر الكومبو)

```sql
CREATE TABLE ComboItems (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    ComboMealId         UNIQUEIDENTIFIER    NOT NULL,
    ProductId           UNIQUEIDENTIFIER    NOT NULL,
    Quantity            INT                 NOT NULL DEFAULT 1,
    AllowSubstitution   BIT                 NOT NULL DEFAULT 0,
    SubstitutionCategoryId UNIQUEIDENTIFIER NULL, -- فئة البدائل المسموحة
    SortOrder           INT                 NOT NULL DEFAULT 0,
    CONSTRAINT PK_ComboItems PRIMARY KEY (Id),
    CONSTRAINT FK_ComboItems_ComboMeals FOREIGN KEY (ComboMealId) REFERENCES ComboMeals(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ComboItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT CK_ComboItems_Quantity CHECK (Quantity >= 1)
);
CREATE INDEX IX_ComboItems_ComboMealId ON ComboItems(ComboMealId);
```

## جدول ProductImages (صور المنتجات)

```sql
CREATE TABLE ProductImages (
    Id                  UNIQUEIDENTIFIER    NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER    NOT NULL,
    ProductId           UNIQUEIDENTIFIER    NOT NULL,
    ImageUrl            NVARCHAR(500)       NOT NULL,
    ThumbnailUrl        NVARCHAR(500)       NULL,
    AltText             NVARCHAR(200)       NULL,
    IsPrimary           BIT                 NOT NULL DEFAULT 0,
    SortOrder           INT                 NOT NULL DEFAULT 0,
    CreatedAt           DATETIMEOFFSET      NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy           NVARCHAR(200)       NULL,
    CONSTRAINT PK_ProductImages PRIMARY KEY (Id),
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
);
CREATE INDEX IX_ProductImages_ProductId ON ProductImages(ProductId);
```

---

# 5. EF Core Configurations

```csharp
// ─── Category Configuration ───────────────────────────────────────────────────
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.NameAr).HasMaxLength(200);
        builder.Property(c => c.ImageUrl).HasMaxLength(500);
        builder.Property(c => c.Color).HasMaxLength(7);
        builder.Property(c => c.Icon).HasMaxLength(100);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Self-referencing hierarchy
        builder.HasOne(c => c.ParentCategory)
               .WithMany(c => c.SubCategories)
               .HasForeignKey(c => c.ParentCategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.TenantId, c.SortOrder });
    }
}

// ─── Product Configuration ────────────────────────────────────────────────────
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.BasePrice).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(3);
        builder.Property(p => p.SKU).HasMaxLength(50);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Price — Owned Value Object
        builder.OwnsMany(p => p.PriceLevels, pl => {
            pl.ToTable("ProductPriceLevels");
            pl.Property(x => x.Price).HasPrecision(18, 2);
        });

        // Modifier Groups
        builder.HasMany(p => p.ModifierGroups)
               .WithOne()
               .HasForeignKey(mg => mg.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        // Option Groups
        builder.HasMany(p => p.OptionGroups)
               .WithOne()
               .HasForeignKey(og => og.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        // Images
        builder.HasMany(p => p.Images)
               .WithOne()
               .HasForeignKey(img => img.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.TenantId, p.SKU }).IsUnique().HasFilter("[IsDeleted] = 0 AND [SKU] IS NOT NULL");
    }
}

// ─── ModifierGroup Configuration ─────────────────────────────────────────────
public class ModifierGroupConfiguration : IEntityTypeConfiguration<ModifierGroup>
{
    public void Configure(EntityTypeBuilder<ModifierGroup> builder)
    {
        builder.ToTable("ModifierGroups");
        builder.HasKey(mg => mg.Id);
        builder.Property(mg => mg.Name).IsRequired().HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(mg => !mg.IsDeleted);

        builder.HasMany(mg => mg.Modifiers)
               .WithOne()
               .HasForeignKey(m => m.ModifierGroupId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

// ─── Menu Configuration ───────────────────────────────────────────────────────
public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
        builder.Property(m => m.MenuType).HasConversion<byte>();
        builder.Property(m => m.SalesChannel).HasConversion<byte>();
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(m => !m.IsDeleted);

        builder.HasMany(m => m.Sections)
               .WithOne()
               .HasForeignKey(s => s.MenuId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

// ─── ComboMeal Configuration ──────────────────────────────────────────────────
public class ComboMealConfiguration : IEntityTypeConfiguration<ComboMeal>
{
    public void Configure(EntityTypeBuilder<ComboMeal> builder)
    {
        builder.ToTable("ComboMeals");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.ComboPrice).HasPrecision(18, 2);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasMany(c => c.Items)
               .WithOne()
               .HasForeignKey(ci => ci.ComboMealId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

# 6. CQRS Checklist

## التصنيفات (Categories)
- [ ] CreateCategoryCommand + Handler
- [ ] UpdateCategoryCommand + Handler
- [ ] DeactivateCategoryCommand + Handler
- [ ] ReorderCategoriesCommand + Handler
- [ ] GetCategoriesTreeQuery + Handler
- [ ] GetCategoryByIdQuery + Handler

## المنتجات (Products)
- [ ] CreateProductCommand + Handler
- [ ] UpdateProductCommand + Handler
- [ ] SetProductAvailabilityCommand + Handler
- [ ] UpdateProductPricesCommand + Handler
- [ ] AddModifierGroupCommand + Handler
- [ ] RemoveModifierGroupCommand + Handler
- [ ] UploadProductImageCommand + Handler
- [ ] GetProductByIdQuery + Handler
- [ ] GetProductsByCategoryQuery + Handler
- [ ] SearchProductsQuery + Handler

## المنيو (Menus)
- [ ] CreateMenuCommand + Handler
- [ ] AddMenuSectionCommand + Handler
- [ ] AddMenuItemCommand + Handler
- [ ] RemoveMenuItemCommand + Handler
- [ ] AssignMenuToBranchCommand + Handler
- [ ] SetMenuAvailabilityCommand + Handler
- [ ] GetBranchActiveMenusQuery + Handler

## الكومبو (Combos)
- [ ] CreateComboMealCommand + Handler
- [ ] AddComboItemCommand + Handler
- [ ] UpdateComboPriceCommand + Handler
- [ ] GetActiveComboMealsQuery + Handler

---

# 7. قواعد التكامل مع وحدات أخرى (Integration Events)

| الحدث | المُنتِج | المستمع |
|-------|---------|--------|
| `ProductCreatedEvent` | Menu Module | Inventory Module (إنشاء عنصر مخزون) |
| `ProductPriceChangedEvent` | Menu Module | Accounting Module (تحديث الأسعار) |
| `MenuActivatedEvent` | Menu Module | POS Module (تحديث الكاشير) |
| `ComboMealCreatedEvent` | Menu Module | POS Module (إضافة للكاشير) |

---

الحالة: الوثيقة مكتملة — كود الكيانات قيد الإنشاء
