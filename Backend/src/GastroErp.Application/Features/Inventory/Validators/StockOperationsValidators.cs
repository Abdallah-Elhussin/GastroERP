using FluentValidation;
using GastroErp.Application.Features.Inventory.Commands;

namespace GastroErp.Application.Features.Inventory.Validators;

public class CreateStockTransferCommandValidator : AbstractValidator<CreateStockTransferCommand>
{
    public CreateStockTransferCommandValidator()
    {
        RuleFor(x => x.Dto.SourceWarehouseId).NotEmpty();
        RuleFor(x => x.Dto.DestinationWarehouseId).NotEmpty();
        RuleFor(x => x.Dto.TransferNumber).NotEmpty().MaximumLength(50);
    }
}

public class AddTransferLineCommandValidator : AbstractValidator<AddTransferLineCommand>
{
    public AddTransferLineCommandValidator()
    {
        RuleFor(x => x.TransferId).NotEmpty();
        RuleFor(x => x.Dto.InventoryItemId).NotEmpty();
        RuleFor(x => x.Dto.UnitId).NotEmpty();
        RuleFor(x => x.Dto.Quantity).GreaterThan(0);
    }
}

public class CreateStockAdjustmentCommandValidator : AbstractValidator<CreateStockAdjustmentCommand>
{
    public CreateStockAdjustmentCommandValidator()
    {
        RuleFor(x => x.Dto.WarehouseId).NotEmpty();
        RuleFor(x => x.Dto.AdjustmentNumber).NotEmpty().MaximumLength(50);
    }
}

public class CreateWasteRecordCommandValidator : AbstractValidator<CreateWasteRecordCommand>
{
    public CreateWasteRecordCommandValidator()
    {
        RuleFor(x => x.Dto.WarehouseId).NotEmpty();
        RuleFor(x => x.Dto.WasteNumber).NotEmpty().MaximumLength(50);
    }
}

public class CreateStockCountCommandValidator : AbstractValidator<CreateStockCountCommand>
{
    public CreateStockCountCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.WarehouseId).NotEmpty();
        RuleFor(x => x.Dto.CountNumber).NotEmpty().MaximumLength(50);
    }
}

public class AddStockCountLineCommandValidator : AbstractValidator<AddStockCountLineCommand>
{
    public AddStockCountLineCommandValidator()
    {
        RuleFor(x => x.StockCountId).NotEmpty();
        RuleFor(x => x.Dto.InventoryItemId).NotEmpty();
        RuleFor(x => x.Dto.UnitId).NotEmpty();
        RuleFor(x => x.Dto.ExpectedQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.ActualQuantity).GreaterThanOrEqualTo(0);
    }
}

public class ReserveStockCommandValidator : AbstractValidator<ReserveStockCommand>
{
    public ReserveStockCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Dto.WarehouseId).NotEmpty();
        RuleFor(x => x.Dto.InventoryItemId).NotEmpty();
        RuleFor(x => x.Dto.ReservedQuantity).GreaterThan(0);
        RuleFor(x => x.Dto.SourceDocument).NotEmpty().MaximumLength(100);
    }
}
