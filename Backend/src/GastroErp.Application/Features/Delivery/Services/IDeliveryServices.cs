using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Delivery.DTOs;
using GastroErp.Domain.Entities.Delivery;
using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Delivery.Services;

public interface IDeliveryNumberGenerator
{
    Task<string> GenerateAsync(Guid tenantId, Guid branchId, CancellationToken ct = default);
}

public interface IDeliveryFeeCalculationService
{
    Task<Result<DeliveryFeeQuoteDto>> CalculateFeeAsync(
        Guid tenantId, Guid branchId, Guid? zoneId, decimal? latitude, decimal? longitude,
        CancellationToken ct = default);
}

public interface IDeliveryAssignmentService
{
    Task<Result> AssignDriverAsync(DeliveryOrder delivery, DeliveryDriver driver, CancellationToken ct = default);
}

public interface IDeliveryEtaService
{
    int EstimateMinutes(DeliveryOrder delivery, DeliveryZone? zone);
}

public interface IDeliveryOrderSyncService
{
    Task SyncOnAssignedAsync(Guid salesOrderId, Guid userId, Guid deviceId, CancellationToken ct = default);
    Task SyncOnPickupAsync(Guid salesOrderId, Guid userId, Guid deviceId, CancellationToken ct = default);
    Task SyncOnDeliveredAsync(Guid salesOrderId, Guid userId, Guid deviceId, CancellationToken ct = default);
}

public interface IDeliveryKitchenIntegrationService
{
    Task CheckAndMarkReadyForPickupAsync(Guid salesOrderId, CancellationToken ct = default);
}

/// <summary>نقطة توسعة لمزودي توصيل خارجيين (Jahez, HungerStation, ...)</summary>
public interface IDeliveryProviderAdapter
{
    DeliveryProviderType ProviderType { get; }
    Task<Result<string>> PushOrderAsync(DeliveryOrder delivery, CancellationToken ct = default);
    Task<Result> SyncStatusAsync(DeliveryOrder delivery, CancellationToken ct = default);
}

public interface IDeliveryOrderFactory
{
    Task<Result<DeliveryOrder>> CreateForSalesOrderAsync(
        Guid tenantId, Guid companyId, Guid branchId, Guid salesOrderId,
        CreateOrderDeliveryDto deliveryDto, CancellationToken ct = default);
}
