using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Invoicing;

/// <summary>TaxGroup — مجموعة ضرائب (Aggregate Root)</summary>
public sealed class TaxGroup : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public bool IsActive { get; private set; }
    public string? Description { get; private set; }

    private readonly List<TaxGroupRate> _rates = [];
    public IReadOnlyCollection<TaxGroupRate> Rates => _rates.AsReadOnly();

    private TaxGroup() { NameAr = string.Empty; }

    public static TaxGroup Create(Guid tenantId, string nameAr, string? nameEn = null, string? description = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        return new TaxGroup
        {
            TenantId = tenantId,
            NameAr = nameAr,
            NameEn = nameEn,
            Description = description,
            IsActive = true
        };
    }

    public void Update(string nameAr, string? nameEn, string? description)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        Description = description;
    }

    public TaxGroupRate AddRate(Guid taxRateId, int sortOrder = 0)
    {
        if (_rates.Any(r => r.TaxRateId == taxRateId))
            throw new BusinessException(ErrorCodes.TaxRateAlreadyInGroup);

        var rate = new TaxGroupRate(Id, taxRateId, sortOrder);
        _rates.Add(rate);
        return rate;
    }

    public void RemoveRate(Guid taxRateId)
    {
        var rate = _rates.FirstOrDefault(r => r.TaxRateId == taxRateId);
        if (rate is not null) _rates.Remove(rate);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

public sealed class TaxGroupRate : AuditableBaseEntity
{
    public Guid TaxGroupId { get; private set; }
    public Guid TaxRateId { get; private set; }
    public int SortOrder { get; private set; }

    private TaxGroupRate() { }

    internal TaxGroupRate(Guid taxGroupId, Guid taxRateId, int sortOrder)
    {
        TaxGroupId = taxGroupId;
        TaxRateId = taxRateId;
        SortOrder = sortOrder;
    }
}
