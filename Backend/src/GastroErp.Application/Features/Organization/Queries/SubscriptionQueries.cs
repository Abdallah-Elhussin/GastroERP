using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace GastroErp.Application.Features.Organization.Queries;

public record GetSubscriptionQuery(Guid TenantId) : IRequest<Result<SubscriptionDto>>;
public record GetPlansQuery() : IRequest<Result<IEnumerable<SubscriptionPlanDto>>>;
