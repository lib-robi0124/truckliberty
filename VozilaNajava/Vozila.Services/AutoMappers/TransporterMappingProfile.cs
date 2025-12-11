using AutoMapper;
using Vozila.Domain.Models;
using Vozila.ViewModels.Models;

namespace Vozila.Services.AutoMappers
{
    public class TransporterMappingProfile : Profile
    {
        public TransporterMappingProfile() 
        {
            CreateMap<Transporter, TransporterVM>();

            CreateMap<Transporter, TransporterListVM>()
                .ForMember(dest => dest.DestinationCount, opt => opt.MapFrom(src => src.Destinations.Count))
                .ForMember(dest => dest.ActiveOrderCount, opt => opt.MapFrom(src =>
                    src.Orders.Count(o => o.Status != Domain.Enums.OrderStatus.Cancelled &&
                                          o.Status != Domain.Enums.OrderStatus.Finished)));

            CreateMap<Transporter, TransporterStatsVM>()
                .ForMember(dest => dest.TotalDestinations, opt => opt.MapFrom(src => src.Destinations.Count))
                .ForMember(dest => dest.ActiveContracts, opt => opt.MapFrom(src =>
                    src.Contracts.Count(c => c.ValidUntil > DateTime.Now)))
                .ForMember(dest => dest.PendingOrders, opt => opt.MapFrom(src =>
                    src.Orders.Count(o => o.Status == Domain.Enums.OrderStatus.Pending)))
                .ForMember(dest => dest.ApprovedOrders, opt => opt.MapFrom(src =>
                    src.Orders.Count(o => o.Status == Domain.Enums.OrderStatus.Approved)))
                .ForMember(dest => dest.FinishedOrders, opt => opt.MapFrom(src =>
                    src.Orders.Count(o => o.Status == Domain.Enums.OrderStatus.Finished)));
        }
    }
}
