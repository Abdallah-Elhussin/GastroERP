using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Organization;
using GastroErp.Domain.ValueObjects;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Organization;

/// <summary>
/// Tenant — المستأجر (Aggregate Root)
/// يمثل شركة مطاعم مشتركة في نظام GastroERP.
/// هو الجذر الأساسي لعزل البيانات في نظام Multi-Tenant.
/// </summary>
public sealed class Tenant : AuditableBaseEntity
{
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string Slug { get; private set; }
    public TenantStatus Status { get; private set; }
    public string? DatabaseName { get; private set; }
    public string DefaultCurrency { get; private set; }
    public string DefaultLanguage { get; private set; }
    public string DefaultTimezone { get; private set; }
    public TenantBranding Branding { get; private set; }

    private Tenant()
    {
        NameAr = string.Empty;
        Slug = string.Empty;
        DefaultCurrency = "SAR";
        DefaultLanguage = "ar";
        DefaultTimezone = "Arab Standard Time";
        Branding = TenantBranding.Empty;
    }

    public Tenant(string nameAr, string slug, string defaultCurrency = "SAR",
                  string defaultLanguage = "ar", string defaultTimezone = "Arab Standard Time", string? nameEn = null)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Tenant slug cannot be empty.", nameof(slug));
        if (slug.Any(c => !char.IsLetterOrDigit(c) && c != '-'))
            throw new ArgumentException("Slug can only contain letters, digits, and hyphens.", nameof(slug));

        NameAr = nameAr;
        NameEn = nameEn;
        Slug = slug.ToLowerInvariant();
        DefaultCurrency = defaultCurrency;
        DefaultLanguage = defaultLanguage;
        DefaultTimezone = defaultTimezone;
        Branding = TenantBranding.Empty;
        Status = TenantStatus.Active;

        RaiseDomainEvent(new TenantCreatedEvent(Id, NameAr, Slug));
    }

    public void Suspend(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason must be provided for suspension.", nameof(reason));
        if (Status == TenantStatus.Cancelled)
            throw new InvalidOperationException("Cannot suspend a cancelled tenant.");

        Status = TenantStatus.Suspended;
        RaiseDomainEvent(new TenantSuspendedEvent(Id, reason));
    }

    public void Activate()
    {
        if (Status == TenantStatus.Cancelled)
            throw new InvalidOperationException("Cannot activate a cancelled tenant.");

        Status = TenantStatus.Active;
        RaiseDomainEvent(new TenantActivatedEvent(Id));
    }

    public void UpdateBranding(string? logoUrl, string? primaryColor, string? secondaryColor)
        => Branding = new TenantBranding(logoUrl, primaryColor, secondaryColor);

    public void UpdateDefaults(string currency, string language, string timezone)
    {
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency cannot be empty.", nameof(currency));
        if (string.IsNullOrWhiteSpace(language)) throw new ArgumentException("Language cannot be empty.", nameof(language));
        if (string.IsNullOrWhiteSpace(timezone)) throw new ArgumentException("Timezone cannot be empty.", nameof(timezone));

        DefaultCurrency = currency;
        DefaultLanguage = language;
        DefaultTimezone = timezone;
    }

    public void SetDatabaseName(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
            throw new ArgumentException("Database name cannot be empty.", nameof(databaseName));
        DatabaseName = databaseName;
    }

    public bool IsActive => Status == TenantStatus.Active;
}
