using Asp.Versioning;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Finance.Queries;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Finance;

[ApiVersion("1.0")]
public class ChartOfAccountsController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.Accounts)]
    [HasPermission(Permissions.Accounting.View)]
    public async Task<IActionResult> GetAccounts([FromQuery] AccountFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetAccountsQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.Accounts}/tree")]
    [HasPermission(Permissions.Accounting.View)]
    public async Task<IActionResult> GetAccountTree(
        [FromQuery] AccountType? accountType = null,
        [FromQuery] bool includeInactive = true,
        [FromQuery] string? search = null)
        => HandleResult(await Mediator.Send(new GetAccountTreeQuery(TenantId, accountType, includeInactive, search)));

    [HttpGet($"{ApiRoutes.Finance.Accounts}/export")]
    [HasPermission(Permissions.Accounting.View)]
    public async Task<IActionResult> ExportAccounts()
    {
        var result = await Mediator.Send(new ExportAccountsQuery(TenantId));
        if (!result.IsSuccess) return HandleResult(result);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("AccountNumber,NameAr,NameEn,ParentAccountNumber,Level,AccountType,AccountCategory,Currency,IsSummaryAccount,IsSystemAccount,IsActive,SortOrder,Notes");
        foreach (var r in result.Data!)
        {
            static string Esc(string? v) => $"\"{(v ?? string.Empty).Replace("\"", "\"\"")}\"";
            sb.AppendLine(string.Join(',',
                Esc(r.AccountNumber), Esc(r.NameAr), Esc(r.NameEn), Esc(r.ParentAccountNumber),
                r.Level, (int)r.AccountType, (int)r.AccountCategory, Esc(r.Currency),
                r.IsSummaryAccount, r.IsSystemAccount, r.IsActive, r.SortOrder, Esc(r.Notes)));
        }

        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", "chart-of-accounts.csv");
    }

    [HttpGet($"{ApiRoutes.Finance.Accounts}/excel-template")]
    [HasPermission(Permissions.Accounting.View)]
    public IActionResult DownloadTemplate()
    {
        const string csv =
            "AccountNumber,NameAr,NameEn,ParentAccountNumber,AccountType,AccountCategory,Currency,IsSummaryAccount,SortOrder,Notes\n" +
            "1500,أصول أخرى,Other Assets,,1,1,SAR,false,99,\n";
        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv)).ToArray();
        return File(bytes, "text/csv", "chart-of-accounts-template.csv");
    }

    [HttpGet($"{ApiRoutes.Finance.Accounts}/{{id:guid}}")]
    [HasPermission(Permissions.Accounting.View)]
    public async Task<IActionResult> GetAccountById(Guid id)
        => HandleResult(await Mediator.Send(new GetAccountByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.Accounts)]
    [HasPermission(Permissions.Accounting.Create)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
        => HandleResult(await Mediator.Send(new CreateAccountCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Finance.Accounts}/{{id:guid}}")]
    [HasPermission(Permissions.Accounting.Update)]
    public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountDto dto)
        => HandleResult(await Mediator.Send(new UpdateAccountCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Finance.Accounts}/{{id:guid}}/reparent")]
    [HasPermission(Permissions.Accounting.Update)]
    public async Task<IActionResult> ReparentAccount(Guid id, [FromBody] ReparentAccountDto dto)
        => HandleResult(await Mediator.Send(new ReparentAccountCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Finance.Accounts}/{{id:guid}}/renumber")]
    [HasPermission(Permissions.Accounting.Update)]
    public async Task<IActionResult> RenumberAccount(Guid id, [FromBody] RenumberAccountDto dto)
        => HandleResult(await Mediator.Send(new RenumberAccountCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Finance.Accounts}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Accounting.Update)]
    public async Task<IActionResult> ActivateAccount(Guid id)
        => HandleResult(await Mediator.Send(new ActivateAccountCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.Accounts}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Accounting.Update)]
    public async Task<IActionResult> DeactivateAccount(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateAccountCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.Accounts}/import")]
    [HasPermission(Permissions.Accounting.Create)]
    public async Task<IActionResult> ImportAccounts([FromBody] ImportAccountsRequest body)
        => HandleResult(await Mediator.Send(new ImportAccountsCommand(TenantId, body.Rows, body.Commit)));

    [HttpDelete($"{ApiRoutes.Finance.Accounts}/{{id:guid}}")]
    [HasPermission(Permissions.Accounting.Delete)]
    public async Task<IActionResult> DeleteAccount(Guid id)
        => HandleResult(await Mediator.Send(new DeleteAccountCommand(id)));
}

public record ImportAccountsRequest(IReadOnlyList<AccountImportRowDto> Rows, bool Commit);

