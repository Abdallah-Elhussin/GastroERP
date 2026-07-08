using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Crm;

public sealed class PromotionCampaign : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public PromotionType Type { get; private set; }
    public decimal Value { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public int Priority { get; private set; }
    public bool Stackable { get; private set; }
    public bool IsActive { get; private set; }

    // Configuration Data (can be JSON or structured depending on requirements)
    public string? ConfigurationJson { get; private set; }

    private PromotionCampaign() { Name = string.Empty; }

    public PromotionCampaign(Guid tenantId, string name, PromotionType type, decimal value, DateTimeOffset startDate, DateTimeOffset endDate, int priority, bool stackable)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");
        
        TenantId = tenantId;
        Name = name;
        Type = type;
        Value = value;
        StartDate = startDate;
        EndDate = endDate;
        Priority = priority;
        Stackable = stackable;
        IsActive = true;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    
    public void UpdateConfiguration(string json)
    {
        ConfigurationJson = json;
    }
}
