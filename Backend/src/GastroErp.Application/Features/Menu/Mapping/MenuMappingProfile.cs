using AutoMapper;
using GastroErp.Application.Features.Menu.DTOs;
using GastroErp.Domain.Entities.Menu;

namespace GastroErp.Application.Features.Menu.Mapping;

public class MenuMappingProfile : Profile
{
    public MenuMappingProfile()
    {
        // Category
        CreateMap<Category, CategoryDto>();

        // PriceLevel
        CreateMap<PriceLevel, PriceLevelDto>();

        // Menu
        CreateMap<global::GastroErp.Domain.Entities.Menu.Menu, MenuDto>()
            .ForMember(dest => dest.SectionCount, opt => opt.MapFrom(src => src.Sections.Count));

        // MenuSection
        CreateMap<MenuSection, MenuSectionDto>()
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

        // MenuItem
        CreateMap<MenuItem, MenuItemDto>()
            .ForMember(dest => dest.ProductNameAr, opt => opt.Ignore())
            .ForMember(dest => dest.ProductNameEn, opt => opt.Ignore());

        // BranchMenu
        CreateMap<BranchMenu, BranchMenuDto>()
            .ForMember(dest => dest.AvailabilityCount, opt => opt.MapFrom(src => src.Availabilities.Count));

        // MenuAvailability
        CreateMap<MenuAvailability, MenuAvailabilityDto>();

        // Product
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryNameAr, opt => opt.Ignore())
            .ForMember(dest => dest.CategoryNameEn, opt => opt.Ignore())
            .ForMember(dest => dest.ModifierGroupCount, opt => opt.MapFrom(src => src.ModifierGroups.Count))
            .ForMember(dest => dest.ImageCount, opt => opt.MapFrom(src => src.Images.Count))
            .ForMember(dest => dest.HasModifiers, opt => opt.MapFrom(src => src.HasModifiers));

        // ModifierGroup
        CreateMap<ModifierGroup, ModifierGroupDto>()
            .ForMember(dest => dest.Modifiers, opt => opt.MapFrom(src => src.Modifiers));

        // Modifier
        CreateMap<Modifier, ModifierDto>();

        // OptionGroup
        CreateMap<OptionGroup, OptionGroupDto>()
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

        // Option
        CreateMap<Option, OptionDto>();

        // ComboMeal
        CreateMap<ComboMeal, ComboDto>();

        // ComboItem
        CreateMap<ComboItem, ComboItemDto>()
            .ForMember(dest => dest.ProductNameAr, opt => opt.Ignore())
            .ForMember(dest => dest.ProductNameEn, opt => opt.Ignore());

        // ProductPriceLevel
        CreateMap<ProductPriceLevel, ProductPriceLevelDto>();

        // ProductImage
        CreateMap<ProductImage, ProductImageDto>();
    }
}
