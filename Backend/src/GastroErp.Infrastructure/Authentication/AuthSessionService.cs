using System.Security.Cryptography;
using System.Text;
using GastroErp.Application.Common.Authorization;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Authentication;
using GastroErp.Application.Common.Options;
using GastroErp.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GastroErp.Infrastructure.Authentication;

public sealed class AuthSessionService : IAuthSessionService
{
    private readonly IApplicationDbContext _context;
    private readonly AuthJwtSettings _jwtSettings;

    public AuthSessionService(IApplicationDbContext context, IOptions<AuthJwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<Guid> CreateSessionAsync(
        Guid tenantId,
        Guid userId,
        string refreshToken,
        string deviceId,
        string? deviceName = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);
        var session = new UserSession(tenantId, userId, deviceId, expiresAt, deviceName, ipAddress, userAgent);
        _context.UserSessions.Add(session);

        var token = new RefreshTokenEntity(tenantId, userId, session.Id, HashToken(refreshToken), expiresAt);
        _context.RefreshTokens.Add(token);

        await _context.SaveChangesAsync(cancellationToken);
        return session.Id;
    }

    public async Task<bool> ValidateRefreshTokenAsync(
        Guid tenantId,
        Guid userId,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var hash = HashToken(refreshToken);
        return await _context.RefreshTokens.AnyAsync(
            t => t.TenantId == tenantId &&
                 t.UserId == userId &&
                 t.TokenHash == hash &&
                 !t.IsRevoked &&
                 t.ExpiresAt > DateTimeOffset.UtcNow,
            cancellationToken);
    }

    public async Task<string?> RotateRefreshTokenAsync(
        Guid tenantId,
        Guid userId,
        string currentRefreshToken,
        CancellationToken cancellationToken = default)
    {
        var hash = HashToken(currentRefreshToken);
        var existing = await _context.RefreshTokens
            .FirstOrDefaultAsync(
                t => t.TenantId == tenantId &&
                     t.UserId == userId &&
                     t.TokenHash == hash &&
                     !t.IsRevoked &&
                     t.ExpiresAt > DateTimeOffset.UtcNow,
                cancellationToken);

        if (existing is null)
        {
            return null;
        }

        var newToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var newHash = HashToken(newToken);
        existing.Revoke(newHash);

        var replacement = new RefreshTokenEntity(
            tenantId,
            userId,
            existing.SessionId,
            newHash,
            DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays));

        _context.RefreshTokens.Add(replacement);
        await _context.SaveChangesAsync(cancellationToken);
        return newToken;
    }

    public async Task RevokeAllSessionsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke();
        }

        var sessionIds = tokens.Select(t => t.SessionId).Distinct().ToList();
        var sessions = await _context.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.Revoke();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeSessionAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var hash = HashToken(refreshToken);
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.TokenHash == hash && !t.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke();
        }

        var sessionIds = tokens.Select(t => t.SessionId).Distinct().ToList();
        var sessions = await _context.UserSessions
            .Where(s => sessionIds.Contains(s.Id) && !s.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.Revoke();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expiredTokens = await _context.RefreshTokens
            .Where(t => t.ExpiresAt <= now || t.IsRevoked)
            .ToListAsync(cancellationToken);

        if (expiredTokens.Count > 0)
        {
            _context.RefreshTokens.RemoveRange(expiredTokens);
        }

        var expiredSessions = await _context.UserSessions
            .Where(s => s.ExpiresAt <= now || s.IsRevoked)
            .ToListAsync(cancellationToken);

        if (expiredSessions.Count > 0)
        {
            _context.UserSessions.RemoveRange(expiredSessions);
        }

        if (expiredTokens.Count > 0 || expiredSessions.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
