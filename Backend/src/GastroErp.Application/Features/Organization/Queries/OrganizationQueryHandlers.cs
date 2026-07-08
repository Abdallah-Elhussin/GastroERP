using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Organization.Queries;

public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, Result<BranchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBranchByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<BranchDto>> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.AsNoTracking().FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
        if (branch == null) return Result<BranchDto>.Failure("BranchNotFound", "Branch not found.");
        return Result<BranchDto>.Success(_mapper.Map<BranchDto>(branch));
    }
}

public class GetBranchesQueryHandler : IRequestHandler<GetBranchesQuery, PagedResult<BranchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBranchesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<BranchDto>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Branches.AsNoTracking();

        if (request.TenantId.HasValue) query = query.Where(b => b.TenantId == request.TenantId.Value);
        if (request.CompanyId.HasValue) query = query.Where(b => b.CompanyId == request.CompanyId.Value);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(b => b.NameAr.Contains(request.SearchTerm) || (b.NameEn != null && b.NameEn.Contains(request.SearchTerm)));

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(b => b.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize).ToListAsync(cancellationToken);

        return PagedResult<BranchDto>.Success(_mapper.Map<List<BranchDto>>(items), totalCount, request.PageNumber, request.PageSize);
    }
}

public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, Result<CompanyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCompanyByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<CompanyDto>> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var company = await _context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (company == null) return Result<CompanyDto>.Failure("CompanyNotFound", "Company not found.");
        return Result<CompanyDto>.Success(_mapper.Map<CompanyDto>(company));
    }
}

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, PagedResult<CompanyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCompaniesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Companies.AsNoTracking();
        if (request.TenantId.HasValue) query = query.Where(c => c.TenantId == request.TenantId.Value);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(c => c.NameAr.Contains(request.SearchTerm) || (c.NameEn != null && c.NameEn.Contains(request.SearchTerm)));

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize).ToListAsync(cancellationToken);

        return PagedResult<CompanyDto>.Success(_mapper.Map<List<CompanyDto>>(items), totalCount, request.PageNumber, request.PageSize);
    }
}

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, Result<DepartmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDepartmentByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<DepartmentDto>> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (department == null) return Result<DepartmentDto>.Failure("DepartmentNotFound", "Department not found.");
        return Result<DepartmentDto>.Success(_mapper.Map<DepartmentDto>(department));
    }
}

public class GetDepartmentsQueryHandler : IRequestHandler<GetDepartmentsQuery, PagedResult<DepartmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDepartmentsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<DepartmentDto>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Departments.AsNoTracking();
        if (request.TenantId.HasValue) query = query.Where(d => d.TenantId == request.TenantId.Value);
        if (request.CompanyId.HasValue) query = query.Where(d => d.CompanyId == request.CompanyId.Value);
        if (request.BranchId.HasValue) query = query.Where(d => d.BranchId == request.BranchId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(d => d.NameAr)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize).ToListAsync(cancellationToken);

        return PagedResult<DepartmentDto>.Success(_mapper.Map<List<DepartmentDto>>(items), totalCount, request.PageNumber, request.PageSize);
    }
}

public class GetDeviceByIdQueryHandler : IRequestHandler<GetDeviceByIdQuery, Result<DeviceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDeviceByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<DeviceDto>> Handle(GetDeviceByIdQuery request, CancellationToken cancellationToken)
    {
        var device = await _context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        if (device == null) return Result<DeviceDto>.Failure("DeviceNotFound", "Device not found.");
        return Result<DeviceDto>.Success(_mapper.Map<DeviceDto>(device));
    }
}

public class GetDevicesQueryHandler : IRequestHandler<GetDevicesQuery, PagedResult<DeviceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDevicesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<DeviceDto>> Handle(GetDevicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Devices.AsNoTracking();
        if (request.TenantId.HasValue) query = query.Where(d => d.TenantId == request.TenantId.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(d => d.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize).ToListAsync(cancellationToken);

        return PagedResult<DeviceDto>.Success(_mapper.Map<List<DeviceDto>>(items), totalCount, request.PageNumber, request.PageSize);
    }
}