[ApiVersion("1.0")]
public class AccountingSettingsController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.AccountingSettings)]
    [HasPermission(Permissions.Accounting.View)]
    public async Task<IActionResult> Get()
        => HandleResult(await Mediator.Send(new GetAccountingSettingsQuery(TenantId)));

    [HttpPut(ApiRoutes.Finance.AccountingSettings)]
    [HasPermission(Permissions.Accounting.Update)]
    public async Task<IActionResult> Upsert([FromBody] UpdateAccountingSettingsDto dto)
        => HandleResult(await Mediator.Send(new UpsertAccountingSettingsCommand(TenantId, dto)));
}

[ApiVersion("1.0")]
public class AccountClassificationController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.AccountMainClassifications)]
    [HasPermission(Permissions.Accounting.ClassificationsView)]
    public async Task<IActionResult> GetMain()
        => HandleResult(await Mediator.Send(new GetAccountMainClassificationsQuery(TenantId)));

    [HttpGet(ApiRoutes.Finance.AccountClassifications)]
    [HasPermission(Permissions.Accounting.ClassificationsView)]
    public async Task<IActionResult> GetList([FromQuery] AccountClassificationFilterDto filter)
        => HandleResult(await Mediator.Send(new GetAccountClassificationsQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.AccountClassifications}/{{id:guid}}")]
    [HasPermission(Permissions.Accounting.ClassificationsView)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetAccountClassificationByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.AccountClassifications)]
    [HasPermission(Permissions.Accounting.ClassificationsCreate)]
    public async Task<IActionResult> Create([FromBody] CreateAccountClassificationDto dto)
        => HandleResult(await Mediator.Send(new CreateAccountClassificationCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Finance.AccountClassifications}/{{id:guid}}")]
    [HasPermission(Permissions.Accounting.ClassificationsUpdate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountClassificationDto dto)
        => HandleResult(await Mediator.Send(new UpdateAccountClassificationCommand(id, dto)));

    [HttpDelete($"{ApiRoutes.Finance.AccountClassifications}/{{id:guid}}")]
    [HasPermission(Permissions.Accounting.ClassificationsDelete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteAccountClassificationCommand(id)));
}

[ApiVersion("1.0")]
public class JournalController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Finance.Journals)]
    [HasPermission(Permissions.Journal.View)]
    public async Task<IActionResult> GetJournals([FromQuery] JournalFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetJournalsQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.Journals}/{{id:guid}}")]
    [HasPermission(Permissions.Journal.View)]
    public async Task<IActionResult> GetJournalById(Guid id)
        => HandleResult(await Mediator.Send(new GetJournalByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.Journals)]
    [HasPermission(Permissions.Journal.Create)]
    public async Task<IActionResult> CreateJournal([FromBody] CreateJournalDto dto)
        => HandleResult(await Mediator.Send(new CreateJournalCommand(TenantId, CurrentUserId, dto)));

    [HttpPut($"{ApiRoutes.Finance.Journals}/{{id:guid}}")]
    [HasPermission(Permissions.Journal.Edit)]
    public async Task<IActionResult> UpdateJournal(Guid id, [FromBody] UpdateJournalDto dto)
        => HandleResult(await Mediator.Send(new UpdateJournalCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Finance.Journals}/{{id:guid}}/approve")]
    [HasPermission(Permissions.Journal.Approve)]
    public async Task<IActionResult> ApproveJournal(Guid id)
        => HandleResult(await Mediator.Send(new ApproveJournalCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.Journals}/{{id:guid}}/post")]
    [HasPermission(Permissions.Journal.Post)]
    public async Task<IActionResult> PostJournal(Guid id)
        => HandleResult(await Mediator.Send(new PostJournalCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Finance.Journals}/{{id:guid}}/reverse")]
    [HasPermission(Permissions.Journal.Reverse)]
    public async Task<IActionResult> ReverseJournal(Guid id)
        => HandleResult(await Mediator.Send(new ReverseJournalCommand(id, CurrentUserId)));

    [HttpDelete($"{ApiRoutes.Finance.Journals}/{{id:guid}}")]
    [HasPermission(Permissions.Journal.Delete)]
    public async Task<IActionResult> DeleteJournal(Guid id)
        => HandleResult(await Mediator.Send(new DeleteJournalCommand(id)));
}

[ApiVersion("1.0")]
public class FiscalPeriodController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.FiscalPeriods)]
    [HasPermission(Permissions.FiscalPeriod.View)]
    public async Task<IActionResult> GetFiscalPeriods(
        [FromQuery] string? search = null,
        [FromQuery] FiscalPeriodStatus? status = null)
        => HandleResult(await Mediator.Send(new GetFiscalPeriodsQuery(TenantId, search, status)));

    [HttpGet($"{ApiRoutes.Finance.FiscalPeriods}/{{id:guid}}")]
    [HasPermission(Permissions.FiscalPeriod.View)]
    public async Task<IActionResult> GetFiscalPeriodById(Guid id)
        => HandleResult(await Mediator.Send(new GetFiscalPeriodByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.FiscalPeriods)]
    [HasPermission(Permissions.FiscalPeriod.Create)]
    public async Task<IActionResult> CreateFiscalPeriod([FromBody] CreateFiscalPeriodDto dto)
        => HandleResult(await Mediator.Send(new CreateFiscalPeriodCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Finance.FiscalPeriods}/{{id:guid}}")]
    [HasPermission(Permissions.FiscalPeriod.Edit)]
    public async Task<IActionResult> UpdateFiscalPeriod(Guid id, [FromBody] UpdateFiscalPeriodDto dto)
        => HandleResult(await Mediator.Send(new UpdateFiscalPeriodCommand(id, dto)));

    [HttpDelete($"{ApiRoutes.Finance.FiscalPeriods}/{{id:guid}}")]
    [HasPermission(Permissions.FiscalPeriod.Delete)]
    public async Task<IActionResult> DeleteFiscalPeriod(Guid id)
        => HandleResult(await Mediator.Send(new DeleteFiscalPeriodCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.FiscalPeriods}/{{id:guid}}/generate-details")]
    [HasPermission(Permissions.FiscalPeriod.Edit)]
    public async Task<IActionResult> GenerateFiscalPeriodDetails(Guid id)
        => HandleResult(await Mediator.Send(new GenerateFiscalPeriodDetailsCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.FiscalPeriods}/{{id:guid}}/close")]
    [HasPermission(Permissions.FiscalPeriod.Close)]
    public async Task<IActionResult> CloseFiscalPeriod(Guid id)
        => HandleResult(await Mediator.Send(new CloseFiscalPeriodCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.FiscalPeriods}/{{id:guid}}/lock")]
    [HasPermission(Permissions.FiscalPeriod.Lock)]
    public async Task<IActionResult> LockFiscalPeriod(Guid id)
        => HandleResult(await Mediator.Send(new LockFiscalPeriodCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.FiscalPeriods}/{{id:guid}}/reopen")]
    [HasPermission(Permissions.FiscalPeriod.Reopen)]
    public async Task<IActionResult> ReopenFiscalPeriod(Guid id)
        => HandleResult(await Mediator.Send(new ReopenFiscalPeriodCommand(id)));
}

[ApiVersion("1.0")]
public class CostCenterController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.CostCenters)]
    [HasPermission(Permissions.CostCenter.View)]
    public async Task<IActionResult> GetCostCenters([FromQuery] CostCenterFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetCostCentersQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.CostCenters}/export")]
    [HasPermission(Permissions.CostCenter.Export)]
    public async Task<IActionResult> Export([FromQuery] CostCenterFilterDto filter)
    {
        filter = filter with { Page = 1, PageSize = 500 };
        var result = await Mediator.Send(new GetCostCentersQuery(TenantId, filter));
        if (!result.IsSuccess) return HandlePagedResult(result);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Number,Code,NameAr,NameEn,Type,ParentCode,LinkedAccounts,Status");
        foreach (var r in result.Data ?? [])
        {
            static string Esc(string? v) => $"\"{(v ?? string.Empty).Replace("\"", "\"\"")}\"";
            sb.AppendLine(string.Join(',',
                r.Number, Esc(r.Code), Esc(r.NameAr), Esc(r.NameEn), Esc(r.CostCenterTypeCode),
                "", r.LinkedAccountsCount, Esc(r.Status.ToString())));
        }

        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", "cost-centers.csv");
    }

    [HttpGet($"{ApiRoutes.Finance.CostCenters}/{{id:guid}}")]
    [HasPermission(Permissions.CostCenter.View)]
    public async Task<IActionResult> GetCostCenterById(Guid id)
        => HandleResult(await Mediator.Send(new GetCostCenterByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.CostCenters)]
    [HasPermission(Permissions.CostCenter.Create)]
    public async Task<IActionResult> CreateCostCenter([FromBody] CreateCostCenterDto dto)
        => HandleResult(await Mediator.Send(new CreateCostCenterCommand(TenantId, BranchId, dto)));

    [HttpPut($"{ApiRoutes.Finance.CostCenters}/{{id:guid}}")]
    [HasPermission(Permissions.CostCenter.Update)]
    public async Task<IActionResult> UpdateCostCenter(Guid id, [FromBody] UpdateCostCenterDto dto)
        => HandleResult(await Mediator.Send(new UpdateCostCenterCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Finance.CostCenters}/{{id:guid}}/activate")]
    [HasPermission(Permissions.CostCenter.Activate)]
    public async Task<IActionResult> ActivateCostCenter(Guid id)
        => HandleResult(await Mediator.Send(new ActivateCostCenterCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.CostCenters}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.CostCenter.Update)]
    public async Task<IActionResult> DeactivateCostCenter(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateCostCenterCommand(id)));

    [HttpDelete($"{ApiRoutes.Finance.CostCenters}/{{id:guid}}")]
    [HasPermission(Permissions.CostCenter.Delete)]
    public async Task<IActionResult> DeleteCostCenter(Guid id)
        => HandleResult(await Mediator.Send(new DeleteCostCenterCommand(id)));
}

