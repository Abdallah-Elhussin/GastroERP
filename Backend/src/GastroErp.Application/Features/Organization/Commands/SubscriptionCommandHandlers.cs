using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Domain.Entities.Organization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GastroErp.Application.Features.Organization.Commands;

public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, Result<SubscriptionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSubscriptionCommandHandler> _logger;

    public CreateSubscriptionCommandHandler(IApplicationDbContext context, IMapper mapper, ILogger<CreateSubscriptionCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<SubscriptionDto>> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == request.Dto.PlanId, cancellationToken);
        if (plan == null) return Result<SubscriptionDto>.Failure("PlanNotFound", "Subscription plan not found.");

        BillingCycle cycle = Enum.Parse<BillingCycle>(request.Dto.BillingCycle);
        DateTimeOffset startDate = DateTimeOffset.UtcNow;
        DateTimeOffset endDate = cycle == BillingCycle.Monthly ? startDate.AddMonths(1) : startDate.AddYears(1);

        var subscription = new Subscription(
            request.Dto.TenantId,
            request.Dto.PlanId,
            cycle,
            startDate,
            endDate,
            request.Dto.MaxBranches,
            request.Dto.MaxUsers,
            request.Dto.MaxDevices,
            new Money(request.Dto.Price, plan.MonthlyPrice.Currency), // Default to plan's currency
            request.Dto.Notes
        );

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Subscription created: {SubscriptionId}", subscription.Id);

        return Result<SubscriptionDto>.Success(_mapper.Map<SubscriptionDto>(subscription));
    }
}

public class RenewSubscriptionCommandHandler : IRequestHandler<RenewSubscriptionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RenewSubscriptionCommandHandler> _logger;

    public RenewSubscriptionCommandHandler(IApplicationDbContext context, ILogger<RenewSubscriptionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(RenewSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (subscription == null) return Result.Failure("SubscriptionNotFound", "Subscription not found.");

        subscription.Renew(request.NewEndDate, new Money(request.Price, subscription.Price.Currency));

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Subscription renewed: {SubscriptionId}", subscription.Id);

        return Result.Success();
    }
}

public class SuspendSubscriptionCommandHandler : IRequestHandler<SuspendSubscriptionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SuspendSubscriptionCommandHandler> _logger;

    public SuspendSubscriptionCommandHandler(IApplicationDbContext context, ILogger<SuspendSubscriptionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(SuspendSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (subscription == null) return Result.Failure("SubscriptionNotFound", "Subscription not found.");

        subscription.Suspend();

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Subscription suspended: {SubscriptionId}", subscription.Id);

        return Result.Success();
    }
}

public class ResumeSubscriptionCommandHandler : IRequestHandler<ResumeSubscriptionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ResumeSubscriptionCommandHandler> _logger;

    public ResumeSubscriptionCommandHandler(IApplicationDbContext context, ILogger<ResumeSubscriptionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(ResumeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (subscription == null) return Result.Failure("SubscriptionNotFound", "Subscription not found.");

        subscription.Resume();

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Subscription resumed: {SubscriptionId}", subscription.Id);

        return Result.Success();
    }
}

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CancelSubscriptionCommandHandler> _logger;

    public CancelSubscriptionCommandHandler(IApplicationDbContext context, ILogger<CancelSubscriptionCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (subscription == null) return Result.Failure("SubscriptionNotFound", "Subscription not found.");

        subscription.Cancel();

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Subscription cancelled: {SubscriptionId}", subscription.Id);

        return Result.Success();
    }
}
