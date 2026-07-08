using GastroErp.Application.Features.Crm.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Crm.Commands;

public record CreateCustomerCommand(CreateCustomerDto Dto) : IRequest<Guid>;
public record UpdateCustomerCommand(Guid Id, UpdateCustomerDto Dto) : IRequest<Unit>;
public record ChangeCustomerStatusCommand(Guid Id, GastroErp.Domain.Enums.CustomerStatus Status) : IRequest<Unit>;

public record CreateMembershipTierCommand(CreateMembershipTierDto Dto) : IRequest<Guid>;
public record CreateCouponCommand(CreateCouponDto Dto) : IRequest<Guid>;
public record RedeemCouponCommand(Guid Id, Guid OrderId) : IRequest<Unit>;

public record CreatePromotionCampaignCommand(CreatePromotionCampaignDto Dto) : IRequest<Guid>;
public record ActivatePromotionCommand(Guid Id) : IRequest<Unit>;
public record DeactivatePromotionCommand(Guid Id) : IRequest<Unit>;

public record IssueGiftCardCommand(IssueGiftCardDto Dto) : IRequest<Guid>;
public record RechargeGiftCardCommand(Guid Id, RechargeGiftCardDto Dto) : IRequest<Unit>;
public record RedeemGiftCardCommand(Guid Id, decimal Amount, Guid OrderId) : IRequest<Unit>;

public record EarnLoyaltyPointsCommand(Guid AccountId, decimal Points, string Reason, Guid? OrderId = null) : IRequest<Unit>;
public record RedeemLoyaltyPointsCommand(Guid AccountId, decimal Points, string Reason, Guid? OrderId = null) : IRequest<Unit>;
