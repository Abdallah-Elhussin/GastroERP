using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;
using System;

namespace GastroErp.Application.Features.Organization.Commands;

public record UpdateOrganizationSettingsCommand(Guid TenantId, UpdateOrganizationSettingsDto Dto) : IRequest<Result<OrganizationSettingsDto>>;
