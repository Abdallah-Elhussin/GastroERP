using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Queries;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.Queries;

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPaymentByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<PaymentDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Allocations)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        return payment is null
            ? Result<PaymentDto>.Failure("NotFound", "Payment not found.")
            : Result<PaymentDto>.Success(_mapper.Map<PaymentDto>(payment));
    }
}

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, PagedResult<PaymentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPaymentsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<PaymentDto>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.Payments.AsNoTracking().Where(p => p.TenantId == request.TenantId);

        if (filter.BranchId.HasValue) query = query.Where(p => p.BranchId == filter.BranchId);
        if (filter.CashierShiftId.HasValue) query = query.Where(p => p.CashierShiftId == filter.CashierShiftId);
        if (filter.Status.HasValue) query = query.Where(p => p.Status == filter.Status);
        if (filter.PaymentMethod.HasValue) query = query.Where(p => p.PaymentMethod == filter.PaymentMethod);
        if (filter.FromDate.HasValue) query = query.Where(p => p.ProcessedAt >= filter.FromDate);
        if (filter.ToDate.HasValue) query = query.Where(p => p.ProcessedAt <= filter.ToDate);

        if (filter.SalesOrderId.HasValue)
        {
            query = query.Where(p => p.Allocations.Any(a => a.SalesOrderId == filter.SalesOrderId));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);

        var payments = await query
            .Include(p => p.Allocations)
            .OrderByDescending(p => p.ProcessedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<PaymentDto>.Success(_mapper.Map<IReadOnlyList<PaymentDto>>(payments), page, pageSize, totalCount);
    }
}

public class GetPaymentsByOrderQueryHandler : IRequestHandler<GetPaymentsByOrderQuery, Result<IReadOnlyList<PaymentDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPaymentsByOrderQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<IReadOnlyList<PaymentDto>>> Handle(GetPaymentsByOrderQuery request, CancellationToken cancellationToken)
    {
        var payments = await _context.Payments
            .AsNoTracking()
            .Include(p => p.Allocations)
            .Where(p => p.Allocations.Any(a => a.SalesOrderId == request.OrderId))
            .OrderByDescending(p => p.ProcessedAt)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<PaymentDto>>.Success(_mapper.Map<IReadOnlyList<PaymentDto>>(payments));
    }
}

public class GetCashRegisterByIdQueryHandler : IRequestHandler<GetCashRegisterByIdQuery, Result<CashRegisterDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCashRegisterByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<CashRegisterDto>> Handle(GetCashRegisterByIdQuery request, CancellationToken cancellationToken)
    {
        var register = await _context.CashRegisters.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        return register is null
            ? Result<CashRegisterDto>.Failure("NotFound", "Cash register not found.")
            : Result<CashRegisterDto>.Success(_mapper.Map<CashRegisterDto>(register));
    }
}

public class GetCashRegistersQueryHandler : IRequestHandler<GetCashRegistersQuery, PagedResult<CashRegisterDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCashRegistersQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<CashRegisterDto>> Handle(GetCashRegistersQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.CashRegisters.AsNoTracking().Where(r => r.TenantId == request.TenantId);
        if (filter.BranchId.HasValue) query = query.Where(r => r.BranchId == filter.BranchId);
        if (filter.Status.HasValue) query = query.Where(r => r.Status == filter.Status);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);

        var registers = await query.OrderBy(r => r.NameAr)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<CashRegisterDto>.Success(_mapper.Map<IReadOnlyList<CashRegisterDto>>(registers), page, pageSize, totalCount);
    }
}

public class GetCurrentCashRegisterQueryHandler : IRequestHandler<GetCurrentCashRegisterQuery, Result<CashRegisterDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCurrentCashRegisterQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<CashRegisterDto>> Handle(GetCurrentCashRegisterQuery request, CancellationToken cancellationToken)
    {
        var register = await _context.CashRegisters.AsNoTracking()
            .Where(r => r.BranchId == request.BranchId && r.Status == RegisterStatus.Open)
            .FirstOrDefaultAsync(cancellationToken);

        return register is null
            ? Result<CashRegisterDto>.Failure("NotFound", "No open cash register found.")
            : Result<CashRegisterDto>.Success(_mapper.Map<CashRegisterDto>(register));
    }
}

