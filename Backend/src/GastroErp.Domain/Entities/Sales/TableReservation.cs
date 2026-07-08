using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Sales;

namespace GastroErp.Domain.Entities.Sales;

/// <summary>TableReservation — حجز الطاولة (Aggregate Root)</summary>
public sealed class TableReservation : AuditableBaseEntity, ITenantEntity, IBranchEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid? TableId { get; private set; }
    public string CustomerName { get; private set; }
    public string CustomerPhone { get; private set; }
    public int GuestCount { get; private set; }
    public DateTimeOffset ReservationDate { get; private set; }
    public int DurationMinutes { get; private set; }
    public TableReservationStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public Guid? SalesOrderId { get; private set; }

    private TableReservation()
    {
        CustomerName = string.Empty;
        CustomerPhone = string.Empty;
    }

    public static TableReservation Create(
        Guid tenantId, Guid branchId, string customerName, string customerPhone,
        int guestCount, DateTimeOffset reservationDate, int durationMinutes = 120,
        Guid? tableId = null, string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(customerName)) throw new BusinessException(ErrorCodes.NameRequired);
        if (string.IsNullOrWhiteSpace(customerPhone)) throw new BusinessException(ErrorCodes.RequiredField);
        if (guestCount <= 0) throw new ArgumentException("GuestCount must be greater than zero.", nameof(guestCount));

        return new TableReservation
        {
            TenantId = tenantId,
            BranchId = branchId,
            TableId = tableId,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            GuestCount = guestCount,
            ReservationDate = reservationDate,
            DurationMinutes = durationMinutes,
            Notes = notes,
            Status = TableReservationStatus.Pending
        };
    }

    public void Confirm()
    {
        if (Status != TableReservationStatus.Pending)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);

        Status = TableReservationStatus.Confirmed;
        ConfirmedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new TableReservationConfirmedEvent(Id, BranchId));
    }

    public void Seat(Guid salesOrderId)
    {
        if (Status is not (TableReservationStatus.Confirmed or TableReservationStatus.Pending))
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);

        Status = TableReservationStatus.Seated;
        SalesOrderId = salesOrderId;
    }

    public void Complete()
    {
        Status = TableReservationStatus.Completed;
    }

    public void Cancel(string reason)
    {
        if (Status is TableReservationStatus.Completed or TableReservationStatus.Cancelled) return;
        Status = TableReservationStatus.Cancelled;
        RaiseDomainEvent(new TableReservationCancelledEvent(Id, reason));
    }

    public void MarkNoShow()
    {
        if (Status != TableReservationStatus.Confirmed)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = TableReservationStatus.NoShow;
    }

    public void AssignTable(Guid tableId) => TableId = tableId;
}
