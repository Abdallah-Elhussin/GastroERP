using GastroErp.Domain.Common;

namespace GastroErp.Domain.Entities.Organization;

/// <summary>
/// Feature — ميزة النظام (Aggregate Root)
/// يمثل ميزة معينة في النظام يمكن تفعيلها أو تقييدها.
/// </summary>
public sealed class Feature : AuditableBaseEntity
{
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private Feature()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    public Feature(string name, string code, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Feature name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Feature code cannot be empty.", nameof(code));

        Name = name;
        Code = code;
        Description = description;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

/// <summary>
/// SubscriptionFeatureLimit — حدود الميزات في باقة الاشتراك (Entity)
/// </summary>
public sealed class SubscriptionFeatureLimit : AuditableBaseEntity
{
    public Guid SubscriptionPlanId { get; private set; }
    public Guid FeatureId { get; private set; }
    public int Limit { get; private set; }
    public bool IsUnlimited => Limit == -1;

    private SubscriptionFeatureLimit() { }

    public SubscriptionFeatureLimit(Guid subscriptionPlanId, Guid featureId, int limit)
    {
        if (subscriptionPlanId == Guid.Empty) throw new ArgumentException("SubscriptionPlanId cannot be empty.", nameof(subscriptionPlanId));
        if (featureId == Guid.Empty) throw new ArgumentException("FeatureId cannot be empty.", nameof(featureId));
        if (limit < -1) throw new ArgumentException("Limit cannot be less than -1.", nameof(limit));

        SubscriptionPlanId = subscriptionPlanId;
        FeatureId = featureId;
        Limit = limit;
    }

    public void UpdateLimit(int limit)
    {
        if (limit < -1) throw new ArgumentException("Limit cannot be less than -1.", nameof(limit));
        Limit = limit;
    }
}
