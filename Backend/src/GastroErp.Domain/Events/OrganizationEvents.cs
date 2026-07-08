using GastroErp.Domain.Common;

namespace GastroErp.Domain.Events.Organization;

/// <summary>حدث: تم إنشاء مستأجر جديد</summary>
public sealed record TenantCreatedEvent(Guid TenantId, string Name, string Slug) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تعليق المستأجر</summary>
public sealed record TenantSuspendedEvent(Guid TenantId, string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تفعيل المستأجر</summary>
public sealed record TenantActivatedEvent(Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم إنشاء اشتراك جديد</summary>
public sealed record SubscriptionCreatedEvent(Guid SubscriptionId, Guid TenantId, DateTimeOffset EndDate) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تجديد الاشتراك</summary>
public sealed record SubscriptionRenewedEvent(Guid SubscriptionId, Guid TenantId, DateTimeOffset NewEndDate) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: انتهت صلاحية الاشتراك</summary>
public sealed record SubscriptionExpiredEvent(Guid SubscriptionId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تعليق الاشتراك</summary>
public sealed record SubscriptionSuspendedEvent(Guid SubscriptionId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم استئناف الاشتراك</summary>
public sealed record SubscriptionResumedEvent(Guid SubscriptionId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم إنشاء شركة جديدة</summary>
public sealed record CompanyCreatedEvent(Guid CompanyId, Guid TenantId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم إيقاف الشركة</summary>
public sealed record CompanyDeactivatedEvent(Guid CompanyId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// حدث: تم تفعيل الشركة بعد إيقافها
/// Raised when a previously inactive company is reactivated.
/// </summary>
public sealed record CompanyActivatedEvent(Guid CompanyId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// حدث: تم تعديل البيانات القانونية للشركة
/// Raised when the company's legal information (name, VAT, commercial register) is updated.
/// </summary>
public sealed record CompanyLegalInfoUpdatedEvent(Guid CompanyId, Guid TenantId, string NewName) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم إنشاء فرع جديد</summary>
public sealed record BranchCreatedEvent(Guid BranchId, Guid CompanyId, Guid TenantId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم إيقاف الفرع</summary>
public sealed record BranchDeactivatedEvent(Guid BranchId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم أرشفة الفرع</summary>
public sealed record BranchArchivedEvent(Guid BranchId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم استعادة الفرع</summary>
public sealed record BranchRestoredEvent(Guid BranchId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تغيير عنوان الفرع</summary>
public sealed record BranchAddressChangedEvent(Guid BranchId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم إنشاء مستخدم جديد</summary>
public sealed record UserCreatedEvent(Guid UserId, Guid TenantId, string Email) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم إيقاف المستخدم</summary>
public sealed record UserDeactivatedEvent(Guid UserId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم قفل حساب المستخدم</summary>
public sealed record UserLockedEvent(Guid UserId, Guid TenantId, DateTimeOffset LockedUntil) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تغيير كلمة مرور المستخدم</summary>
public sealed record UserPasswordChangedEvent(Guid UserId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تسجيل جهاز جديد</summary>
public sealed record DeviceRegisteredEvent(Guid DeviceId, Guid TenantId, string ActivationCode) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تفعيل الجهاز</summary>
public sealed record DeviceActivatedEvent(Guid DeviceId, Guid TenantId, Guid BranchId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
