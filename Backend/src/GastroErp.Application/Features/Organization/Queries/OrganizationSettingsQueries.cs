using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;
using System;

namespace GastroErp.Application.Features.Organization.Queries;

public record GetOrganizationSettingsQuery(Guid TenantId) : IRequest<Result<OrganizationSettingsDto>>;
