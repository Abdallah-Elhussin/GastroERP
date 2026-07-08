using Asp.Versioning;
using GastroErp.Application.Features.Invoicing.Commands;
using GastroErp.Application.Features.Invoicing.DTOs;
using GastroErp.Application.Features.Invoicing.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Invoicing;

[ApiVersion("1.0")]
public class InvoiceController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Sales.Invoices)]
    [HasPermission(Permissions.Invoice.View)]
    public async Task<IActionResult> GetInvoices([FromQuery] InvoiceFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetInvoicesQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Sales.Invoices}/{{id:guid}}")]
    [HasPermission(Permissions.Invoice.View)]
    public async Task<IActionResult> GetInvoiceById(Guid id)
        => HandleResult(await Mediator.Send(new GetInvoiceByIdQuery(id)));

    [HttpGet($"{ApiRoutes.Sales.Invoices}/order/{{orderId:guid}}")]
    [HasPermission(Permissions.Invoice.View)]
    public async Task<IActionResult> GetInvoicesByOrder(Guid orderId)
        => HandleResult(await Mediator.Send(new GetInvoicesByOrderQuery(orderId)));

    [HttpPost(ApiRoutes.Sales.Invoices)]
    [HasPermission(Permissions.Invoice.Create)]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDto dto)
        => HandleResult(await Mediator.Send(new CreateInvoiceCommand(TenantId, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Sales.Invoices}/{{id:guid}}/finalize")]
    [HasPermission(Permissions.Invoice.Finalize)]
    public async Task<IActionResult> FinalizeInvoice(Guid id)
        => HandleResult(await Mediator.Send(new FinalizeInvoiceCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Sales.Invoices}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.Invoice.Cancel)]
    public async Task<IActionResult> CancelInvoice(Guid id, [FromBody] CancelInvoiceDto dto)
        => HandleResult(await Mediator.Send(new CancelInvoiceCommand(id, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Sales.Invoices}/{{id:guid}}/print")]
    [HasPermission(Permissions.Invoice.Print)]
    public async Task<IActionResult> PrintInvoice(Guid id, [FromBody] PrintInvoiceDto dto)
        => HandleResult(await Mediator.Send(new PrintInvoiceCommand(id, dto)));
}

[ApiVersion("1.0")]
public class TaxController : BaseApiController
{
    [HttpGet(ApiRoutes.Sales.TaxRates)]
    [HasPermission(Permissions.Tax.View)]
    public async Task<IActionResult> GetTaxRates()
        => HandleResult(await Mediator.Send(new GetTaxRatesQuery(TenantId)));

    [HttpGet($"{ApiRoutes.Sales.TaxRates}/{{id:guid}}")]
    [HasPermission(Permissions.Tax.View)]
    public async Task<IActionResult> GetTaxRateById(Guid id)
        => HandleResult(await Mediator.Send(new GetTaxRateByIdQuery(id)));

    [HttpPost(ApiRoutes.Sales.TaxRates)]
    [HasPermission(Permissions.Tax.Manage)]
    public async Task<IActionResult> CreateTaxRate([FromBody] CreateTaxRateDto dto)
        => HandleResult(await Mediator.Send(new CreateTaxRateCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Sales.TaxRates}/{{id:guid}}")]
    [HasPermission(Permissions.Tax.Manage)]
    public async Task<IActionResult> UpdateTaxRate(Guid id, [FromBody] UpdateTaxRateDto dto)
        => HandleResult(await Mediator.Send(new UpdateTaxRateCommand(id, dto)));

    [HttpDelete($"{ApiRoutes.Sales.TaxRates}/{{id:guid}}")]
    [HasPermission(Permissions.Tax.Manage)]
    public async Task<IActionResult> DeleteTaxRate(Guid id)
        => HandleResult(await Mediator.Send(new DeleteTaxRateCommand(id)));

    [HttpGet(ApiRoutes.Sales.TaxGroups)]
    [HasPermission(Permissions.Tax.View)]
    public async Task<IActionResult> GetTaxGroups()
        => HandleResult(await Mediator.Send(new GetTaxGroupsQuery(TenantId)));

    [HttpGet($"{ApiRoutes.Sales.TaxGroups}/{{id:guid}}")]
    [HasPermission(Permissions.Tax.View)]
    public async Task<IActionResult> GetTaxGroupById(Guid id)
        => HandleResult(await Mediator.Send(new GetTaxGroupByIdQuery(id)));

    [HttpPost(ApiRoutes.Sales.TaxGroups)]
    [HasPermission(Permissions.Tax.Manage)]
    public async Task<IActionResult> CreateTaxGroup([FromBody] CreateTaxGroupDto dto)
        => HandleResult(await Mediator.Send(new CreateTaxGroupCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Sales.TaxGroups}/{{id:guid}}")]
    [HasPermission(Permissions.Tax.Manage)]
    public async Task<IActionResult> UpdateTaxGroup(Guid id, [FromBody] UpdateTaxGroupDto dto)
        => HandleResult(await Mediator.Send(new UpdateTaxGroupCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Sales.TaxGroups}/{{taxGroupId:guid}}/rates")]
    [HasPermission(Permissions.Tax.Manage)]
    public async Task<IActionResult> AddTaxGroupRate(Guid taxGroupId, [FromBody] AddTaxGroupRateDto dto)
        => HandleResult(await Mediator.Send(new AddTaxGroupRateCommand(taxGroupId, dto)));

    [HttpDelete($"{ApiRoutes.Sales.TaxGroups}/{{taxGroupId:guid}}/rates/{{taxRateId:guid}}")]
    [HasPermission(Permissions.Tax.Manage)]
    public async Task<IActionResult> RemoveTaxGroupRate(Guid taxGroupId, Guid taxRateId)
        => HandleResult(await Mediator.Send(new RemoveTaxGroupRateCommand(taxGroupId, taxRateId)));
}

[ApiVersion("1.0")]
public class CreditNoteController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Sales.CreditNotes}/{{id:guid}}")]
    [HasPermission(Permissions.CreditNote.View)]
    public async Task<IActionResult> GetCreditNoteById(Guid id)
        => HandleResult(await Mediator.Send(new GetCreditNoteByIdQuery(id)));

    [HttpGet($"{ApiRoutes.Sales.CreditNotes}/invoice/{{invoiceId:guid}}")]
    [HasPermission(Permissions.CreditNote.View)]
    public async Task<IActionResult> GetCreditNotesByInvoice(Guid invoiceId)
        => HandleResult(await Mediator.Send(new GetCreditNotesByInvoiceQuery(invoiceId)));

    [HttpPost(ApiRoutes.Sales.CreditNotes)]
    [HasPermission(Permissions.CreditNote.Create)]
    public async Task<IActionResult> CreateCreditNote([FromBody] CreateCreditNoteDto dto)
        => HandleResult(await Mediator.Send(new CreateCreditNoteCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Sales.CreditNotes}/{{id:guid}}/issue")]
    [HasPermission(Permissions.CreditNote.Create)]
    public async Task<IActionResult> IssueCreditNote(Guid id)
        => HandleResult(await Mediator.Send(new IssueCreditNoteCommand(id)));

    [HttpPost($"{ApiRoutes.Sales.CreditNotes}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.CreditNote.Cancel)]
    public async Task<IActionResult> CancelCreditNote(Guid id, [FromBody] CancelCreditNoteDto dto)
        => HandleResult(await Mediator.Send(new CancelCreditNoteCommand(id, dto)));
}

[ApiVersion("1.0")]
public class DebitNoteController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Sales.DebitNotes}/{{id:guid}}")]
    [HasPermission(Permissions.DebitNote.View)]
    public async Task<IActionResult> GetDebitNoteById(Guid id)
        => HandleResult(await Mediator.Send(new GetDebitNoteByIdQuery(id)));

    [HttpGet($"{ApiRoutes.Sales.DebitNotes}/invoice/{{invoiceId:guid}}")]
    [HasPermission(Permissions.DebitNote.View)]
    public async Task<IActionResult> GetDebitNotesByInvoice(Guid invoiceId)
        => HandleResult(await Mediator.Send(new GetDebitNotesByInvoiceQuery(invoiceId)));

    [HttpPost(ApiRoutes.Sales.DebitNotes)]
    [HasPermission(Permissions.DebitNote.Create)]
    public async Task<IActionResult> CreateDebitNote([FromBody] CreateDebitNoteDto dto)
        => HandleResult(await Mediator.Send(new CreateDebitNoteCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Sales.DebitNotes}/{{id:guid}}/issue")]
    [HasPermission(Permissions.DebitNote.Create)]
    public async Task<IActionResult> IssueDebitNote(Guid id)
        => HandleResult(await Mediator.Send(new IssueDebitNoteCommand(id)));

    [HttpPost($"{ApiRoutes.Sales.DebitNotes}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.DebitNote.Cancel)]
    public async Task<IActionResult> CancelDebitNote(Guid id, [FromBody] CancelDebitNoteDto dto)
        => HandleResult(await Mediator.Send(new CancelDebitNoteCommand(id, dto)));
}

[ApiVersion("1.0")]
public class InvoicingReportController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Sales.InvoicingReports}/daily-sales")]
    [HasPermission(Permissions.Invoice.View)]
    public async Task<IActionResult> GetDailySales([FromQuery] Guid branchId, [FromQuery] DateOnly date)
        => HandleResult(await Mediator.Send(new GetDailySalesReportQuery(TenantId, branchId, date)));

    [HttpGet($"{ApiRoutes.Sales.InvoicingReports}/vat-summary")]
    [HasPermission(Permissions.Invoice.View)]
    public async Task<IActionResult> GetVatSummary([FromQuery] Guid branchId, [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to)
        => HandleResult(await Mediator.Send(new GetVatSummaryQuery(TenantId, branchId, from, to)));

    [HttpGet($"{ApiRoutes.Sales.InvoicingReports}/invoice-register")]
    [HasPermission(Permissions.Invoice.View)]
    public async Task<IActionResult> GetInvoiceRegister([FromQuery] Guid? branchId, [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to)
        => HandleResult(await Mediator.Send(new GetInvoiceRegisterQuery(TenantId, branchId, from, to)));

    [HttpGet($"{ApiRoutes.Sales.InvoicingReports}/outstanding")]
    [HasPermission(Permissions.Invoice.View)]
    public async Task<IActionResult> GetOutstandingInvoices([FromQuery] Guid? branchId)
        => HandleResult(await Mediator.Send(new GetOutstandingInvoicesQuery(TenantId, branchId)));

    [HttpGet($"{ApiRoutes.Sales.InvoicingReports}/tax-report")]
    [HasPermission(Permissions.Invoice.View)]
    public async Task<IActionResult> GetTaxReport([FromQuery] Guid branchId, [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to)
        => HandleResult(await Mediator.Send(new GetTaxReportQuery(TenantId, branchId, from, to)));
}
