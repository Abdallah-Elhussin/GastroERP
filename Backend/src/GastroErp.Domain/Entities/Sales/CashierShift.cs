using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Sales;

namespace GastroErp.Domain.Entities.Sales;

/// <summary>CashierShift — وردية الكاشير (Aggregate Root)</summary>
public sealed class CashierShift : AuditableBaseEntity, ITenantEntity, IBranchEntity
{
    private static readonly HashSet<ShiftStatus> AcceptsPayments =
        [ShiftStatus.Open, ShiftStatus.Active];

    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid CashRegisterId { get; private set; }
    public Guid DeviceId { get; private set; }
    public Guid CashierId { get; private set; }
    public string ShiftNumber { get; private set; }
    public ShiftStatus Status { get; private set; }
    public decimal OpeningFloat { get; private set; }
    public decimal ExpectedCash { get; private set; }
    public decimal ActualCash { get; private set; }
    public decimal Variance { get; private set; }
    public ReconciliationStatus ReconciliationStatus { get; private set; }
    public DateTimeOffset OpenedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public DateTimeOffset? ReconciledAt { get; private set; }
    public Guid? ReconciledBy { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<CashMovement> _cashMovements = [];
    public IReadOnlyCollection<CashMovement> CashMovements => _cashMovements.AsReadOnly();

    private CashierShift() { ShiftNumber = string.Empty; }

    public static CashierShift Open(
        Guid tenantId, Guid branchId, Guid cashRegisterId, Guid deviceId,
        Guid cashierId, string shiftNumber, decimal openingFloat, string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (openingFloat < 0) throw new BusinessException(ErrorCodes.InvalidPaymentAmount);

        var shift = new CashierShift
        {
            TenantId = tenantId,
            BranchId = branchId,
            CashRegisterId = cashRegisterId,
            DeviceId = deviceId,
            CashierId = cashierId,
            ShiftNumber = shiftNumber,
            OpeningFloat = openingFloat,
            ExpectedCash = openingFloat,
            Status = ShiftStatus.Open,
            ReconciliationStatus = ReconciliationStatus.Pending,
            OpenedAt = DateTimeOffset.UtcNow,
            Notes = notes
        };

        shift._cashMovements.Add(new CashMovement(
            shift.Id, cashRegisterId, branchId, tenantId,
            CashMovementType.Float, openingFloat, "Opening float", cashierId));

        shift.RaiseDomainEvent(new ShiftOpenedEvent(shift.Id, cashierId, deviceId, cashRegisterId));
        return shift;
    }

    public void Activate()
    {
        if (Status != ShiftStatus.Open) throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = ShiftStatus.Active;
    }

    public void Suspend()
    {
        if (Status != ShiftStatus.Active) throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = ShiftStatus.Suspended;
    }

    public void Resume()
    {
        if (Status != ShiftStatus.Suspended) throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = ShiftStatus.Active;
    }

    public void StartClosing()
    {
        if (Status != ShiftStatus.Active && Status != ShiftStatus.Suspended)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = ShiftStatus.Closing;
    }

    public void Close(decimal actualCash, Guid closedBy)
    {
        if (Status != ShiftStatus.Closing && Status != ShiftStatus.Active)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);

        ActualCash = actualCash;
        Variance = actualCash - ExpectedCash;
        Status = ShiftStatus.Closed;
        ClosedAt = DateTimeOffset.UtcNow;
        ReconciliationStatus = Math.Abs(Variance) > 0
            ? ReconciliationStatus.VarianceDetected
            : ReconciliationStatus.Balanced;

        if (Math.Abs(Variance) > 0)
        {
            _cashMovements.Add(new CashMovement(
                Id, CashRegisterId, BranchId, TenantId,
                CashMovementType.Variance, Math.Abs(Variance),
                $"Variance: {Variance:F2}", closedBy));
        }

        RaiseDomainEvent(new ShiftClosedEvent(Id, actualCash, Variance));
    }

    public void Reconcile(Guid reconciledBy, decimal varianceThreshold = 100)
    {
        if (Status != ShiftStatus.Closed) throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        if (Math.Abs(Variance) > varianceThreshold)
            throw new BusinessException(ErrorCodes.ManagerApprovalRequired);

        Status = ShiftStatus.Reconciled;
        ReconciledAt = DateTimeOffset.UtcNow;
        ReconciledBy = reconciledBy;
        ReconciliationStatus = ReconciliationStatus.Approved;

        RaiseDomainEvent(new ReconciliationCompletedEvent(Id, reconciledBy, Variance));
    }

    public CashMovement RecordMovement(
        CashMovementType type, decimal amount, string reason, Guid userId, string? referenceDocument = null)
    {
        EnsureAcceptsTransactions();

        var isInflow = type is CashMovementType.CashIn or CashMovementType.SafeWithdrawal
            or CashMovementType.Float or CashMovementType.Sale;

        ExpectedCash = isInflow ? ExpectedCash + amount : ExpectedCash - amount;

        var movement = new CashMovement(Id, CashRegisterId, BranchId, TenantId, type, amount, reason, userId, referenceDocument);
        _cashMovements.Add(movement);
        RaiseDomainEvent(new CashMovementCreatedEvent(movement.Id, Id, type, amount));
        return movement;
    }

    public void EnsureAcceptsPayments()
    {
        if (!AcceptsPayments.Contains(Status))
            throw new BusinessException(ErrorCodes.ShiftClosed);
    }

    private void EnsureAcceptsTransactions()
    {
        if (Status is ShiftStatus.Closed or ShiftStatus.Reconciled or ShiftStatus.Closing)
            throw new BusinessException(ErrorCodes.ShiftClosed);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class CashMovement : BaseEntity
{
    public Guid CashierShiftId { get; private set; }
    public Guid CashRegisterId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid TenantId { get; private set; }
    public CashMovementType MovementType { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; }
    public string? ReferenceDocument { get; private set; }
    public Guid CreatedByUser { get; private set; }
    public DateTimeOffset CreatedAtMovement { get; private set; }

    private CashMovement() { Reason = string.Empty; }

    internal CashMovement(Guid shiftId, Guid registerId, Guid branchId, Guid tenantId,
        CashMovementType type, decimal amount, string reason, Guid createdBy, string? referenceDocument = null)
    {
        if (amount <= 0) throw new BusinessException(ErrorCodes.InvalidPaymentAmount);
        if (string.IsNullOrWhiteSpace(reason)) throw new BusinessException(ErrorCodes.RequiredField);

        CashierShiftId = shiftId;
        CashRegisterId = registerId;
        BranchId = branchId;
        TenantId = tenantId;
        MovementType = type;
        Amount = amount;
        Reason = reason;
        CreatedByUser = createdBy;
        ReferenceDocument = referenceDocument;
        CreatedAtMovement = DateTimeOffset.UtcNow;
    }
}
