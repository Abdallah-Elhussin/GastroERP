namespace GastroErp.Domain.Enums;

/// <summary>أنواع كatalog المنتجات — قابلة للتوسع</summary>
public enum ProductCatalogType
{
    RawMaterial = 1,
    SemiFinished = 2,
    FinishedProduct = 3,
    MenuItem = 4,
    Combo = 5,
    Modifier = 6,
    Bundle = 7,
    Service = 8,
    Voucher = 9,
    GiftCard = 10,
    Packaging = 11,
    Asset = 12,
    Expense = 13
}

/// <summary>حالة تعريف الكatalog</summary>
public enum ProductCatalogStatus
{
    Draft = 1,
    Active = 2,
    Archived = 3,
    PendingApproval = 4
}
