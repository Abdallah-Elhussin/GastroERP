using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.Commands;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Common.Localization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Queries;

public sealed class GetReceiptVouchersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetReceiptVouchersQuery, PagedResult<ReceiptVoucherDto>>
{
    public async Task<PagedResult<ReceiptVoucherDto>> Handle(
        GetReceiptVouchersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = context.ReceiptVouchers.AsNoTracking()
            .Include(d => d.Lines)
            .Where(d => d.TenantId == request.TenantId);

        if (filter.CompanyId is Guid companyId)
            query = query.Where(d => d.CompanyId == companyId);
        if (filter.BranchId is Guid branchId)
            query = query.Where(d => d.BranchId == branchId);
        if (filter.FiscalPeriodId is Guid periodId)
            query = query.Where(d => d.FiscalPeriodId == periodId);
        if (filter.Status is { } status)
            query = query.Where(d => d.Status == status);
        if (filter.ReceiptMethod is { } method)
            query = query.Where(d => d.ReceiptMethod == method);
        if (filter.CashBoxId is Guid cashBoxId)
            query = query.Where(d => d.CashBoxId == cashBoxId);
        if (filter.BankId is Guid bankId)
            query = query.Where(d => d.BankId == bankId);
        if (!string.IsNullOrWhiteSpace(filter.Currency))
            query = query.Where(d => d.Currency == filter.Currency.Trim().ToUpperInvariant());
        if (filter.FromDate is DateOnly from)
            query = query.Where(d => d.VoucherDate >= from);
        if (filter.ToDate is DateOnly to)
            query = query.Where(d => d.VoucherDate <= to);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            query = query.Where(d =>
                d.DocumentNumber.Contains(s)
                || (d.PartyName != null && d.PartyName.Contains(s))
                || (d.Reference != null && d.Reference.Contains(s))
                || (d.Description != null && d.Description.Contains(s)));
        }

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 500 ? 200 : filter.PageSize;
        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(d => d.VoucherDate)
            .ThenByDescending(d => d.Number)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<ReceiptVoucherDto>(rows.Count);
        foreach (var row in rows)
            items.Add(await CreateReceiptVoucherCommandHandler.EnrichAsync(context, row, cancellationToken));

        return PagedResult<ReceiptVoucherDto>.Success(items, page, pageSize, total);
    }
}

public sealed class GetReceiptVoucherByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetReceiptVoucherByIdQuery, Result<ReceiptVoucherDto>>
{
    public async Task<Result<ReceiptVoucherDto>> Handle(
        GetReceiptVoucherByIdQuery request, CancellationToken cancellationToken)
    {
        var exists = await context.ReceiptVouchers.AsNoTracking()
            .AnyAsync(d => d.Id == request.Id, cancellationToken);
        if (!exists)
            return Result<ReceiptVoucherDto>.Failure(ErrorCodes.ReceiptVoucherNotFound, "Receipt voucher not found.");

        return Result<ReceiptVoucherDto>.Success(
            await CreateReceiptVoucherCommandHandler.LoadDtoAsync(context, request.Id, cancellationToken));
    }
}
