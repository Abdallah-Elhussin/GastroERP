using FluentValidation;
using GastroErp.Application.Features.Sales.ProductPricing.Commands;

namespace GastroErp.Application.Features.Sales.ProductPricing;

public sealed class CreateSalesPriceListCommandValidator : AbstractValidator<CreateSalesPriceListCommand>
{
    public CreateSalesPriceListCommandValidator()
    {
        RuleFor(x => x.Request.TenantId).NotEmpty();
        RuleFor(x => x.Request.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.NameEn).MaximumLength(200);
        RuleFor(x => x.Request.Description).MaximumLength(500);
    }
}

public sealed class UpdateSalesPriceListCommandValidator : AbstractValidator<UpdateSalesPriceListCommand>
{
    public UpdateSalesPriceListCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.TenantId).NotEmpty();
        RuleFor(x => x.Request.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.NameEn).MaximumLength(200);
        RuleFor(x => x.Request.Description).MaximumLength(500);
    }
}

public sealed class CreateProductPriceCommandValidator : AbstractValidator<CreateProductPriceCommand>
{
    public CreateProductPriceCommandValidator()
    {
        RuleFor(x => x.Request.TenantId).NotEmpty();
        RuleFor(x => x.Request.ProductId).NotEmpty();
        RuleFor(x => x.Request.PriceListId).NotEmpty();
        RuleFor(x => x.Request.UnitId).NotEmpty();
        RuleFor(x => x.Request.Cost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.ProfitMargin).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.ProfitAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.MaximumDiscount).InclusiveBetween(0, 100).When(x => x.Request.MaximumDiscount.HasValue);
        RuleFor(x => x.Request.Notes).MaximumLength(1000);
    }
}

public sealed class CreateProductPricesBatchCommandValidator : AbstractValidator<CreateProductPricesBatchCommand>
{
    public CreateProductPricesBatchCommandValidator()
    {
        RuleFor(x => x.Request.TenantId).NotEmpty();
        RuleFor(x => x.Request.ProductId).NotEmpty();
        RuleFor(x => x.Request.PriceListId).NotEmpty();
        RuleFor(x => x.Request.Lines).NotEmpty();
        RuleForEach(x => x.Request.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.UnitId).NotEmpty();
            line.RuleFor(l => l.Cost).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.SellingPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class UpdateProductPriceCommandValidator : AbstractValidator<UpdateProductPriceCommand>
{
    public UpdateProductPriceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.TenantId).NotEmpty();
        RuleFor(x => x.Request.PriceListId).NotEmpty();
        RuleFor(x => x.Request.UnitId).NotEmpty();
        RuleFor(x => x.Request.Cost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.SellingPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.MaximumDiscount).InclusiveBetween(0, 100).When(x => x.Request.MaximumDiscount.HasValue);
        RuleFor(x => x.Request.Notes).MaximumLength(1000);
    }
}

public sealed class CopyPriceListCommandValidator : AbstractValidator<CopyPriceListCommand>
{
    public CopyPriceListCommandValidator()
    {
        RuleFor(x => x.Request.TenantId).NotEmpty();
        RuleFor(x => x.Request.SourcePriceListId).NotEmpty();
        RuleFor(x => x.Request.TargetPriceListId).NotEmpty();
    }
}
