using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Crm;
using GastroErp.Domain.Common.Exceptions;
using MediatR;
using AutoMapper;

namespace GastroErp.Application.Features.Crm.Commands;

public class CrmCommandHandlers :
    IRequestHandler<CreateCustomerCommand, Guid>,
    IRequestHandler<UpdateCustomerCommand, Unit>,
    IRequestHandler<ChangeCustomerStatusCommand, Unit>,
    IRequestHandler<CreateMembershipTierCommand, Guid>,
    IRequestHandler<CreateCouponCommand, Guid>,
    IRequestHandler<RedeemCouponCommand, Unit>,
    IRequestHandler<CreatePromotionCampaignCommand, Guid>,
    IRequestHandler<ActivatePromotionCommand, Unit>,
    IRequestHandler<DeactivatePromotionCommand, Unit>,
    IRequestHandler<IssueGiftCardCommand, Guid>,
    IRequestHandler<RechargeGiftCardCommand, Unit>,
    IRequestHandler<RedeemGiftCardCommand, Unit>,
    IRequestHandler<EarnLoyaltyPointsCommand, Unit>,
    IRequestHandler<RedeemLoyaltyPointsCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenantService;
    private readonly IMapper _mapper;

    public CrmCommandHandlers(IApplicationDbContext context, ITenantProvider tenantService, IMapper mapper)
    {
        _context = context;
        _tenantService = tenantService;
        _mapper = mapper;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.TenantId ?? throw new UnauthorizedAccessException();
        
        var customerNumber = "CUST-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString().Substring(5);
        
        var customer = new Customer(
            tenantId, 
            customerNumber,
            request.Dto.FullName, 
            request.Dto.Mobile, 
            request.Dto.Email);
            
        customer.UpdateInfo(request.Dto.FullName, request.Dto.Mobile, request.Dto.Email, request.Dto.DateOfBirth, request.Dto.Gender, request.Dto.PreferredLanguage, request.Dto.Notes);
        customer.UpdateCommercialTerms(
            request.Dto.TaxNumber,
            request.Dto.ArAccountId,
            request.Dto.Currency,
            request.Dto.PaymentDueDays,
            request.Dto.PaymentTerms,
            request.Dto.CreditLimit);

        _context.Customers.Add(customer);
        
        var loyaltyAccount = new LoyaltyAccount(tenantId, customer.Id);
        _context.LoyaltyAccounts.Add(loyaltyAccount);
        
        await _context.SaveChangesAsync(cancellationToken);
        return customer.Id;
    }

    public async Task<Unit> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"{nameof(Customer)} not found with ID {request.Id}");

        customer.UpdateInfo(request.Dto.FullName, request.Dto.Mobile, request.Dto.Email, request.Dto.DateOfBirth, request.Dto.Gender, request.Dto.PreferredLanguage, request.Dto.Notes);
        customer.UpdateCommercialTerms(
            request.Dto.TaxNumber,
            request.Dto.ArAccountId,
            request.Dto.Currency,
            request.Dto.PaymentDueDays,
            request.Dto.PaymentTerms,
            request.Dto.CreditLimit);

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    public async Task<Unit> Handle(ChangeCustomerStatusCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"{nameof(Customer)} not found with ID {request.Id}");

        customer.ChangeStatus(request.Status);
        
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    public async Task<Guid> Handle(CreateMembershipTierCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.TenantId ?? throw new UnauthorizedAccessException();
        
        var tier = new MembershipTier(tenantId, request.Dto.Name, request.Dto.TierLevel, request.Dto.RequiredPoints, request.Dto.DiscountPercentage, request.Dto.Priority, request.Dto.Benefits);
        
        _context.MembershipTiers.Add(tier);
        await _context.SaveChangesAsync(cancellationToken);
        return tier.Id;
    }

    public async Task<Guid> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.TenantId ?? throw new UnauthorizedAccessException();
        
        var coupon = new Coupon(tenantId, request.Dto.Code, request.Dto.Type, request.Dto.Value, request.Dto.ValidFrom, request.Dto.ValidTo, request.Dto.UsageLimit, request.Dto.MinimumOrderAmount, request.Dto.RestrictedToCustomerId);
        
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);
        return coupon.Id;
    }

    public async Task<Unit> Handle(RedeemCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _context.Coupons.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"{nameof(Coupon)} not found with ID {request.Id}");

        coupon.Redeem();
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    public async Task<Guid> Handle(CreatePromotionCampaignCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.TenantId ?? throw new UnauthorizedAccessException();
        
        var promo = new PromotionCampaign(tenantId, request.Dto.Name, request.Dto.Type, request.Dto.Value, request.Dto.StartDate, request.Dto.EndDate, request.Dto.Priority, request.Dto.Stackable);
        promo.UpdateConfiguration(request.Dto.ConfigurationJson ?? "");
        
        _context.PromotionCampaigns.Add(promo);
        await _context.SaveChangesAsync(cancellationToken);
        return promo.Id;
    }

    public async Task<Unit> Handle(ActivatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promo = await _context.PromotionCampaigns.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"{nameof(PromotionCampaign)} not found with ID {request.Id}");

        promo.Activate();
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    public async Task<Unit> Handle(DeactivatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promo = await _context.PromotionCampaigns.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"{nameof(PromotionCampaign)} not found with ID {request.Id}");

        promo.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    public async Task<Guid> Handle(IssueGiftCardCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantService.TenantId ?? throw new UnauthorizedAccessException();
        
        var card = new GiftCard(tenantId, request.Dto.CardNumber, request.Dto.InitialValue, request.Dto.ExpiryDate, request.Dto.CustomerId);
        
        _context.GiftCards.Add(card);
        await _context.SaveChangesAsync(cancellationToken);
        return card.Id;
    }

    public async Task<Unit> Handle(RechargeGiftCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _context.GiftCards.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"{nameof(GiftCard)} not found with ID {request.Id}");

        card.Recharge(request.Dto.Amount);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    public async Task<Unit> Handle(RedeemGiftCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _context.GiftCards.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new KeyNotFoundException($"{nameof(GiftCard)} not found with ID {request.Id}");

        card.Redeem(request.Amount, request.OrderId);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    public async Task<Unit> Handle(EarnLoyaltyPointsCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.LoyaltyAccounts.FindAsync(new object[] { request.AccountId }, cancellationToken)
            ?? throw new KeyNotFoundException($"{nameof(LoyaltyAccount)} not found with ID {request.AccountId}");

        account.EarnPoints(request.Points, request.Reason, request.OrderId);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    public async Task<Unit> Handle(RedeemLoyaltyPointsCommand request, CancellationToken cancellationToken)
    {
        var account = await _context.LoyaltyAccounts.FindAsync(new object[] { request.AccountId }, cancellationToken)
            ?? throw new KeyNotFoundException($"{nameof(LoyaltyAccount)} not found with ID {request.AccountId}");

        account.RedeemPoints(request.Points, request.Reason, request.OrderId);
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
