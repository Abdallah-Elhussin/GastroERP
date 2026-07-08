using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace GastroErp.Application.Features.Organization.Queries;

public class GetSubscriptionQueryHandler : IRequestHandler<GetSubscriptionQuery, Result<SubscriptionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSubscriptionQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<SubscriptionDto>> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == request.TenantId, cancellationToken);

        if (subscription == null)
        {
            return Result<SubscriptionDto>.Failure("NotFound", "Subscription not found.");
        }

        return Result<SubscriptionDto>.Success(_mapper.Map<SubscriptionDto>(subscription));
    }
}

public class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, Result<IEnumerable<SubscriptionPlanDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPlansQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<SubscriptionPlanDto>>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _context.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<SubscriptionPlanDto>>.Success(_mapper.Map<IEnumerable<SubscriptionPlanDto>>(plans));
    }
}
