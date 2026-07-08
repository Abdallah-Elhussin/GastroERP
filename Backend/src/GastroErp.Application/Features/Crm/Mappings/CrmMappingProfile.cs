using AutoMapper;
using GastroErp.Application.Features.Crm.DTOs;
using GastroErp.Domain.Entities.Crm;

namespace GastroErp.Application.Features.Crm.Mappings;

public class CrmMappingProfile : Profile
{
    public CrmMappingProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.LoyaltyAccountId, opt => opt.MapFrom(src => src.LoyaltyAccount != null ? (Guid?)src.LoyaltyAccount.Id : null));
        
        CreateMap<LoyaltyAccount, LoyaltyAccountDto>();
        CreateMap<LoyaltyTransaction, LoyaltyTransactionDto>();
        CreateMap<MembershipTier, MembershipTierDto>();
        CreateMap<Coupon, CouponDto>();
        CreateMap<PromotionCampaign, PromotionCampaignDto>();
        CreateMap<GiftCard, GiftCardDto>();
    }
}