[ApiVersion("1.0")]
public class CurrencyController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.Currencies)]
    [HasPermission(Permissions.Currency.View)]
    public async Task<IActionResult> GetCurrencies([FromQuery] CurrencyFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetCurrenciesQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.Currencies}/export")]
    [HasPermission(Permissions.Currency.Export)]
    public async Task<IActionResult> Export([FromQuery] CurrencyFilterDto filter)
    {
        filter = filter with { Page = 1, PageSize = 500 };
        var result = await Mediator.Send(new GetCurrenciesQuery(TenantId, filter));
        if (!result.IsSuccess) return HandlePagedResult(result);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Number,Code,NameAr,NameEn,Symbol,Rate,IsCompany,Decimals,Status");
        foreach (var r in result.Data ?? [])
        {
            static string Esc(string? v) => $"\"{(v ?? string.Empty).Replace("\"", "\"\"")}\"";
            sb.AppendLine(string.Join(',',
                r.Number, Esc(r.Code), Esc(r.NameAr), Esc(r.NameEn), Esc(r.Symbol),
                r.CurrentExchangeRate, r.IsCompanyCurrency, r.DecimalPlaces, Esc(r.Status.ToString())));
        }

        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", "currencies.csv");
    }

    [HttpGet($"{ApiRoutes.Finance.Currencies}/{{id:guid}}")]
    [HasPermission(Permissions.Currency.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetCurrencyByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.Currencies)]
    [HasPermission(Permissions.Currency.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCurrencyDto dto)
        => HandleResult(await Mediator.Send(new CreateCurrencyCommand(TenantId, User?.Identity?.Name, dto)));

    [HttpPut($"{ApiRoutes.Finance.Currencies}/{{id:guid}}")]
    [HasPermission(Permissions.Currency.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCurrencyDto dto)
        => HandleResult(await Mediator.Send(new UpdateCurrencyCommand(id, User?.Identity?.Name, dto)));

    [HttpPost($"{ApiRoutes.Finance.Currencies}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Currency.Activate)]
    public async Task<IActionResult> Activate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateCurrencyCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.Currencies}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Currency.Update)]
    public async Task<IActionResult> Deactivate(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateCurrencyCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.Currencies}/{{id:guid}}/set-company")]
    [HasPermission(Permissions.Currency.SetCompany)]
    public async Task<IActionResult> SetCompany(Guid id)
        => HandleResult(await Mediator.Send(new SetCompanyCurrencyCommand(TenantId, id, User?.Identity?.Name)));

    [HttpDelete($"{ApiRoutes.Finance.Currencies}/{{id:guid}}")]
    [HasPermission(Permissions.Currency.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteCurrencyCommand(id)));
}

