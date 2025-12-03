using AutoMapper;
using Vozila.Domain.Models;
using Vozila.ViewModels.Models;

namespace Vozila.Services.AutoMappers
{
    public class ConditionMappingProfile : Profile
    {
        public ConditionMappingProfile() 
        {
            CreateMap<Condition, ConditionVM>()
                .ForMember(dest => dest.ContractNumber, opt => opt.MapFrom(src => src.Contract.ContractNumber))
                .ForMember(dest => dest.DestinationCount, opt => opt.MapFrom(src => src.Destinations.Count));
        }
    }
}
