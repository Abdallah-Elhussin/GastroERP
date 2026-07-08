using AutoMapper;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Domain.Entities.Organization;

namespace GastroErp.Application.Features.Organization.Mapping;

public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        // Tenant
        CreateMap<Tenant, TenantDto>()
            .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.Branding.LogoUrl))
            .ForMember(dest => dest.PrimaryColor, opt => opt.MapFrom(src => src.Branding.PrimaryColor))
            .ForMember(dest => dest.SecondaryColor, opt => opt.MapFrom(src => src.Branding.SecondaryColor));

        // Branch
        CreateMap<Branch, BranchDto>()
            .ForMember(dest => dest.AddressStreetAr, opt => opt.MapFrom(src => src.Address.StreetAr))
            .ForMember(dest => dest.AddressStreetEn, opt => opt.MapFrom(src => src.Address.StreetEn))
            .ForMember(dest => dest.CityAr, opt => opt.MapFrom(src => src.Address.CityAr))
            .ForMember(dest => dest.CityEn, opt => opt.MapFrom(src => src.Address.CityEn));

        // Company
        CreateMap<Company, CompanyDto>()
            .ForMember(dest => dest.VatNumber, opt => opt.MapFrom(src => src.VatNumber != null ? src.VatNumber.Value : null))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email != null ? src.Email.Value : null))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber != null ? src.PhoneNumber.Value : null));

        // Department
        CreateMap<Department, DepartmentDto>();

        // Device
        CreateMap<Device, DeviceDto>();

        // Organization Settings
        CreateMap<OrganizationSettings, OrganizationSettingsDto>();

        // Subscription
        CreateMap<Subscription, SubscriptionDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.BillingCycle, opt => opt.MapFrom(src => src.BillingCycle.ToString()))
            .ForMember(dest => dest.PriceAmount, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.PriceCurrency, opt => opt.MapFrom(src => src.Price.Currency));

        // Subscription Plan
        CreateMap<SubscriptionPlan, SubscriptionPlanDto>()
            .ForMember(dest => dest.MonthlyPrice, opt => opt.MapFrom(src => src.MonthlyPrice.Amount))
            .ForMember(dest => dest.YearlyPrice, opt => opt.MapFrom(src => src.YearlyPrice.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.MonthlyPrice.Currency));
    }
}