[ApiVersion("1.0")]
public class CurrencyExchangeRateController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.ExchangeRates)]
    [HasPermission(Permissions.Currency.View)]
    public async Task<IActionResult> GetRates([FromQuery] CurrencyExchangeRateFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetCurrencyExchangeRatesQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.ExchangeRates}/export")]
    [HasPermission(Permissions.Currency.Export)]
    public async Task<IActionResult> Export([FromQuery] CurrencyExchangeRateFilterDto filter)
    {
        filter = filter with { Page = 1, PageSize = 500 };
        var result = await Mediator.Send(new GetCurrencyExchangeRatesQuery(TenantId, filter));
        if (!result.IsSuccess) return HandlePagedResult(result);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Number,CurrencyCode,CurrencyNameAr,Rate,StartDate,EndDate,IsActive,UpdatedBy,UpdatedAt");
        foreach (var r in result.Data ?? [])
        {
            static string Esc(string? v) => $"\"{(v ?? string.Empty).Replace("\"", "\"\"")}\"";
            sb.AppendLine(string.Join(',',
                r.Number, Esc(r.CurrencyCode), Esc(r.CurrencyNameAr), r.Rate,
                r.StartDate, r.EndDate?.ToString() ?? "", r.IsActive, Esc(r.UpdatedBy), Esc(r.UpdatedAt?.ToString("O"))));
        }

        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", "exchange-rates.csv");
    }

    [HttpPost(ApiRoutes.Finance.ExchangeRates)]
    [HasPermission(Permissions.Currency.ManageRates)]
    public async Task<IActionResult> Create([FromBody] CreateCurrencyExchangeRateDto dto)
        => HandleResult(await Mediator.Send(new CreateCurrencyExchangeRateCommand(TenantId, User?.Identity?.Name, dto)));

    [HttpPut($"{ApiRoutes.Finance.ExchangeRates}/{{id:guid}}")]
    [HasPermission(Permissions.Currency.ManageRates)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCurrencyExchangeRateDto dto)
        => HandleResult(await Mediator.Send(new UpdateCurrencyExchangeRateCommand(id, User?.Identity?.Name, dto)));

    [HttpPost($"{ApiRoutes.Finance.ExchangeRates}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Currency.Activate)]
    public async Task<IActionResult> Activate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateCurrencyExchangeRateCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.ExchangeRates}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Currency.ManageRates)]
    public async Task<IActionResult> Deactivate(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateCurrencyExchangeRateCommand(id)));

    [HttpDelete($"{ApiRoutes.Finance.ExchangeRates}/{{id:guid}}")]
    [HasPermission(Permissions.Currency.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteCurrencyExchangeRateCommand(id)));
}

