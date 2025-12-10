using AutoMapper;
using Vozila.Domain.Models;
using Vozila.ViewModels.Models;

namespace Vozila.Services.AutoMappers
{
    public class RoleMappingProfile : Profile
    {
        public RoleMappingProfile()
        {
            CreateMap<Role, RoleVM>()
                .ForMember(dest => dest.UserCount, opt => opt.MapFrom(src => src.Users.Count))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}
