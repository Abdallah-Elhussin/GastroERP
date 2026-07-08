using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Crm.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Crm.Queries;

public record GetCustomerQuery(Guid Id) : IRequest<CustomerDto>;
public record GetCustomersQuery(int PageNumber = 1, int PageSize = 10, string? SearchTerm = null) : IRequest<PagedResult<CustomerDto>>;
public record GetLoyaltyAccountQuery(Guid CustomerId) : IRequest<LoyaltyAccountDto>;
public record GetLoyaltyTransactionsQuery(Guid AccountId, int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<LoyaltyTransactionDto>>;

public record GetMembershipTiersQuery : IRequest<List<MembershipTierDto>>;
public record GetCouponsQuery(int PageNumber = 1, int PageSize = 10, bool? IsActive = null) : IRequest<PagedResult<CouponDto>>;
public record GetPromotionsQuery(int PageNumber = 1, int PageSize = 10, bool? IsActive = null) : IRequest<PagedResult<PromotionCampaignDto>>;
public record GetGiftCardsQuery(int PageNumber = 1, int PageSize = 10, string? CardNumber = null) : IRequest<PagedResult<GiftCardDto>>;
