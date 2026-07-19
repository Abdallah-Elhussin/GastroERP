using AutoMapper;
using GastroErp.Application.Features.Identity.DTOs;
using GastroErp.Domain.Entities.Identity;

namespace GastroErp.Application.Features.Identity;

public class IdentityMappingProfile : Profile
{
    public IdentityMappingProfile()
    {
        CreateMap<AppUser, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.IsLocked))
            .ForMember(dest => dest.Number, opt => opt.Ignore())
            .ForMember(dest => dest.BranchId, opt => opt.Ignore())
            .ForMember(dest => dest.BranchNameAr, opt => opt.Ignore())
            .ForMember(dest => dest.RoleId, opt => opt.Ignore())
            .ForMember(dest => dest.RoleName, opt => opt.Ignore())
            .ForMember(dest => dest.RoleNameAr, opt => opt.Ignore());

        CreateMap<Role, RoleDto>();
    }
}
