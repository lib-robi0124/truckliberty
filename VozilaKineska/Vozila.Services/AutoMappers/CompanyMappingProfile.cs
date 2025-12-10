using AutoMapper;
using Vozila.Domain.Models;
using Vozila.ViewModels.ModelsCompany;

namespace Vozila.Services.AutoMappers
{
    public class CompanyMappingProfile : Profile
    {
        public CompanyMappingProfile() 
        {
            CreateMap<Company, CompanyVM>()
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country.ToString()))
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.ToString()))
                .ForMember(dest => dest.OrderCount, opt => opt.MapFrom(src => src.Orders.Count));
        }
    }
}
