using FluentValidation;
using GastroErp.Application.Features.Finance.Commands;

namespace GastroErp.Application.Features.Finance.Validators;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Dto.AccountNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
    }
}

public class CreateJournalCommandValidator : AbstractValidator<CreateJournalCommand>
{
    public CreateJournalCommandValidator()
    {
        RuleFor(x => x.Dto.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Dto.Lines).NotEmpty();
    }
}

public class UpdateJournalCommandValidator : AbstractValidator<UpdateJournalCommand>
{
    public UpdateJournalCommandValidator()
    {
        RuleFor(x => x.Dto.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Dto.Lines).NotEmpty();
    }
}

public class CreateFiscalPeriodCommandValidator : AbstractValidator<CreateFiscalPeriodCommand>
{
    public CreateFiscalPeriodCommandValidator()
    {
        RuleFor(x => x.Dto.FiscalYear).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Dto.StartMonth).InclusiveBetween((byte)1, (byte)12);
        RuleFor(x => x.Dto.Notes).MaximumLength(1000);
    }
}

public class UpdateFiscalPeriodCommandValidator : AbstractValidator<UpdateFiscalPeriodCommand>
{
    public UpdateFiscalPeriodCommandValidator()
    {
        RuleFor(x => x.Dto.StartMonth).InclusiveBetween((byte)1, (byte)12);
        RuleFor(x => x.Dto.Notes).MaximumLength(1000);
    }
}

public class CreateCostCenterCommandValidator : AbstractValidator<CreateCostCenterCommand>
{
    public CreateCostCenterCommandValidator()
    {
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).MaximumLength(200);
        RuleFor(x => x.Dto.Code).MaximumLength(20);
        RuleFor(x => x.Dto.Description).MaximumLength(500);
    }
}

public class UpdateCostCenterCommandValidator : AbstractValidator<UpdateCostCenterCommand>
{
    public UpdateCostCenterCommandValidator()
    {
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).MaximumLength(200);
        RuleFor(x => x.Dto.Description).MaximumLength(500);
    }
}

public class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyCommandValidator()
    {
        RuleFor(x => x.Dto.Code).NotEmpty().Length(3);
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Symbol).MaximumLength(10);
        RuleFor(x => x.Dto.SubUnitNameAr).MaximumLength(50);
        RuleFor(x => x.Dto.SubUnitNameEn).MaximumLength(50);
        RuleFor(x => x.Dto.DecimalPlaces).Must(d => d is 0 or 2 or 3 or 4);
        RuleFor(x => x.Dto.CurrentExchangeRate).GreaterThan(0);
    }
}

public class UpdateCurrencyCommandValidator : AbstractValidator<UpdateCurrencyCommand>
{
    public UpdateCurrencyCommandValidator()
    {
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Symbol).MaximumLength(10);
        RuleFor(x => x.Dto.SubUnitNameAr).MaximumLength(50);
        RuleFor(x => x.Dto.SubUnitNameEn).MaximumLength(50);
        RuleFor(x => x.Dto.DecimalPlaces).Must(d => d is 0 or 2 or 3 or 4);
        RuleFor(x => x.Dto.CurrentExchangeRate!).GreaterThan(0).When(x => x.Dto.CurrentExchangeRate.HasValue);
    }
}

public class CreateCurrencyExchangeRateCommandValidator : AbstractValidator<CreateCurrencyExchangeRateCommand>
{
    public CreateCurrencyExchangeRateCommandValidator()
    {
        RuleFor(x => x.Dto.CurrencyId).NotEmpty();
        RuleFor(x => x.Dto.Rate).GreaterThan(0);
        RuleFor(x => x.Dto.ChangeReason).MaximumLength(500);
        RuleFor(x => x.Dto.EndDate)
            .GreaterThanOrEqualTo(x => x.Dto.StartDate)
            .When(x => x.Dto.EndDate.HasValue);
    }
}

public class UpdateCurrencyExchangeRateCommandValidator : AbstractValidator<UpdateCurrencyExchangeRateCommand>
{
    public UpdateCurrencyExchangeRateCommandValidator()
    {
        RuleFor(x => x.Dto.Rate).GreaterThan(0);
        RuleFor(x => x.Dto.ChangeReason).MaximumLength(500);
        RuleFor(x => x.Dto.EndDate)
            .GreaterThanOrEqualTo(x => x.Dto.StartDate)
            .When(x => x.Dto.EndDate.HasValue);
    }
}

