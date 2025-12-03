using AutoMapper;
using Vozila.Domain.Models;
using Vozila.ViewModels.Models;

namespace Vozila.Services.AutoMappers
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile() 
        {
            CreateMap<Order, OrderVM>()
               .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.CustomerName))
               .ForMember(dest => dest.TransporterName, opt => opt.MapFrom(src => src.Transporter.CompanyName))
               .ForMember(dest => dest.DestinationCity, opt => opt.MapFrom(src => src.Destination.City.ToString()))
               .ForMember(dest => dest.DestinationCountry, opt => opt.MapFrom(src => src.Destination.Country.ToString()))
               .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Order, OrderListVM>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.CustomerName))
                .ForMember(dest => dest.TransporterName, opt => opt.MapFrom(src => src.Transporter.CompanyName))
                .ForMember(dest => dest.DestinationCity, opt => opt.MapFrom(src => src.Destination.City.ToString()))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Order, OrderDetailsVM>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.CustomerName))
                .ForMember(dest => dest.TransporterName, opt => opt.MapFrom(src => src.Transporter.CompanyName))
                .ForMember(dest => dest.DestinationCity, opt => opt.MapFrom(src => src.Destination.City.ToString()))
                .ForMember(dest => dest.DestinationCountry, opt => opt.MapFrom(src => src.Destination.Country.ToString()))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CancelledByUserName, opt => opt.MapFrom(src => src.CancelledByUser != null ? src.CancelledByUser.FullName : null))
                .ForMember(dest => dest.DestinationPrice, opt => opt.MapFrom(src =>
                    CalculateDestinationPrice(src.Destination.DestinationContractPrice, src.Destination.DailyPricePerLiter, src.ContractOilPrice)))
                .ForMember(dest => dest.CanSubmitTruck, opt => opt.MapFrom(src =>
                    src.Status == Domain.Enums.OrderStatus.Pending && src.DateForLoadingFrom >= DateTime.Now))
                .ForMember(dest => dest.CanCancel, opt => opt.MapFrom(src =>
                    src.Status == Domain.Enums.OrderStatus.Pending || src.Status == Domain.Enums.OrderStatus.Approved))
                .ForMember(dest => dest.CanFinish, opt => opt.MapFrom(src =>
                    src.Status == Domain.Enums.OrderStatus.Approved));
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
