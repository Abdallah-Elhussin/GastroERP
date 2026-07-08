using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Crm.DTOs;
using GastroErp.Domain.Entities.Crm;
using GastroErp.Domain.Common.Exceptions;
using MediatR;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Crm.Queries;

public class CrmQueryHandlers :
    IRequestHandler<GetCustomerQuery, CustomerDto>,
    IRequestHandler<GetCustomersQuery, PagedResult<CustomerDto>>,
    IRequestHandler<GetLoyaltyAccountQuery, LoyaltyAccountDto>,
    IRequestHandler<GetLoyaltyTransactionsQuery, PagedResult<LoyaltyTransactionDto>>,
    IRequestHandler<GetMembershipTiersQuery, List<MembershipTierDto>>,
    IRequestHandler<GetCouponsQuery, PagedResult<CouponDto>>,
    IRequestHandler<GetPromotionsQuery, PagedResult<PromotionCampaignDto>>,
    IRequestHandler<GetGiftCardsQuery, PagedResult<GiftCardDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CrmQueryHandlers(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CustomerDto> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"{nameof(Customer)} not found with ID {request.Id}");

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<PagedResult<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(c => c.FullName.Contains(request.SearchTerm) || c.Mobile.Contains(request.SearchTerm) || c.CustomerNumber.Contains(request.SearchTerm));
        }

        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(c => c.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<CustomerDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return PagedResult<CustomerDto>.Success(items, request.PageNumber, request.PageSize, count);
    }

    public async Task<LoyaltyAccountDto> Handle(GetLoyaltyAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await _context.LoyaltyAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.CustomerId == request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"{nameof(LoyaltyAccount)} not found for Customer ID {request.CustomerId}");

        return _mapper.Map<LoyaltyAccountDto>(account);
    }

    public async Task<PagedResult<LoyaltyTransactionDto>> Handle(GetLoyaltyTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LoyaltyTransactions
            .AsNoTracking()
            .Where(t => t.LoyaltyAccountId == request.AccountId);

        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(t => t.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<LoyaltyTransactionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return PagedResult<LoyaltyTransactionDto>.Success(items, request.PageNumber, request.PageSize, count);
    }

    public async Task<List<MembershipTierDto>> Handle(GetMembershipTiersQuery request, CancellationToken cancellationToken)
    {
        return await _context.MembershipTiers
            .AsNoTracking()
            .OrderBy(m => m.Priority)
            .ProjectTo<MembershipTierDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<CouponDto>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Coupons.AsNoTracking();

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<CouponDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return PagedResult<CouponDto>.Success(items, request.PageNumber, request.PageSize, count);
    }

    public async Task<PagedResult<PromotionCampaignDto>> Handle(GetPromotionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PromotionCampaigns.AsNoTracking();

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(p => p.Priority)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<PromotionCampaignDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return PagedResult<PromotionCampaignDto>.Success(items, request.PageNumber, request.PageSize, count);
    }

    public async Task<PagedResult<GiftCardDto>> Handle(GetGiftCardsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.GiftCards.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.CardNumber))
        {
            query = query.Where(g => g.CardNumber == request.CardNumber);
        }

        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(g => g.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<GiftCardDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return PagedResult<GiftCardDto>.Success(items, request.PageNumber, request.PageSize, count);
    }
}
