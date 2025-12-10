using AutoMapper;
using Vozila.Domain.Models;
using Vozila.ViewModels.Models;

namespace Vozila.Services.AutoMappers
{
    public class PriceOilMappingProfile : Profile
    {
        public PriceOilMappingProfile()
        {
            CreateMap<PriceOil, PriceOilVM>();

            CreateMap<PriceOil, PriceOilHistoryVM>()
                .ForMember(dest => dest.PriceChange, opt => opt.Ignore())
                .ForMember(dest => dest.PercentageChange, opt => opt.Ignore());
        }
    }
}
