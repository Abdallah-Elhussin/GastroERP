using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Invoicing.DTOs;
using GastroErp.Application.Features.Invoicing.Queries;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Invoicing.Queries;

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, PagedResult<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInvoicesQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<InvoiceDto>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.Invoices.AsNoTracking().Where(i => i.TenantId == request.TenantId);
        if (filter.BranchId.HasValue) query = query.Where(i => i.BranchId == filter.BranchId);
        if (filter.Status.HasValue) query = query.Where(i => i.Status == filter.Status);
        if (filter.InvoiceType.HasValue) query = query.Where(i => i.InvoiceType == filter.InvoiceType);
        if (filter.SalesOrderId.HasValue) query = query.Where(i => i.SalesOrderId == filter.SalesOrderId);
        if (filter.FromDate.HasValue) query = query.Where(i => i.IssuedAt >= filter.FromDate);
        if (filter.ToDate.HasValue) query = query.Where(i => i.IssuedAt <= filter.ToDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);

        var invoices = await query.OrderByDescending(i => i.IssuedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<InvoiceDto>.Success(
            _mapper.Map<IReadOnlyList<InvoiceDto>>(invoices), page, pageSize, totalCount);
    }
}

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInvoiceByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<InvoiceDetailDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices.AsNoTracking()
            .Include(i => i.Lines).Include(i => i.TaxLines)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        return invoice is null
            ? Result<InvoiceDetailDto>.Failure("NotFound", "Invoice not found.")
            : Result<InvoiceDetailDto>.Success(_mapper.Map<InvoiceDetailDto>(invoice));
    }
}

public class GetInvoicesByOrderQueryHandler : IRequestHandler<GetInvoicesByOrderQuery, Result<IReadOnlyList<InvoiceDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInvoicesByOrderQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<InvoiceDto>>> Handle(GetInvoicesByOrderQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _context.Invoices.AsNoTracking()
            .Where(i => i.SalesOrderId == request.OrderId)
            .OrderByDescending(i => i.IssuedAt)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<InvoiceDto>>.Success(_mapper.Map<IReadOnlyList<InvoiceDto>>(invoices));
    }
}

public class GetTaxRatesQueryHandler : IRequestHandler<GetTaxRatesQuery, Result<IReadOnlyList<TaxRateDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTaxRatesQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<TaxRateDto>>> Handle(GetTaxRatesQuery request, CancellationToken cancellationToken)
    {
        var rates = await _context.TaxRates.AsNoTracking()
            .Where(r => r.TenantId == request.TenantId)
            .OrderBy(r => r.Code).ToListAsync(cancellationToken);
        return Result<IReadOnlyList<TaxRateDto>>.Success(_mapper.Map<IReadOnlyList<TaxRateDto>>(rates));
    }
}

public class GetTaxRateByIdQueryHandler : IRequestHandler<GetTaxRateByIdQuery, Result<TaxRateDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTaxRateByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<TaxRateDto>> Handle(GetTaxRateByIdQuery request, CancellationToken cancellationToken)
    {
        var rate = await _context.TaxRates.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        return rate is null
            ? Result<TaxRateDto>.Failure("NotFound", "Tax rate not found.")
            : Result<TaxRateDto>.Success(_mapper.Map<TaxRateDto>(rate));
    }
}

public class GetTaxGroupsQueryHandler : IRequestHandler<GetTaxGroupsQuery, Result<IReadOnlyList<TaxGroupDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTaxGroupsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<TaxGroupDto>>> Handle(GetTaxGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await _context.TaxGroups.AsNoTracking()
            .Include(g => g.Rates)
            .Where(g => g.TenantId == request.TenantId)
            .OrderBy(g => g.NameAr).ToListAsync(cancellationToken);
        return Result<IReadOnlyList<TaxGroupDto>>.Success(_mapper.Map<IReadOnlyList<TaxGroupDto>>(groups));
    }
}