[ApiVersion("1.0")]
public class DocumentTypeController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.DocumentTypes)]
    [HasPermission(Permissions.DocumentType.View)]
    public async Task<IActionResult> GetList([FromQuery] DocumentTypeFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetDocumentTypesQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.DocumentTypes}/{{id:guid}}")]
    [HasPermission(Permissions.DocumentType.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetDocumentTypeByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.DocumentTypes)]
    [HasPermission(Permissions.DocumentType.Create)]
    public async Task<IActionResult> Create([FromBody] UpsertDocumentTypeDto dto)
        => HandleResult(await Mediator.Send(new CreateDocumentTypeCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Finance.DocumentTypes}/{{id:guid}}")]
    [HasPermission(Permissions.DocumentType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertDocumentTypeDto dto)
        => HandleResult(await Mediator.Send(new UpdateDocumentTypeCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Finance.DocumentTypes}/{{id:guid}}/activate")]
    [HasPermission(Permissions.DocumentType.Activate)]
    public async Task<IActionResult> Activate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateDocumentTypeCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.DocumentTypes}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.DocumentType.Update)]
    public async Task<IActionResult> Deactivate(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateDocumentTypeCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.DocumentTypes}/{{id:guid}}/copy")]
    [HasPermission(Permissions.DocumentType.Create)]
    public async Task<IActionResult> Copy(Guid id, [FromBody] CopyDocumentTypeDto dto)
        => HandleResult(await Mediator.Send(new CopyDocumentTypeCommand(id, dto)));

    [HttpDelete($"{ApiRoutes.Finance.DocumentTypes}/{{id:guid}}")]
    [HasPermission(Permissions.DocumentType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteDocumentTypeCommand(id)));
}

[ApiVersion("1.0")]
public class BankController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.Banks)]
    [HasPermission(Permissions.Bank.View)]
    public async Task<IActionResult> GetList([FromQuery] BankFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetBanksQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.Banks}/{{id:guid}}")]
    [HasPermission(Permissions.Bank.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetBankByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.Banks)]
    [HasPermission(Permissions.Bank.Create)]
    public async Task<IActionResult> Create([FromBody] UpsertBankDto dto)
        => HandleResult(await Mediator.Send(new CreateBankCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Finance.Banks}/{{id:guid}}")]
    [HasPermission(Permissions.Bank.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertBankDto dto)
        => HandleResult(await Mediator.Send(new UpdateBankCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Finance.Banks}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Bank.Activate)]
    public async Task<IActionResult> Activate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateBankCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.Banks}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Bank.Update)]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateBankRequest? body = null)
        => HandleResult(await Mediator.Send(new DeactivateBankCommand(id, body?.DeactivatedAt, body?.Reason)));

    [HttpDelete($"{ApiRoutes.Finance.Banks}/{{id:guid}}")]
    [HasPermission(Permissions.Bank.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteBankCommand(id)));
}

public sealed record DeactivateBankRequest(DateOnly? DeactivatedAt = null, string? Reason = null);

[ApiVersion("1.0")]
public class CashBoxController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.CashBoxes)]
    [HasPermission(Permissions.CashBox.View)]
    public async Task<IActionResult> GetList([FromQuery] CashBoxFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetCashBoxesQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.CashBoxes}/{{id:guid}}")]
    [HasPermission(Permissions.CashBox.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetCashBoxByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.CashBoxes)]
    [HasPermission(Permissions.CashBox.Create)]
    public async Task<IActionResult> Create([FromBody] UpsertCashBoxDto dto)
        => HandleResult(await Mediator.Send(new CreateCashBoxCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Finance.CashBoxes}/{{id:guid}}")]
    [HasPermission(Permissions.CashBox.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertCashBoxDto dto)
        => HandleResult(await Mediator.Send(new UpdateCashBoxCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Finance.CashBoxes}/{{id:guid}}/activate")]
    [HasPermission(Permissions.CashBox.Activate)]
    public async Task<IActionResult> Activate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateCashBoxCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.CashBoxes}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.CashBox.Update)]
    public async Task<IActionResult> Deactivate(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateCashBoxCommand(id)));

    [HttpDelete($"{ApiRoutes.Finance.CashBoxes}/{{id:guid}}")]
    [HasPermission(Permissions.CashBox.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteCashBoxCommand(id)));
}

