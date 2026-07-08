using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Inventory;
using GastroErp.Domain.Events.Sales;

namespace GastroErp.Domain.Entities.Sales;

/// <summary>Payment — عملية الدفع (Aggregate Root)</summary>
public sealed class Payment : AuditableBaseEntity, ITenantEntity, IBranchEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid CashierShiftId { get; private set; }
    public string ReceiptNumber { get; private set; }
    public PaymentMethodType PaymentMethod { get; private set; }
    public PaymentStatus Status { get; private set; }
    public decimal Amount { get; private set; }
    public decimal TipAmount { get; private set; }
    public string Currency { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public Guid ProcessedBy { get; private set; }
    public DateTimeOffset ProcessedAt { get; private set; }
    public string? VoidReason { get; private set; }

    private readonly List<PaymentAllocation> _allocations = [];
    public IReadOnlyCollection<PaymentAllocation> Allocations => _allocations.AsReadOnly();

    private readonly List<Refund> _refunds = [];
    public IReadOnlyCollection<Refund> Refunds => _refunds.AsReadOnly();

    private Payment()
    {
        ReceiptNumber = string.Empty;
        Currency = "SAR";
    }

    public static Payment Create(
        Guid tenantId, Guid branchId, Guid cashierShiftId, string receiptNumber,
        PaymentMethodType method, decimal amount, string currency, Guid processedBy,
        decimal tipAmount = 0, string? referenceNumber = null, string? gatewayTransactionId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (branchId == Guid.Empty) throw new ArgumentException("BranchId cannot be empty.", nameof(branchId));
        if (cashierShiftId == Guid.Empty) throw new ArgumentException("CashierShiftId cannot be empty.", nameof(cashierShiftId));
        if (amount <= 0) throw new BusinessException(ErrorCodes.InvalidPaymentAmount);
        if (tipAmount < 0) throw new BusinessException(ErrorCodes.InvalidPaymentAmount);

        return new Payment
        {
            TenantId = tenantId,
            BranchId = branchId,
            CashierShiftId = cashierShiftId,
            ReceiptNumber = receiptNumber,
            PaymentMethod = method,
            Amount = amount,
            TipAmount = tipAmount,
            Currency = currency.ToUpperInvariant(),
            ProcessedBy = processedBy,
            ProcessedAt = DateTimeOffset.UtcNow,
            ReferenceNumber = referenceNumber,
            GatewayTransactionId = gatewayTransactionId,
            Status = PaymentStatus.Pending
        };
    }

    public PaymentAllocation AllocateToOrder(Guid salesOrderId, decimal amount)
    {
        if (Status is PaymentStatus.Voided or PaymentStatus.Cancelled)
            throw new BusinessException(ErrorCodes.PaymentAlreadyVoided);
        if (amount <= 0) throw new BusinessException(ErrorCodes.InvalidPaymentAmount);

        var allocated = _allocations.Sum(a => a.AllocatedAmount);
        if (allocated + amount > Amount)
            throw new BusinessException(ErrorCodes.PaymentExceedsBalance);

        var allocation = new PaymentAllocation(Id, salesOrderId, amount, Currency);
        _allocations.Add(allocation);
        return allocation;
    }

    public void Complete(Guid orderId)
    {
        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Authorized)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);

        Status = PaymentStatus.Completed;
        RaiseDomainEvent(new PaymentCompletedEvent(Id, orderId, Amount, Currency, CashierShiftId));
    }

    public void MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        RaiseDomainEvent(new PaymentFailedEvent(Id, reason));
    }

    public void Void(string reason, Guid voidedBy)
    {
        if (Status == PaymentStatus.Voided) throw new BusinessException(ErrorCodes.PaymentAlreadyVoided);
        if (string.IsNullOrWhiteSpace(reason)) throw new BusinessException(ErrorCodes.VoidReasonRequired);

        Status = PaymentStatus.Voided;
        VoidReason = reason;
        RaiseDomainEvent(new PaymentVoidedEvent(Id, voidedBy, reason));
    }

    public void Cancel()
    {
        if (Status == PaymentStatus.Completed) throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = PaymentStatus.Cancelled;
    }

    public Refund AddRefund(Guid salesOrderId, decimal amount, PaymentMethodType method, string reason, Guid requestedBy)
    {
        if (Status != PaymentStatus.Completed && Status != PaymentStatus.PartiallyRefunded)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);

        var totalRefunded = _refunds.Where(r => r.Status == RefundStatus.Processed).Sum(r => r.RefundAmount);
        if (totalRefunded + amount > Amount)
            throw new BusinessException(ErrorCodes.RefundExceedsPaid);

        var refund = new Refund(Id, salesOrderId, amount, Currency, method, reason, requestedBy);
        _refunds.Add(refund);
        RaiseDomainEvent(new RefundRequestedEvent(refund.Id, TenantId, Id, amount));
        return refund;
    }

    public void MarkRefunded(decimal refundAmount, Guid refundId, Guid orderId)
    {
        var totalRefunded = _refunds.Where(r => r.Status == RefundStatus.Processed).Sum(r => r.RefundAmount) + refundAmount;
        Status = totalRefunded >= Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
        RaiseDomainEvent(new PaymentRefundedEvent(Id, refundId, refundAmount, orderId));
    }

    public decimal RefundedAmount => _refunds.Where(r => r.Status == RefundStatus.Processed).Sum(r => r.RefundAmount);
    public decimal RemainingRefundable => Amount - RefundedAmount;
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class PaymentAllocation : AuditableBaseEntity
{
    public Guid PaymentId { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public decimal AllocatedAmount { get; private set; }
    public string Currency { get; private set; }

    private PaymentAllocation() { Currency = "SAR"; }

    internal PaymentAllocation(Guid paymentId, Guid salesOrderId, decimal amount, string currency)
    {
        PaymentId = paymentId;
        SalesOrderId = salesOrderId;
        AllocatedAmount = amount;
        Currency = currency;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class Refund : AuditableBaseEntity
{
    public Guid PaymentId { get; private set; }
    public Guid SalesOrderId { get; private set; }
    public decimal RefundAmount { get; private set; }
    public string Currency { get; private set; }
    public PaymentMethodType RefundMethod { get; private set; }
    public RefundStatus Status { get; private set; }
    public string Reason { get; private set; }
    public Guid RequestedBy { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    private Refund() { Reason = string.Empty; Currency = "SAR"; }

    internal Refund(Guid paymentId, Guid salesOrderId, decimal amount, string currency,
        PaymentMethodType method, string reason, Guid requestedBy)
    {
        PaymentId = paymentId;
        SalesOrderId = salesOrderId;
        RefundAmount = amount;
        Currency = currency;
        RefundMethod = method;
        Reason = reason;
        RequestedBy = requestedBy;
        Status = RefundStatus.Pending;
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != RefundStatus.Pending) throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = RefundStatus.Approved;
        ApprovedBy = approvedBy;
    }

    public void Process()
    {
        if (Status != RefundStatus.Approved && Status != RefundStatus.Pending)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = RefundStatus.Processed;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public void Reject() => Status = RefundStatus.Rejected;
}
