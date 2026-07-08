namespace GastroErp.Domain.Common;

public interface ISoftDelete
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAt { get; }
    string? DeletedBy { get; }
}

public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; }
    string? CreatedBy { get; }
    DateTimeOffset? UpdatedAt { get; }
    string? UpdatedBy { get; }
}

public interface ITenantEntity
{
    Guid TenantId { get; }
}

public interface ICompanyEntity
{
    Guid CompanyId { get; }
}

public interface IBranchEntity
{
    Guid BranchId { get; }
}
