using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Entities.Inventory.Transactions;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.UnitTests.Inventory;

public sealed class InventoryBalanceTests
{
    private static InventoryBalance CreateBalance() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void ApplyInbound_ComputesWeightedAverage()
    {
        var balance = CreateBalance();
        balance.ApplyInbound(10m, 5m);
        balance.ApplyInbound(10m, 15m);

        Assert.Equal(20m, balance.QtyOnHand);
        Assert.Equal(10m, balance.AvgCost);
    }

    [Fact]
    public void ApplyOutbound_DoesNotChangeAvgCost()
    {
        var balance = CreateBalance();
        balance.ApplyInbound(10m, 8m);
        balance.ApplyOutbound(4m, allowNegative: false);

        Assert.Equal(6m, balance.QtyOnHand);
        Assert.Equal(8m, balance.AvgCost);
    }

    [Fact]
    public void Reserve_ReducesAvailable_NotOnHand()
    {
        var balance = CreateBalance();
        balance.ApplyInbound(10m, 2m);
        balance.Reserve(3m, allowNegative: false);

        Assert.Equal(10m, balance.QtyOnHand);
        Assert.Equal(3m, balance.ReservedQty);
        Assert.Equal(7m, balance.AvailableQty);
    }

    [Fact]
    public void ApplyOutbound_ThrowsWhenInsufficientAndNegativeDisallowed()
    {
        var balance = CreateBalance();
        balance.ApplyInbound(2m, 1m);

        Assert.Throws<BusinessException>(() => balance.ApplyOutbound(5m, allowNegative: false));
    }
}

public sealed class StockMovementContractTests
{
    [Fact]
    public void Quantity_IsAlwaysPositive_DirectionFromMovementType()
    {
        var tx = new InventoryTransaction(
            Guid.NewGuid(),
            TransactionType.GoodsIssue,
            "GI-1",
            Guid.NewGuid());

        var outMovement = tx.AddMovement(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            5m,
            InventoryMovementType.OUT,
            2m);

        var inMovement = tx.AddMovement(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            5m,
            InventoryMovementType.IN,
            2m);

        Assert.Equal(5m, outMovement.Quantity);
        Assert.Equal(-5m, outMovement.QuantityChange);
        Assert.Equal(InventoryMovementType.OUT, outMovement.MovementType);

        Assert.Equal(5m, inMovement.Quantity);
        Assert.Equal(5m, inMovement.QuantityChange);
        Assert.Equal(InventoryMovementType.IN, inMovement.MovementType);
    }

    [Fact]
    public void AddMovement_RejectsNonPositiveQuantity()
    {
        var tx = new InventoryTransaction(
            Guid.NewGuid(),
            TransactionType.GoodsReceipt,
            "GR-1",
            Guid.NewGuid());

        Assert.Throws<ArgumentException>(() =>
            tx.AddMovement(Guid.NewGuid(), Guid.NewGuid(), null, 0m, InventoryMovementType.IN, 1m));
    }
}
