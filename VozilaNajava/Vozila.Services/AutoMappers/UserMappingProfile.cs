using AutoMapper;
using Vozila.Domain.Models;
using Vozila.ViewModels.Models;

namespace Vozila.Services.AutoMappers
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserVM>()
               .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name))
               .ForMember(dest => dest.TransporterName, opt => opt.MapFrom(src => src.Transporter != null ? src.Transporter.CompanyName : null));

            CreateMap<User, UserListVM>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));
        }
    }
}
