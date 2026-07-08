using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Domain.Entities.Organization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Organization.Commands;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<DepartmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateDepartmentCommandHandler> _logger;

    public CreateDepartmentCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateDepartmentCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<DepartmentDto>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = new Department(
            request.Dto.TenantId,
            request.Dto.CompanyId,
            request.Dto.NameAr,
            request.Dto.BranchId,
            request.Dto.ParentDepartmentId,
            request.Dto.NameEn,
            request.Dto.Code
        );

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Department created: {DepartmentId}", department.Id);

        return Result<DepartmentDto>.Success(_mapper.Map<DepartmentDto>(department));
    }
}

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateDepartmentCommandHandler> _logger;

    public UpdateDepartmentCommandHandler(IApplicationDbContext context, ILogger<UpdateDepartmentCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (department == null) return Result.Failure("DepartmentNotFound", "Department not found.");

        department.UpdateName(request.Dto.NameAr, request.Dto.NameEn);
        _context.Departments.Update(department);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Department updated: {DepartmentId}", department.Id);

        return Result.Success();
    }
}

public class DeactivateDepartmentCommandHandler : IRequestHandler<DeactivateDepartmentCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<DeactivateDepartmentCommandHandler> _logger;

    public DeactivateDepartmentCommandHandler(IApplicationDbContext context, ILogger<DeactivateDepartmentCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(DeactivateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (department == null) return Result.Failure("DepartmentNotFound", "Department not found.");

        department.Deactivate();
        _context.Departments.Update(department);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Department deactivated: {DepartmentId}", department.Id);

        return Result.Success();
    }
}
