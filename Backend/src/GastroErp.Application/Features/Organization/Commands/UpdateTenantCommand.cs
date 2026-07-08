using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Organization.Commands;

public record UpdateTenantCommand(Guid Id, UpdateTenantDto Dto) : IRequest<Result>;
