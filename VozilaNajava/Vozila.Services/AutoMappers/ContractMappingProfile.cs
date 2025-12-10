using AutoMapper;
using Vozila.ViewModels.Models;
using Vozila.Domain.Models;

namespace Vozila.Services.AutoMappers
{
    public class ContractMappingProfile : Profile
    {
        public ContractMappingProfile() 
        {
            CreateMap<Contract, ContractVM>()
                            .ForMember(dest => dest.TransporterName, opt => opt.MapFrom(src => src.Transporter.CompanyName))
                            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.ValidUntil > DateTime.Now))
                            .ForMember(dest => dest.DaysUntilExpiry, opt => opt.MapFrom(src =>
                                (src.ValidUntil - DateTime.Now).Days));

            CreateMap<Contract, ContractListVM>()
                .ForMember(dest => dest.TransporterName, opt => opt.MapFrom(src => src.Transporter.CompanyName))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.ValidUntil > DateTime.Now))
                .ForMember(dest => dest.ContractNumber, opt => opt.MapFrom(src => src.Contract.ContractNumber))
                .ForMember(dest => dest.DestinationCount, opt => opt.MapFrom(src => src.Destinations.Count));

            CreateMap<Contract, ContractDetailsVM>()
                .ForMember(dest => dest.TransporterName, opt => opt.MapFrom(src => src.Transporter.CompanyName))
                .ForMember(dest => dest.TransporterEmail, opt => opt.MapFrom(src => src.Transporter.Email))
                .ForMember(dest => dest.ContractNumber, opt => opt.MapFrom(src => src.Contract.ContractNumber))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.ValidUntil > DateTime.Now))
                .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Destinations));
        }
    }
}