public class GetTaxGroupByIdQueryHandler : IRequestHandler<GetTaxGroupByIdQuery, Result<TaxGroupDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTaxGroupByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<TaxGroupDto>> Handle(GetTaxGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var group = await _context.TaxGroups.AsNoTracking()
            .Include(g => g.Rates)
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);
        return group is null
            ? Result<TaxGroupDto>.Failure("NotFound", "Tax group not found.")
            : Result<TaxGroupDto>.Success(_mapper.Map<TaxGroupDto>(group));
    }
}

public class GetCreditNoteByIdQueryHandler : IRequestHandler<GetCreditNoteByIdQuery, Result<CreditNoteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCreditNoteByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<CreditNoteDto>> Handle(GetCreditNoteByIdQuery request, CancellationToken cancellationToken)
    {
        var note = await _context.CreditNotes.AsNoTracking()
            .Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        return note is null
            ? Result<CreditNoteDto>.Failure("NotFound", "Credit note not found.")
            : Result<CreditNoteDto>.Success(_mapper.Map<CreditNoteDto>(note));
    }
}

public class GetCreditNotesByInvoiceQueryHandler : IRequestHandler<GetCreditNotesByInvoiceQuery, Result<IReadOnlyList<CreditNoteDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCreditNotesByInvoiceQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<CreditNoteDto>>> Handle(GetCreditNotesByInvoiceQuery request, CancellationToken cancellationToken)
    {
        var notes = await _context.CreditNotes.AsNoTracking()
            .Include(c => c.Lines)
            .Where(c => c.OriginalInvoiceId == request.InvoiceId)
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<CreditNoteDto>>.Success(_mapper.Map<IReadOnlyList<CreditNoteDto>>(notes));
    }
}

public class GetDebitNoteByIdQueryHandler : IRequestHandler<GetDebitNoteByIdQuery, Result<DebitNoteDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDebitNoteByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<DebitNoteDto>> Handle(GetDebitNoteByIdQuery request, CancellationToken cancellationToken)
    {
        var note = await _context.DebitNotes.AsNoTracking()
            .Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
        return note is null
            ? Result<DebitNoteDto>.Failure("NotFound", "Debit note not found.")
            : Result<DebitNoteDto>.Success(_mapper.Map<DebitNoteDto>(note));
    }
}

public class GetDebitNotesByInvoiceQueryHandler : IRequestHandler<GetDebitNotesByInvoiceQuery, Result<IReadOnlyList<DebitNoteDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDebitNotesByInvoiceQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<DebitNoteDto>>> Handle(GetDebitNotesByInvoiceQuery request, CancellationToken cancellationToken)
    {
        var notes = await _context.DebitNotes.AsNoTracking()
            .Include(d => d.Lines)
            .Where(d => d.OriginalInvoiceId == request.InvoiceId)
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<DebitNoteDto>>.Success(_mapper.Map<IReadOnlyList<DebitNoteDto>>(notes));
    }
}

public class GetDailySalesReportQueryHandler : IRequestHandler<GetDailySalesReportQuery, Result<DailySalesReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDailySalesReportQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<DailySalesReportDto>> Handle(GetDailySalesReportQuery request, CancellationToken cancellationToken)
    {
        var start = request.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = start.AddDays(1);

        var invoices = await _context.Invoices.AsNoTracking()
            .Where(i => i.TenantId == request.TenantId && i.BranchId == request.BranchId
                && i.Status == InvoiceStatus.Finalized
                && i.FinalizedAt >= start && i.FinalizedAt < end)
            .ToListAsync(cancellationToken);

        var currency = invoices.FirstOrDefault()?.Currency ?? "SAR";
        return Result<DailySalesReportDto>.Success(new DailySalesReportDto(
            request.Date, invoices.Count,
            invoices.Sum(i => i.SubTotal), invoices.Sum(i => i.TaxTotal),
            invoices.Sum(i => i.GrandTotal), currency));
    }
}

