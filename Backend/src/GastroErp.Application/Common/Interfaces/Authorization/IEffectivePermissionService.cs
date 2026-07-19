namespace GastroErp.Application.Common.Interfaces.Authorization;

/// <summary>يحسب الصلاحيات الفعّالة للمستخدم: (أدوار ∪ Allow) − Deny.</summary>
public interface IEffectivePermissionService
{
    Task<IReadOnlyList<string>> GetPermissionNamesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetRolePermissionIdsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, byte>> GetUserOverridesAsync(Guid userId, CancellationToken cancellationToken = default);
}
