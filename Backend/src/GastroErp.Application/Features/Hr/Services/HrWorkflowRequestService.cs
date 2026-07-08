using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Hr.DTOs;
using GastroErp.Domain.Entities.HR;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Hr.Services;

public interface IHrWorkflowRequestService
{
    Task<HrWorkflowRequestDto> SubmitAsync(Guid tenantId, SubmitHrWorkflowRequestDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<HrWorkflowRequestDto>> ListAsync(Guid tenantId, Guid? employeeId, HrWorkflowRequestType? type, CancellationToken ct = default);
}

public sealed class HrWorkflowRequestService : IHrWorkflowRequestService
{
    private readonly IApplicationDbContext _context;

    public HrWorkflowRequestService(IApplicationDbContext context) => _context = context;

    public async Task<HrWorkflowRequestDto> SubmitAsync(Guid tenantId, SubmitHrWorkflowRequestDto dto, CancellationToken ct = default)
    {
        var req = HrWorkflowRequest.Submit(tenantId, dto.EmployeeId, dto.RequestType, dto.Title,
            dto.Description, dto.Amount, dto.MetadataJson);
        _context.HrWorkflowRequests.Add(req);
        await _context.SaveChangesAsync(ct);
        return Map(req);
    }

    public async Task<IReadOnlyList<HrWorkflowRequestDto>> ListAsync(
        Guid tenantId, Guid? employeeId, HrWorkflowRequestType? type, CancellationToken ct = default)
    {
        var q = _context.HrWorkflowRequests.AsNoTracking().Where(r => r.TenantId == tenantId);
        if (employeeId.HasValue) q = q.Where(r => r.EmployeeId == employeeId);
        if (type.HasValue) q = q.Where(r => r.RequestType == type);
        return await q.OrderByDescending(r => r.CreatedAt).Take(100)
            .Select(r => new HrWorkflowRequestDto(r.Id, r.EmployeeId, r.RequestType, r.Status, r.Title, r.Amount, r.CreatedAt))
            .ToListAsync(ct);
    }

    private static HrWorkflowRequestDto Map(HrWorkflowRequest r)
        => new(r.Id, r.EmployeeId, r.RequestType, r.Status, r.Title, r.Amount, r.CreatedAt);
}
