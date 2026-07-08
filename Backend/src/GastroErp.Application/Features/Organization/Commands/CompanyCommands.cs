using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Organization.Commands;

public record CreateCompanyCommand(CreateCompanyDto Dto) : IRequest<Result<CompanyDto>>;
public record UpdateCompanyCommand(Guid Id, UpdateCompanyDto Dto) : IRequest<Result>;
public record ActivateCompanyCommand(Guid Id) : IRequest<Result>;
public record DeactivateCompanyCommand(Guid Id) : IRequest<Result>;
