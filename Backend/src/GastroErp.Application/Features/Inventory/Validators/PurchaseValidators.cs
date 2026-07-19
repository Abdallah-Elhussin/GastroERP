using FluentValidation;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Domain.Enums;

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
        RuleFor(x => x.Dto.WarehouseId).NotEmpty();
        RuleFor(x => x.Dto.GrnNumber).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Dto.GrnNumber));
        RuleFor(x => x.Dto.PurchaseOrderId)
            .NotEmpty()
            .When(x => !x.Dto.DirectReceipt);
        RuleFor(x => x.Dto.SupplierId)
            .NotEmpty()
            .When(x => x.Dto.DirectReceipt);
        RuleFor(x => x.Dto.ExchangeRate).GreaterThan(0);
    }
}

public class UpdateGoodsReceiptCommandValidator : AbstractValidator<UpdateGoodsReceiptCommand>
{
    public UpdateGoodsReceiptCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Dto.WarehouseId).NotEmpty();
        RuleFor(x => x.Dto.ExchangeRate).GreaterThan(0);
    }
}

public class AddGoodsReceiptLineCommandValidator : AbstractValidator<AddGoodsReceiptLineCommand>
{
    public AddGoodsReceiptLineCommandValidator()
    {
        RuleFor(x => x.GoodsReceiptId).NotEmpty();
        RuleFor(x => x.Dto.InventoryItemId).NotEmpty();
        RuleFor(x => x.Dto.UnitId).NotEmpty();
        RuleFor(x => x.Dto.ReceivedQuantity).GreaterThan(0);
    }
}

public class CreatePurchaseReturnCommandValidator : AbstractValidator<CreatePurchaseReturnCommand>
{
    public CreatePurchaseReturnCommandValidator()
    {
        RuleFor(x => x.Dto.WarehouseId).NotEmpty()
            .When(x => x.Dto.ReturnType != PurchaseReturnType.BeforeInvoice || x.Dto.WarehouseId != Guid.Empty);
        RuleFor(x => x.Dto.GoodsReceiptId).NotEmpty()
            .When(x => x.Dto.ReturnType == PurchaseReturnType.BeforeInvoice);
        RuleFor(x => x.Dto.PurchaseInvoiceId).NotEmpty()
            .When(x => x.Dto.ReturnType is PurchaseReturnType.AfterInvoice or PurchaseReturnType.Direct);
        RuleFor(x => x.Dto.ReturnNumber).MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.ReturnNumber));
    }
}

public class UpdatePurchaseReturnCommandValidator : AbstractValidator<UpdatePurchaseReturnCommand>
{
    public UpdatePurchaseReturnCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
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
