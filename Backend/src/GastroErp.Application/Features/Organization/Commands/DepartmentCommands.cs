using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Organization.Commands;

public record CreateDepartmentCommand(CreateDepartmentDto Dto) : IRequest<Result<DepartmentDto>>;
public record UpdateDepartmentCommand(Guid Id, UpdateDepartmentDto Dto) : IRequest<Result>;
public record DeactivateDepartmentCommand(Guid Id) : IRequest<Result>;