public class GetShiftByIdQueryHandler : IRequestHandler<GetShiftByIdQuery, Result<CashierShiftDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetShiftByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<CashierShiftDto>> Handle(GetShiftByIdQuery request, CancellationToken cancellationToken)
    {
        var shift = await _context.CashierShifts.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        return shift is null
            ? Result<CashierShiftDto>.Failure("NotFound", "Shift not found.")
            : Result<CashierShiftDto>.Success(_mapper.Map<CashierShiftDto>(shift));
    }
}

public class GetShiftsQueryHandler : IRequestHandler<GetShiftsQuery, PagedResult<CashierShiftDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetShiftsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<CashierShiftDto>> Handle(GetShiftsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.CashierShifts.AsNoTracking().Where(s => s.TenantId == request.TenantId);
        if (filter.BranchId.HasValue) query = query.Where(s => s.BranchId == filter.BranchId);
        if (filter.CashierId.HasValue) query = query.Where(s => s.CashierId == filter.CashierId);
        if (filter.Status.HasValue) query = query.Where(s => s.Status == filter.Status);
        if (filter.FromDate.HasValue) query = query.Where(s => s.OpenedAt >= filter.FromDate);
        if (filter.ToDate.HasValue) query = query.Where(s => s.OpenedAt <= filter.ToDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);

        var shifts = await query.OrderByDescending(s => s.OpenedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<CashierShiftDto>.Success(_mapper.Map<IReadOnlyList<CashierShiftDto>>(shifts), page, pageSize, totalCount);
    }
}

public class GetCurrentShiftQueryHandler : IRequestHandler<GetCurrentShiftQuery, Result<CashierShiftDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCurrentShiftQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<CashierShiftDto>> Handle(GetCurrentShiftQuery request, CancellationToken cancellationToken)
    {
        var activeStatuses = new[] { ShiftStatus.Open, ShiftStatus.Active, ShiftStatus.Suspended, ShiftStatus.Closing };
        var shift = await _context.CashierShifts.AsNoTracking()
            .Where(s => s.CashierId == request.CashierId && s.DeviceId == request.DeviceId && activeStatuses.Contains(s.Status))
            .FirstOrDefaultAsync(cancellationToken);

        return shift is null
            ? Result<CashierShiftDto>.Failure("NotFound", "No active shift found.")
            : Result<CashierShiftDto>.Success(_mapper.Map<CashierShiftDto>(shift));
    }
}

public class GetCashMovementsQueryHandler : IRequestHandler<GetCashMovementsQuery, PagedResult<CashMovementDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCashMovementsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<CashMovementDto>> Handle(GetCashMovementsQuery request, CancellationToken cancellationToken)
    {
        var filter = request.Filter;
        var query = _context.CashierShifts.AsNoTracking()
            .Where(s => s.TenantId == request.TenantId)
            .SelectMany(s => s.CashMovements);

        if (filter.CashierShiftId.HasValue) query = query.Where(m => m.CashierShiftId == filter.CashierShiftId);
        if (filter.CashRegisterId.HasValue) query = query.Where(m => m.CashRegisterId == filter.CashRegisterId);
        if (filter.MovementType.HasValue) query = query.Where(m => m.MovementType == filter.MovementType);
        if (filter.FromDate.HasValue) query = query.Where(m => m.CreatedAtMovement >= filter.FromDate);
        if (filter.ToDate.HasValue) query = query.Where(m => m.CreatedAtMovement <= filter.ToDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page = Math.Max(filter.Page, 1);

        var movements = await query.OrderByDescending(m => m.CreatedAtMovement)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return PagedResult<CashMovementDto>.Success(_mapper.Map<IReadOnlyList<CashMovementDto>>(movements), page, pageSize, totalCount);
    }
}