public class CreateDocumentTypeCommandValidator : AbstractValidator<CreateDocumentTypeCommand>
{
    public CreateDocumentTypeCommandValidator()
    {
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Prefix).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Dto.Suffix).MaximumLength(20);
        RuleFor(x => x.Dto.Description).MaximumLength(500);
        RuleFor(x => x.Dto.NumberLength).InclusiveBetween((byte)1, (byte)12);
        RuleFor(x => x.Dto.StartingNumber).GreaterThanOrEqualTo(0);
    }
}

public class UpdateDocumentTypeCommandValidator : AbstractValidator<UpdateDocumentTypeCommand>
{
    public UpdateDocumentTypeCommandValidator()
    {
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Prefix).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Dto.NumberLength).InclusiveBetween((byte)1, (byte)12);
    }
}

public class CopyDocumentTypeCommandValidator : AbstractValidator<CopyDocumentTypeCommand>
{
    public CopyDocumentTypeCommandValidator()
    {
        RuleFor(x => x.Dto.NewCode).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Prefix).NotEmpty().MaximumLength(20);
    }
}

public class CreateBankCommandValidator : AbstractValidator<CreateBankCommand>
{
    public CreateBankCommandValidator()
    {
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).MaximumLength(200);
        RuleFor(x => x.Dto.Code).MaximumLength(30);
        RuleFor(x => x.Dto.SwiftCode).MaximumLength(20);
        RuleFor(x => x.Dto.DefaultIban).MaximumLength(50);
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.ChartOfAccountId).NotEmpty();
        RuleFor(x => x.Dto.BaseCurrencyId).NotEmpty();
        RuleFor(x => x.Dto.DeactivationReason).MaximumLength(500);
    }
}

public class UpdateBankCommandValidator : AbstractValidator<UpdateBankCommand>
{
    public UpdateBankCommandValidator()
    {
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.ChartOfAccountId).NotEmpty();
        RuleFor(x => x.Dto.BaseCurrencyId).NotEmpty();
    }
}

public class CreateCashBoxCommandValidator : AbstractValidator<CreateCashBoxCommand>
{
    public CreateCashBoxCommandValidator()
    {
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).MaximumLength(200);
        RuleFor(x => x.Dto.LocationName).MaximumLength(200);
        RuleFor(x => x.Dto.Description).MaximumLength(1000);
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.ChartOfAccountId).NotEmpty();
        RuleFor(x => x.Dto.CurrencyId).NotEmpty();
        RuleFor(x => x.Dto.OpeningBalance).GreaterThanOrEqualTo(0);
    }
}

public class UpdateCashBoxCommandValidator : AbstractValidator<UpdateCashBoxCommand>
{
    public UpdateCashBoxCommandValidator()
    {
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.ChartOfAccountId).NotEmpty();
        RuleFor(x => x.Dto.CurrencyId).NotEmpty();
    }
}

public class CreateTaxRegistrationCommandValidator : AbstractValidator<CreateTaxRegistrationCommand>
{
    public CreateTaxRegistrationCommandValidator()
    {
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
        RuleFor(x => x.Dto.VatNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Dto.BranchVatNumber).MaximumLength(30);
        RuleFor(x => x.Dto.TaxOffice).MaximumLength(200);
        RuleFor(x => x.Dto.ActivityCode).MaximumLength(50);
        RuleFor(x => x.Dto.ActivityNameAr).MaximumLength(200);
        RuleFor(x => x.Dto.ActivityNameEn).MaximumLength(200);
        RuleFor(x => x.Dto.Notes).MaximumLength(1000);
        RuleFor(x => x.Dto.DefaultTaxRate).InclusiveBetween(0, 100);
    }
}

public class UpdateTaxRegistrationCommandValidator : AbstractValidator<UpdateTaxRegistrationCommand>
{
    public UpdateTaxRegistrationCommandValidator()
    {
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
        RuleFor(x => x.Dto.VatNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Dto.DefaultTaxRate).InclusiveBetween(0, 100);
    }
}

public class CreateTaxCodeCommandValidator : AbstractValidator<CreateTaxCodeCommand>
{
    public CreateTaxCodeCommandValidator()
    {
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.AppliesTo).IsInEnum();
        RuleFor(x => x.Dto.CalculationMethod).IsInEnum();
        RuleForEach(x => x.Dto.Rates).ChildRules(r =>
        {
            r.RuleFor(x => x.Rate).InclusiveBetween(0, 100);
            r.RuleFor(x => x.FromDate).NotEmpty();
        });
    }
}

