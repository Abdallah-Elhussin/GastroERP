using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Organization.Queries;

// Branch
public record GetBranchByIdQuery(Guid Id) : IRequest<Result<BranchDto>>;
public record GetBranchesQuery(
    Guid? TenantId = null,
    Guid? CompanyId = null,
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null) : IRequest<PagedResult<BranchDto>>;

// Company
public record GetCompanyByIdQuery(Guid Id) : IRequest<Result<CompanyDto>>;
public record GetCompaniesQuery(Guid? TenantId = null, int PageNumber = 1, int PageSize = 10, string? SearchTerm = null) : IRequest<PagedResult<CompanyDto>>;

// Department
public record GetDepartmentByIdQuery(Guid Id) : IRequest<Result<DepartmentDto>>;
public record GetDepartmentsQuery(Guid? TenantId = null, Guid? CompanyId = null, Guid? BranchId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<DepartmentDto>>;

// Device
public record GetDeviceByIdQuery(Guid Id) : IRequest<Result<DeviceDto>>;
public record GetDevicesQuery(Guid? TenantId = null, int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<DeviceDto>>;