[ApiVersion("1.0")]
public class TaxRegistrationController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.TaxRegistrations)]
    [HasPermission(Permissions.TaxRegistration.View)]
    public async Task<IActionResult> GetList([FromQuery] TaxRegistrationFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetTaxRegistrationsQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.TaxRegistrations}/{{id:guid}}")]
    [HasPermission(Permissions.TaxRegistration.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetTaxRegistrationByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.TaxRegistrations)]
    [HasPermission(Permissions.TaxRegistration.Create)]
    public async Task<IActionResult> Create([FromBody] UpsertTaxRegistrationDto dto)
        => HandleResult(await Mediator.Send(new CreateTaxRegistrationCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Finance.TaxRegistrations}/{{id:guid}}")]
    [HasPermission(Permissions.TaxRegistration.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertTaxRegistrationDto dto)
        => HandleResult(await Mediator.Send(new UpdateTaxRegistrationCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Finance.TaxRegistrations}/{{id:guid}}/certificate")]
    [HasPermission(Permissions.TaxRegistration.UploadCertificate)]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadCertificate(Guid id, [FromForm] TaxCertificateUploadRequest request)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(new { error = "Certificate file is required." });

        await using var stream = request.File.OpenReadStream();
        DateOnly? issue = DateOnly.TryParse(request.IssueDate, out var i) ? i : null;
        DateOnly? expiry = DateOnly.TryParse(request.ExpiryDate, out var e) ? e : null;

        return HandleResult(await Mediator.Send(new UploadTaxRegistrationCertificateCommand(
            id, request.File.FileName, stream, request.File.ContentType,
            request.DocumentNumber, issue, expiry, request.Notes)));
    }

    [HttpDelete($"{ApiRoutes.Finance.TaxRegistrations}/{{id:guid}}")]
    [HasPermission(Permissions.TaxRegistration.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteTaxRegistrationCommand(id)));
}

[ApiVersion("1.0")]
public class TaxCodeController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.TaxCodes)]
    [HasPermission(Permissions.TaxCode.View)]
    public async Task<IActionResult> GetList([FromQuery] TaxCodeFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetTaxCodesQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.TaxCodes}/{{id:guid}}")]
    [HasPermission(Permissions.TaxCode.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetTaxCodeByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.TaxCodes)]
    [HasPermission(Permissions.TaxCode.Create)]
    public async Task<IActionResult> Create([FromBody] UpsertTaxCodeDto dto)
        => HandleResult(await Mediator.Send(new CreateTaxCodeCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Finance.TaxCodes}/{{id:guid}}")]
    [HasPermission(Permissions.TaxCode.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertTaxCodeDto dto)
        => HandleResult(await Mediator.Send(new UpdateTaxCodeCommand(id, dto)));

    [HttpDelete($"{ApiRoutes.Finance.TaxCodes}/{{id:guid}}")]
    [HasPermission(Permissions.TaxCode.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteTaxCodeCommand(id)));
}

[ApiVersion("1.0")]
public class NotificationReasonController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.NotificationReasons)]
    [HasPermission(Permissions.NotificationReason.View)]
    public async Task<IActionResult> GetList([FromQuery] NotificationReasonFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetNotificationReasonsQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.NotificationReasons}/{{id:guid}}")]
    [HasPermission(Permissions.NotificationReason.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetNotificationReasonByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.NotificationReasons)]
    [HasPermission(Permissions.NotificationReason.Create)]
    public async Task<IActionResult> Create([FromBody] UpsertNotificationReasonDto dto)
        => HandleResult(await Mediator.Send(new CreateNotificationReasonCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Finance.NotificationReasons}/{{id:guid}}")]
    [HasPermission(Permissions.NotificationReason.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertNotificationReasonDto dto)
        => HandleResult(await Mediator.Send(new UpdateNotificationReasonCommand(id, dto)));

    [HttpDelete($"{ApiRoutes.Finance.NotificationReasons}/{{id:guid}}")]
    [HasPermission(Permissions.NotificationReason.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteNotificationReasonCommand(id)));
}

[ApiVersion("1.0")]
public class FinancialOpeningBalanceController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Finance.OpeningBalances)]
    [HasPermission(Permissions.OpeningBalances.View)]
    public async Task<IActionResult> GetList([FromQuery] FinancialOpeningBalanceFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetFinancialOpeningBalancesQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.OpeningBalances}/{{id:guid}}")]
    [HasPermission(Permissions.OpeningBalances.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetFinancialOpeningBalanceByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.OpeningBalances)]
    [HasPermission(Permissions.OpeningBalances.Create)]
    public async Task<IActionResult> Create([FromBody] UpsertFinancialOpeningBalanceDto dto)
        => HandleResult(await Mediator.Send(new CreateFinancialOpeningBalanceCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Finance.OpeningBalances}/{{id:guid}}")]
    [HasPermission(Permissions.OpeningBalances.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertFinancialOpeningBalanceDto dto)
        => HandleResult(await Mediator.Send(new UpdateFinancialOpeningBalanceCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Finance.OpeningBalances}/{{id:guid}}/post")]
    [HasPermission(Permissions.OpeningBalances.Post)]
    public async Task<IActionResult> Post(Guid id)
        => HandleResult(await Mediator.Send(new PostFinancialOpeningBalanceCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Finance.OpeningBalances}/{{id:guid}}/reverse")]
    [HasPermission(Permissions.OpeningBalances.Reverse)]
    public async Task<IActionResult> Reverse(Guid id)
        => HandleResult(await Mediator.Send(new ReverseFinancialOpeningBalanceCommand(id, CurrentUserId)));

    [HttpDelete($"{ApiRoutes.Finance.OpeningBalances}/{{id:guid}}")]
    [HasPermission(Permissions.OpeningBalances.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteFinancialOpeningBalanceCommand(id)));
}

[ApiVersion("1.0")]
public class ReceiptVoucherController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Finance.ReceiptVouchers)]
    [HasPermission(Permissions.ReceiptVouchers.View)]
    public async Task<IActionResult> GetList([FromQuery] ReceiptVoucherFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetReceiptVouchersQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.ReceiptVouchers}/{{id:guid}}")]
    [HasPermission(Permissions.ReceiptVouchers.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetReceiptVoucherByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.ReceiptVouchers)]
    [HasPermission(Permissions.ReceiptVouchers.Create)]
    public async Task<IActionResult> Create([FromBody] UpsertReceiptVoucherDto dto)
        => HandleResult(await Mediator.Send(new CreateReceiptVoucherCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Finance.ReceiptVouchers}/{{id:guid}}")]
    [HasPermission(Permissions.ReceiptVouchers.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertReceiptVoucherDto dto)
        => HandleResult(await Mediator.Send(new UpdateReceiptVoucherCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Finance.ReceiptVouchers}/{{id:guid}}/submit")]
    [HasPermission(Permissions.ReceiptVouchers.Edit)]
    public async Task<IActionResult> Submit(Guid id)
        => HandleResult(await Mediator.Send(new SubmitReceiptVoucherCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.ReceiptVouchers}/{{id:guid}}/approve")]
    [HasPermission(Permissions.ReceiptVouchers.Approve)]
    public async Task<IActionResult> Approve(Guid id)
        => HandleResult(await Mediator.Send(new ApproveReceiptVoucherCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Finance.ReceiptVouchers}/{{id:guid}}/post")]
    [HasPermission(Permissions.ReceiptVouchers.Post)]
    public async Task<IActionResult> Post(Guid id)
        => HandleResult(await Mediator.Send(new PostReceiptVoucherCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Finance.ReceiptVouchers}/{{id:guid}}/reverse")]
    [HasPermission(Permissions.ReceiptVouchers.Reverse)]
    public async Task<IActionResult> Reverse(Guid id)
        => HandleResult(await Mediator.Send(new ReverseReceiptVoucherCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Finance.ReceiptVouchers}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.ReceiptVouchers.Cancel)]
    public async Task<IActionResult> Cancel(Guid id)
        => HandleResult(await Mediator.Send(new CancelReceiptVoucherCommand(id, CurrentUserId)));

    [HttpDelete($"{ApiRoutes.Finance.ReceiptVouchers}/{{id:guid}}")]
    [HasPermission(Permissions.ReceiptVouchers.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteReceiptVoucherCommand(id)));
}

[ApiVersion("1.0")]
public class FinancialNoteController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Finance.FinancialNotes)]
    [HasPermission(Permissions.FinancialNotes.View)]
    public async Task<IActionResult> GetList([FromQuery] FinancialNoteFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetFinancialNotesQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.FinancialNotes}/{{id:guid}}")]
    [HasPermission(Permissions.FinancialNotes.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetFinancialNoteByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.FinancialNotes)]
    [HasPermission(Permissions.FinancialNotes.Create)]
    public async Task<IActionResult> Create([FromBody] UpsertFinancialNoteDto dto)
        => HandleResult(await Mediator.Send(new CreateFinancialNoteCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Finance.FinancialNotes}/{{id:guid}}")]
    [HasPermission(Permissions.FinancialNotes.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertFinancialNoteDto dto)
        => HandleResult(await Mediator.Send(new UpdateFinancialNoteCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Finance.FinancialNotes}/{{id:guid}}/submit")]
    [HasPermission(Permissions.FinancialNotes.Edit)]
    public async Task<IActionResult> Submit(Guid id)
        => HandleResult(await Mediator.Send(new SubmitFinancialNoteCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.FinancialNotes}/{{id:guid}}/approve")]
    [HasPermission(Permissions.FinancialNotes.Approve)]
    public async Task<IActionResult> Approve(Guid id)
        => HandleResult(await Mediator.Send(new ApproveFinancialNoteCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Finance.FinancialNotes}/{{id:guid}}/post")]
    [HasPermission(Permissions.FinancialNotes.Post)]
    public async Task<IActionResult> Post(Guid id)
        => HandleResult(await Mediator.Send(new PostFinancialNoteCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Finance.FinancialNotes}/{{id:guid}}/reverse")]
    [HasPermission(Permissions.FinancialNotes.Reverse)]
    public async Task<IActionResult> Reverse(Guid id)
        => HandleResult(await Mediator.Send(new ReverseFinancialNoteCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Finance.FinancialNotes}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.FinancialNotes.Cancel)]
    public async Task<IActionResult> Cancel(Guid id)
        => HandleResult(await Mediator.Send(new CancelFinancialNoteCommand(id, CurrentUserId)));

    [HttpDelete($"{ApiRoutes.Finance.FinancialNotes}/{{id:guid}}")]
    [HasPermission(Permissions.FinancialNotes.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteFinancialNoteCommand(id)));
}

public sealed class TaxCertificateUploadRequest
{
    public IFormFile? File { get; set; }
    public string? DocumentNumber { get; set; }
    public string? IssueDate { get; set; }
    public string? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

[ApiVersion("1.0")]
public class GeneralLedgerSettingsController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.GeneralLedgerSettings)]
    [HasPermission(Permissions.GeneralLedgerSettings.View)]
    public async Task<IActionResult> GetList([FromQuery] GeneralLedgerSettingFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetGeneralLedgerSettingsQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.GeneralLedgerSettings}/{{id:guid}}")]
    [HasPermission(Permissions.GeneralLedgerSettings.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetGeneralLedgerSettingByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.GeneralLedgerSettings)]
    [HasPermission(Permissions.GeneralLedgerSettings.Create)]
    public async Task<IActionResult> Create([FromBody] UpsertGeneralLedgerSettingDto dto)
        => HandleResult(await Mediator.Send(new CreateGeneralLedgerSettingCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Finance.GeneralLedgerSettings}/{{id:guid}}")]
    [HasPermission(Permissions.GeneralLedgerSettings.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertGeneralLedgerSettingDto dto)
        => HandleResult(await Mediator.Send(new UpdateGeneralLedgerSettingCommand(id, dto)));

    [HttpDelete($"{ApiRoutes.Finance.GeneralLedgerSettings}/{{id:guid}}")]
    [HasPermission(Permissions.GeneralLedgerSettings.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteGeneralLedgerSettingCommand(id)));
}

[ApiVersion("1.0")]
public class AccountingReportsController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Finance.Reports}/trial-balance")]
    [HasPermission(Permissions.ReportsAccounting.View)]
    public async Task<IActionResult> GetTrialBalance([FromQuery] TrialBalanceFilterDto filter)
        => HandleResult(await Mediator.Send(new GetTrialBalanceQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.Reports}/general-ledger")]
    [HasPermission(Permissions.GeneralLedger.View)]
    public async Task<IActionResult> GetGeneralLedger([FromQuery] GeneralLedgerFilterDto filter)
        => HandleResult(await Mediator.Send(new GetGeneralLedgerQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.Reports}/account-statement")]
    [HasPermission(Permissions.ReportsAccounting.View)]
    public async Task<IActionResult> GetAccountStatement([FromQuery] AccountStatementFilterDto filter)
        => HandleResult(await Mediator.Send(new GetAccountStatementQuery(filter)));

    [HttpGet($"{ApiRoutes.Finance.Reports}/journal-register")]
    [HasPermission(Permissions.ReportsAccounting.View)]
    public async Task<IActionResult> GetJournalRegister([FromQuery] JournalRegisterFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetJournalRegisterQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.Reports}/balance-verification")]
    [HasPermission(Permissions.ReportsAccounting.View)]
    public async Task<IActionResult> GetBalanceVerification([FromQuery] TrialBalanceFilterDto filter)
        => HandleResult(await Mediator.Send(new GetBalanceVerificationQuery(TenantId, filter)));
}
