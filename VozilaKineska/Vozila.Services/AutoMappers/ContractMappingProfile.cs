using AutoMapper;
using Vozila.Domain.Models;
using Vozila.ViewModels.ModelsContract;

namespace Vozila.Services.AutoMappers
{
    public class ContractMappingProfile : Profile
    {
        public ContractMappingProfile()
        {
            // --- ContractVM ---
            CreateMap<Contract, ContractVM>()
                .ForMember(dest => dest.TransporterName,
                    opt => opt.MapFrom(src => src.Transporter.CompanyName))
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.ValidUntil > DateTime.Now))
                .ForMember(dest => dest.DaysUntilExpiry,
                    opt => opt.MapFrom(src => (src.ValidUntil - DateTime.Now).Days));

            // --- ContractListVM ---
            CreateMap<Contract, ContractListVM>()
                .ForMember(dest => dest.TransporterName,
                    opt => opt.MapFrom(src => src.Transporter.CompanyName))
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.ValidUntil > DateTime.Now))
                .ForMember(dest => dest.ContractNumber,
                    opt => opt.MapFrom(src => src.ContractNumber))   // FIXED
                .ForMember(dest => dest.DestrinationCount,
                    opt => opt.MapFrom(src => src.Destinations.Count)); // FIXED

            // --- ContractDetailsVM ---
            CreateMap<Contract, ContractDetailsVM>()
                .ForMember(dest => dest.TransporterName,
                    opt => opt.MapFrom(src => src.Transporter.CompanyName))
                .ForMember(dest => dest.TransporterEmail,
                    opt => opt.MapFrom(src => src.Transporter.Email))
                .ForMember(dest => dest.ContractNumber,
                    opt => opt.MapFrom(src => src.ContractNumber)) // FIXED
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.ValidUntil > DateTime.Now))
                .ForMember(dest => dest.Destinations,
                    opt => opt.MapFrom(src => src.Destinations)); // AUTO MAP USING DestinationMappingProfile
        }
    }

}
