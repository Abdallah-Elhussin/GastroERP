using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;
using System;

namespace GastroErp.Application.Features.Organization.Commands;

public record CreateSubscriptionCommand(CreateSubscriptionDto Dto) : IRequest<Result<SubscriptionDto>>;
public record RenewSubscriptionCommand(Guid Id, DateTimeOffset NewEndDate, decimal Price) : IRequest<Result>;
public record SuspendSubscriptionCommand(Guid Id) : IRequest<Result>;
public record ResumeSubscriptionCommand(Guid Id) : IRequest<Result>;
public record CancelSubscriptionCommand(Guid Id) : IRequest<Result>;