public class GetVatSummaryQueryHandler : IRequestHandler<GetVatSummaryQuery, Result<IReadOnlyList<VatSummaryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetVatSummaryQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<IReadOnlyList<VatSummaryDto>>> Handle(GetVatSummaryQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _context.Invoices.AsNoTracking()
            .Include(i => i.TaxLines)
            .Where(i => i.TenantId == request.TenantId && i.BranchId == request.BranchId
                && i.Status == InvoiceStatus.Finalized
                && i.FinalizedAt >= request.From && i.FinalizedAt <= request.To)
            .ToListAsync(cancellationToken);

        var summary = invoices.SelectMany(i => i.TaxLines)
            .GroupBy(t => t.TaxNameAr)
            .Select(g => new VatSummaryDto(
                g.Key, g.Sum(t => t.TaxableAmount), g.Sum(t => t.TaxAmount),
                g.First().Currency))
            .ToList();

        return Result<IReadOnlyList<VatSummaryDto>>.Success(summary);
    }
}

public class GetInvoiceRegisterQueryHandler : IRequestHandler<GetInvoiceRegisterQuery, Result<IReadOnlyList<InvoiceRegisterDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetInvoiceRegisterQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<IReadOnlyList<InvoiceRegisterDto>>> Handle(GetInvoiceRegisterQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Invoices.AsNoTracking()
            .Where(i => i.TenantId == request.TenantId
                && i.Status == InvoiceStatus.Finalized
                && i.FinalizedAt >= request.From && i.FinalizedAt <= request.To);

        if (request.BranchId.HasValue) query = query.Where(i => i.BranchId == request.BranchId);

        var register = await query.OrderBy(i => i.FinalizedAt)
            .Select(i => new InvoiceRegisterDto(
                i.Id, i.InvoiceNumber, i.InvoiceType, i.Status,
                i.IssuedAt, i.GrandTotal, i.TaxTotal, i.Currency))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<InvoiceRegisterDto>>.Success(register);
    }
}

public class GetOutstandingInvoicesQueryHandler : IRequestHandler<GetOutstandingInvoicesQuery, Result<IReadOnlyList<OutstandingInvoiceDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetOutstandingInvoicesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<IReadOnlyList<OutstandingInvoiceDto>>> Handle(GetOutstandingInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Invoices.AsNoTracking()
            .Where(i => i.TenantId == request.TenantId
                && i.Status == InvoiceStatus.Finalized
                && i.PaymentStatus != InvoicePaymentStatus.Paid);

        if (request.BranchId.HasValue) query = query.Where(i => i.BranchId == request.BranchId);

        var outstanding = await query.OrderBy(i => i.IssuedAt)
            .Select(i => new OutstandingInvoiceDto(
                i.Id, i.InvoiceNumber, i.SalesOrderId, i.GrandTotal,
                i.PaidAmount, i.GrandTotal - i.PaidAmount - i.CreditedAmount, i.IssuedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<OutstandingInvoiceDto>>.Success(outstanding);
    }
}

public class GetTaxReportQueryHandler : IRequestHandler<GetTaxReportQuery, Result<TaxReportDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTaxReportQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<TaxReportDto>> Handle(GetTaxReportQuery request, CancellationToken cancellationToken)
    {
        var vatHandler = new GetVatSummaryQueryHandler(_context);
        var vatResult = await vatHandler.Handle(
            new GetVatSummaryQuery(request.TenantId, request.BranchId, request.From, request.To),
            cancellationToken);

        if (!vatResult.IsSuccess) return Result<TaxReportDto>.Failure(vatResult.ErrorCode!, vatResult.ErrorMessage!);

        var breakdown = vatResult.Data ?? [];
        var currency = breakdown.FirstOrDefault()?.Currency ?? "SAR";
        return Result<TaxReportDto>.Success(new TaxReportDto(
            request.From, request.To, breakdown, breakdown.Sum(v => v.TaxAmount), currency));
    }
}
