namespace GastroErp.Application.Common.Interfaces.Authentication;

public interface IAuthSessionService
{
    Task<Guid> CreateSessionAsync(
        Guid tenantId,
        Guid userId,
        string refreshToken,
        string deviceId,
        string? deviceName = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    Task<bool> ValidateRefreshTokenAsync(
        Guid tenantId,
        Guid userId,
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<string?> RotateRefreshTokenAsync(
        Guid tenantId,
        Guid userId,
        string currentRefreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeAllSessionsForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task CleanupExpiredAsync(CancellationToken cancellationToken = default);
}
