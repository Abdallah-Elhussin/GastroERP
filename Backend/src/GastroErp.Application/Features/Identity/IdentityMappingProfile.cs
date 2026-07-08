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
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.IsLocked));
            
        CreateMap<Role, RoleDto>();
    }
}
