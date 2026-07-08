using GastroErp.Domain.Common;

namespace GastroErp.Domain.Events.Menu;

/// <summary>حدث: تم إنشاء تصنيف جديد</summary>
public sealed record CategoryCreatedEvent(Guid CategoryId, Guid TenantId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تعطيل تصنيف</summary>
public sealed record CategoryDeactivatedEvent(Guid CategoryId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم إنشاء منتج جديد</summary>
public sealed record ProductCreatedEvent(Guid ProductId, Guid TenantId, Guid CategoryId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تغيير سعر المنتج</summary>
public sealed record ProductPriceChangedEvent(Guid ProductId, Guid TenantId, decimal OldPrice, decimal NewPrice) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تعطيل منتج (نفاد المخزون أو إخفاء مؤقت)</summary>
public sealed record ProductUnavailableEvent(Guid ProductId, Guid TenantId, string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم إنشاء منيو جديد</summary>
public sealed record MenuCreatedEvent(Guid MenuId, Guid TenantId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تفعيل المنيو</summary>
public sealed record MenuActivatedEvent(Guid MenuId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تعطيل المنيو</summary>
public sealed record MenuDeactivatedEvent(Guid MenuId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم ربط المنيو بفرع</summary>
public sealed record MenuAssignedToBranchEvent(Guid MenuId, Guid BranchId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم إنشاء وجبة كومبو</summary>
public sealed record ComboMealCreatedEvent(Guid ComboMealId, Guid TenantId, string Name, decimal Price) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تعطيل وجبة كومبو</summary>
public sealed record ComboMealDeactivatedEvent(Guid ComboMealId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
