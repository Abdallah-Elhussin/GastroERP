using FluentValidation;
using GastroErp.Application.Features.Inventory.Commands;

namespace GastroErp.Application.Features.Inventory.Validators;

public sealed class UpdateInventorySettingsCommandValidator : AbstractValidator<UpdateInventorySettingsCommand>
{
    public UpdateInventorySettingsCommandValidator()
    {
        RuleFor(x => x.Dto.TenantId).NotEmpty();
        RuleFor(x => x.Dto.DefaultCurrencyCode).MaximumLength(10);
        RuleFor(x => x.Dto.CostPrecision).InclusiveBetween((byte)0, (byte)6);
        RuleForEach(x => x.Dto.DocumentSeries).ChildRules(s =>
        {
            s.RuleFor(x => x.Prefix).NotEmpty().MaximumLength(20);
            s.RuleFor(x => x.NumberLength).InclusiveBetween((byte)1, (byte)12);
            s.RuleFor(x => x.NextNumber).GreaterThan(0);
        });
    }
}

public sealed class UpsertInventorySettingCommandValidator : AbstractValidator<UpsertInventorySettingCommand>
{
    public UpsertInventorySettingCommandValidator()
    {
        RuleFor(x => x.Dto.TenantId).NotEmpty();
        RuleFor(x => x.Dto.CostPrecision).InclusiveBetween((byte)0, (byte)6);
    }
}
