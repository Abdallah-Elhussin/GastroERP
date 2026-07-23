namespace GastroErp.Application.Features.Inventory.DTOs;

/// <summary>
/// Purchasing module dashboard — KPIs, alerts, and recent documents in one round-trip.
/// </summary>
public record PurchasingDashboardDto(
    int OpenPurchaseOrders,
    int LatePurchaseOrders,
    int TotalPurchaseOrders,
    int UninvoicedReceipts,
    int PendingReceipts,
    int TotalReceipts,
    int UnpaidInvoices,
    int DraftInvoices,
    int TotalInvoices,
    int TotalSuppliers,
    int ActiveSuppliers,
    int OverCreditSuppliers,
    IReadOnlyList<PurchasingDashboardAlertDto> Alerts,
    IReadOnlyList<PurchasingDashboardActivityDto> RecentActivities
);

public record PurchasingDashboardAlertDto(
    string Code,
    string Severity,
    string MessageEn,
    string MessageAr,
    string? Path
);

public record PurchasingDashboardActivityDto(
    Guid Id,
    string Kind,
    string Reference,
    DateTimeOffset OccurredAt,
    string? Notes,
    string? Path
);
