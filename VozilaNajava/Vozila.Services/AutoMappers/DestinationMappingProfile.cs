using AutoMapper;
using Vozila.Domain.Models;
using Vozila.ViewModels.Models;

namespace Vozila.Services.AutoMappers
{
    public class DestinationMappingProfile : Profile
    {
        public DestinationMappingProfile() 
        {
            CreateMap<Destination, DestinationVM>()
              .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.ToString()))
              .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country.ToString()))
              .ForMember(dest => dest.ContractOilPrice, opt => opt.MapFrom(src => src.Contract.ContractOilPrice))
              .ForMember(dest => dest.CalculatedPrice, opt => opt.MapFrom(src =>
                  CalculateDestinationPrice(src.DestinationContractPrice, src.DailyPricePerLiter, src.Contract.ContractOilPrice)));

            CreateMap<Destination, DestinationListVM>()
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.ToString()))
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country.ToString()))
                .ForMember(dest => dest.CalculatedPrice, opt => opt.MapFrom(src =>
                    CalculateDestinationPrice(src.DestinationContractPrice, src.DailyPricePerLiter, src.Contract.ContractOilPrice)))
                .ForMember(dest => dest.OrderCount, opt => opt.MapFrom(src => src.Orders.Count));

            CreateMap<Destination, DestinationDetailsVM>()
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.City.ToString()))
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country.ToString()))
                .ForMember(dest => dest.ContractOilPrice, opt => opt.MapFrom(src => src.Contract.ContractOilPrice))
                .ForMember(dest => dest.CalculatedPrice, opt => opt.MapFrom(src =>
                    CalculateDestinationPrice(src.DestinationContractPrice, src.DailyPricePerLiter, src.Contract.ContractOilPrice)))
                .ForMember(dest => dest.ContractNumber, opt => opt.MapFrom(src => src.Contract.ContractNumber))
                .ForMember(dest => dest.TransporterName, opt => opt.MapFrom(src => src.Contract.Transporter.CompanyName))
                .ForMember(dest => dest.IsContractActive, opt => opt.MapFrom(src => src.Contract.ValidUntil > DateTime.Now));
        }
        private static decimal CalculateDestinationPrice(decimal contractPrice, decimal dailyPrice, decimal contractOilPrice)
        {
            if (contractOilPrice == 0)
                return contractPrice;

            var priceDifference = dailyPrice - contractOilPrice;
            var adjustmentFactor = priceDifference / contractOilPrice * 0.3m;
            return contractPrice * (1 + adjustmentFactor);
        }
    }
}
