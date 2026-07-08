using FluentValidation;
using GastroErp.Application.Features.Inventory.Commands;

namespace GastroErp.Application.Features.Inventory.Validators;

public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Dto.SupplierId).NotEmpty();
        RuleFor(x => x.Dto.DestinationWarehouseId).NotEmpty();
        RuleFor(x => x.Dto.PoNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Dto.ExpectedDeliveryDate).NotEmpty();
    }
}

public class AddPurchaseOrderLineCommandValidator : AbstractValidator<AddPurchaseOrderLineCommand>
{
    public AddPurchaseOrderLineCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
        RuleFor(x => x.Dto.InventoryItemId).NotEmpty();
        RuleFor(x => x.Dto.UnitId).NotEmpty();
        RuleFor(x => x.Dto.Quantity).GreaterThan(0);
        RuleFor(x => x.Dto.UnitPrice).GreaterThanOrEqualTo(0);
    }
}

public class CreateGoodsReceiptCommandValidator : AbstractValidator<CreateGoodsReceiptCommand>
{
    public CreateGoodsReceiptCommandValidator()
    {
        RuleFor(x => x.Dto.PurchaseOrderId).NotEmpty();
        RuleFor(x => x.Dto.GrnNumber).NotEmpty().MaximumLength(50);
    }
}

public class AddGoodsReceiptLineCommandValidator : AbstractValidator<AddGoodsReceiptLineCommand>
{
    public AddGoodsReceiptLineCommandValidator()
    {
        RuleFor(x => x.GoodsReceiptId).NotEmpty();
        RuleFor(x => x.Dto.PurchaseOrderLineId).NotEmpty();
        RuleFor(x => x.Dto.ReceivedQuantity).GreaterThan(0);
    }
}

public class CreatePurchaseReturnCommandValidator : AbstractValidator<CreatePurchaseReturnCommand>
{
    public CreatePurchaseReturnCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.SupplierId).NotEmpty();
        RuleFor(x => x.Dto.WarehouseId).NotEmpty();
        RuleFor(x => x.Dto.ReturnNumber).NotEmpty().MaximumLength(50);
    }
}

public class AddPurchaseReturnLineCommandValidator : AbstractValidator<AddPurchaseReturnLineCommand>
{
    public AddPurchaseReturnLineCommandValidator()
    {
        RuleFor(x => x.PurchaseReturnId).NotEmpty();
        RuleFor(x => x.Dto.InventoryItemId).NotEmpty();
        RuleFor(x => x.Dto.UnitId).NotEmpty();
        RuleFor(x => x.Dto.ReturnQuantity).GreaterThan(0);
        RuleFor(x => x.Dto.UnitCost).GreaterThanOrEqualTo(0);
    }
}