public class UpdateTaxCodeCommandValidator : AbstractValidator<UpdateTaxCodeCommand>
{
    public UpdateTaxCodeCommandValidator()
    {
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Dto.NameEn).MaximumLength(150);
        RuleFor(x => x.Dto.AppliesTo).IsInEnum();
        RuleFor(x => x.Dto.CalculationMethod).IsInEnum();
        RuleForEach(x => x.Dto.Rates).ChildRules(r =>
        {
            r.RuleFor(x => x.Rate).InclusiveBetween(0, 100);
            r.RuleFor(x => x.FromDate).NotEmpty();
        });
    }
}

public class CreateNotificationReasonCommandValidator : AbstractValidator<CreateNotificationReasonCommand>
{
    public CreateNotificationReasonCommandValidator()
    {
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).MaximumLength(200);
        RuleFor(x => x.Dto.NoteType).IsInEnum();
        RuleFor(x => x.Dto.PartyType).IsInEnum();
        RuleFor(x => x.Dto.CounterpartAccountId).NotEmpty();
    }
}

public class UpdateNotificationReasonCommandValidator : AbstractValidator<UpdateNotificationReasonCommand>
{
    public UpdateNotificationReasonCommandValidator()
    {
        RuleFor(x => x.Dto.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Dto.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.NameEn).MaximumLength(200);
        RuleFor(x => x.Dto.NoteType).IsInEnum();
        RuleFor(x => x.Dto.PartyType).IsInEnum();
        RuleFor(x => x.Dto.CounterpartAccountId).NotEmpty();
    }
}

public class CreateFinancialOpeningBalanceCommandValidator : AbstractValidator<CreateFinancialOpeningBalanceCommand>
{
    public CreateFinancialOpeningBalanceCommandValidator()
    {
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
        RuleFor(x => x.Dto.FiscalPeriodId).NotEmpty();
        RuleFor(x => x.Dto.OpeningDate).NotEmpty();
        RuleFor(x => x.Dto.Description).MaximumLength(500);
        RuleForEach(x => x.Dto.Lines).ChildRules(l =>
        {
            l.RuleFor(x => x.ChartOfAccountId).NotEmpty();
            l.RuleFor(x => x.Debit).GreaterThanOrEqualTo(0);
            l.RuleFor(x => x.Credit).GreaterThanOrEqualTo(0);
        });
    }
}

public class UpdateFinancialOpeningBalanceCommandValidator : AbstractValidator<UpdateFinancialOpeningBalanceCommand>
{
    public UpdateFinancialOpeningBalanceCommandValidator()
    {
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
        RuleFor(x => x.Dto.FiscalPeriodId).NotEmpty();
        RuleFor(x => x.Dto.OpeningDate).NotEmpty();
        RuleFor(x => x.Dto.Description).MaximumLength(500);
        RuleForEach(x => x.Dto.Lines).ChildRules(l =>
        {
            l.RuleFor(x => x.ChartOfAccountId).NotEmpty();
            l.RuleFor(x => x.Debit).GreaterThanOrEqualTo(0);
            l.RuleFor(x => x.Credit).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreateGeneralLedgerSettingCommandValidator : AbstractValidator<CreateGeneralLedgerSettingCommand>
{
    public CreateGeneralLedgerSettingCommandValidator()
    {
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.VoucherNumberLength).InclusiveBetween(4, 12);
        RuleFor(x => x.Dto.DecimalPlaces).InclusiveBetween(0, 4);
        RuleFor(x => x.Dto.ClosingMethod).IsInEnum();
    }
}

public class UpdateGeneralLedgerSettingCommandValidator : AbstractValidator<UpdateGeneralLedgerSettingCommand>
{
    public UpdateGeneralLedgerSettingCommandValidator()
    {
        RuleFor(x => x.Dto.CompanyId).NotEmpty();
        RuleFor(x => x.Dto.BranchId).NotEmpty();
        RuleFor(x => x.Dto.VoucherNumberLength).InclusiveBetween(4, 12);
        RuleFor(x => x.Dto.DecimalPlaces).InclusiveBetween(0, 4);
        RuleFor(x => x.Dto.ClosingMethod).IsInEnum();
    }
}
