using Asp.Versioning;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Finance.Queries;
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
    public async Task<IActionResult> GetAccountTree()
        => HandleResult(await Mediator.Send(new GetAccountTreeQuery(TenantId)));

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

    [HttpPost($"{ApiRoutes.Finance.Accounts}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Accounting.Update)]
    public async Task<IActionResult> ActivateAccount(Guid id)
        => HandleResult(await Mediator.Send(new ActivateAccountCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.Accounts}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Accounting.Update)]
    public async Task<IActionResult> DeactivateAccount(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateAccountCommand(id)));

    [HttpDelete($"{ApiRoutes.Finance.Accounts}/{{id:guid}}")]
    [HasPermission(Permissions.Accounting.Delete)]
    public async Task<IActionResult> DeleteAccount(Guid id)
        => HandleResult(await Mediator.Send(new DeleteAccountCommand(id)));
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

    [HttpPost($"{ApiRoutes.Finance.Journals}/{{id:guid}}/post")]
    [HasPermission(Permissions.Journal.Post)]
    public async Task<IActionResult> PostJournal(Guid id)
        => HandleResult(await Mediator.Send(new PostJournalCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Finance.Journals}/{{id:guid}}/reverse")]
    [HasPermission(Permissions.Journal.Reverse)]
    public async Task<IActionResult> ReverseJournal(Guid id)
        => HandleResult(await Mediator.Send(new ReverseJournalCommand(id, CurrentUserId)));
}

[ApiVersion("1.0")]
public class FiscalPeriodController : BaseApiController
{
    [HttpGet(ApiRoutes.Finance.FiscalPeriods)]
    [HasPermission(Permissions.FiscalPeriod.View)]
    public async Task<IActionResult> GetFiscalPeriods()
        => HandleResult(await Mediator.Send(new GetFiscalPeriodsQuery(TenantId)));

    [HttpGet($"{ApiRoutes.Finance.FiscalPeriods}/{{id:guid}}")]
    [HasPermission(Permissions.FiscalPeriod.View)]
    public async Task<IActionResult> GetFiscalPeriodById(Guid id)
        => HandleResult(await Mediator.Send(new GetFiscalPeriodByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.FiscalPeriods)]
    [HasPermission(Permissions.FiscalPeriod.Create)]
    public async Task<IActionResult> CreateFiscalPeriod([FromBody] CreateFiscalPeriodDto dto)
        => HandleResult(await Mediator.Send(new CreateFiscalPeriodCommand(TenantId, dto)));

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

    [HttpGet($"{ApiRoutes.Finance.CostCenters}/{{id:guid}}")]
    [HasPermission(Permissions.CostCenter.View)]
    public async Task<IActionResult> GetCostCenterById(Guid id)
        => HandleResult(await Mediator.Send(new GetCostCenterByIdQuery(id)));

    [HttpPost(ApiRoutes.Finance.CostCenters)]
    [HasPermission(Permissions.CostCenter.Create)]
    public async Task<IActionResult> CreateCostCenter([FromBody] CreateCostCenterDto dto)
        => HandleResult(await Mediator.Send(new CreateCostCenterCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Finance.CostCenters}/{{id:guid}}")]
    [HasPermission(Permissions.CostCenter.Update)]
    public async Task<IActionResult> UpdateCostCenter(Guid id, [FromBody] UpdateCostCenterDto dto)
        => HandleResult(await Mediator.Send(new UpdateCostCenterCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Finance.CostCenters}/{{id:guid}}/activate")]
    [HasPermission(Permissions.CostCenter.Update)]
    public async Task<IActionResult> ActivateCostCenter(Guid id)
        => HandleResult(await Mediator.Send(new ActivateCostCenterCommand(id)));

    [HttpPost($"{ApiRoutes.Finance.CostCenters}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.CostCenter.Update)]
    public async Task<IActionResult> DeactivateCostCenter(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateCostCenterCommand(id)));
}

[ApiVersion("1.0")]
public class AccountingReportsController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Finance.Reports}/trial-balance")]
    [HasPermission(Permissions.ReportsAccounting.View)]
    public async Task<IActionResult> GetTrialBalance([FromQuery] TrialBalanceFilterDto filter)
        => HandleResult(await Mediator.Send(new GetTrialBalanceQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Finance.Reports}/general-ledger")]
    [HasPermission(Permissions.ReportsAccounting.View)]
    public async Task<IActionResult> GetGeneralLedger([FromQuery] GeneralLedgerFilterDto filter)
        => HandleResult(await Mediator.Send(new GetGeneralLedgerQuery(filter)));

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
