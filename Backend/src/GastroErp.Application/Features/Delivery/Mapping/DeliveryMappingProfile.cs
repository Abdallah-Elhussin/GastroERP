using AutoMapper;
using GastroErp.Application.Features.Delivery.DTOs;
using GastroErp.Domain.Entities.Delivery;

namespace GastroErp.Application.Features.Delivery.Mapping;

public class DeliveryMappingProfile : Profile
{
    public DeliveryMappingProfile()
    {
        CreateMap<DeliveryZone, DeliveryZoneDto>();
        CreateMap<DeliveryDriver, DeliveryDriverDto>();

        CreateMap<DeliveryOrder, DeliveryOrderDto>();
        CreateMap<DeliveryOrder, DeliveryOrderDetailDto>()
            .ForMember(d => d.TrackingEvents, o => o.MapFrom(s => s.TrackingEvents));

        CreateMap<DeliveryTrackingEvent, DeliveryTrackingEventDto>();
    }
}
