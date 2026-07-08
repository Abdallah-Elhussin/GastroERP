using AutoMapper;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Sales.Mapping;

public class SalesMappingProfile : Profile
{
    public SalesMappingProfile()
    {
        CreateMap<SalesOrder, OrderDto>()
            .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Count(i => !i.IsVoided)))
            .ForMember(d => d.RemainingBalance, o => o.MapFrom(s => s.RemainingBalance));

        CreateMap<SalesOrder, OrderSummaryDto>()
            .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Count(i => !i.IsVoided)))
            .ForMember(d => d.RemainingBalance, o => o.MapFrom(s => s.RemainingBalance));

        CreateMap<SalesOrder, OrderDetailDto>()
            .ForMember(d => d.RemainingBalance, o => o.MapFrom(s => s.RemainingBalance))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items))
            .ForMember(d => d.Discounts, o => o.MapFrom(s => s.Discounts))
            .ForMember(d => d.Taxes, o => o.MapFrom(s => s.Taxes))
            .ForMember(d => d.StatusHistory, o => o.MapFrom(s => s.StatusHistory));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.Modifiers, o => o.MapFrom(s => s.Modifiers));

        CreateMap<OrderLineModifier, OrderLineModifierDto>();
        CreateMap<OrderDiscount, OrderDiscountDto>();
        CreateMap<OrderTax, OrderTaxDto>();
        CreateMap<OrderStatusHistory, OrderStatusHistoryDto>();

        // Payment
        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.Allocations, o => o.MapFrom(s => s.Allocations));
        CreateMap<PaymentAllocation, PaymentAllocationDto>();
        CreateMap<Refund, RefundDto>();

        // Cash Register
        CreateMap<CashRegister, CashRegisterDto>();

        // Shift
        CreateMap<CashierShift, CashierShiftDto>();

        // Cash Movement
        CreateMap<CashMovement, CashMovementDto>();

        // Kitchen
        CreateMap<KitchenStation, KitchenStationDto>();
        CreateMap<KitchenTicket, KitchenTicketDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));
        CreateMap<KitchenTicketItem, KitchenTicketItemDto>();

        // Floor Plan
        CreateMap<FloorPlan, FloorPlanDto>()
            .ForMember(d => d.AreaCount, o => o.MapFrom(s => s.DiningAreas.Count));
        CreateMap<FloorPlan, FloorPlanDetailDto>()
            .ForMember(d => d.DiningAreas, o => o.MapFrom(s => s.DiningAreas));
        CreateMap<DiningArea, DiningAreaDto>()
            .ForMember(d => d.Tables, o => o.MapFrom(s => s.Tables));
        CreateMap<RestaurantTable, RestaurantTableDto>();

        // Reservations
        CreateMap<TableReservation, TableReservationDto>();
    }
}
