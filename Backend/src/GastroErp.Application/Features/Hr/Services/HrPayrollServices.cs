using System.Text.Json;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Application.Features.Hr.DTOs;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Entities.HR;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Hr.Services;

public interface IPayrollService
{
    Task<PayrollRunDto> CreateRunAsync(Guid tenantId, CreatePayrollRunDto dto, CancellationToken ct = default);
    Task<PayrollRunDto> CalculateRunAsync(Guid tenantId, Guid runId, CancellationToken ct = default);
    Task<PayrollRunDto> ApproveRunAsync(Guid tenantId, Guid runId, Guid approverId, CancellationToken ct = default);
    Task<Result> PostRunAsync(Guid tenantId, Guid runId, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<PayrollRunDto>> GetRunsAsync(Guid tenantId, Guid companyId, CancellationToken ct = default);
    Task<IReadOnlyList<PayslipDto>> GetPayslipsAsync(Guid tenantId, Guid runId, CancellationToken ct = default);
    Task<IReadOnlyList<PayslipDto>> GetPayslipsForEmployeeAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default);
    Task<SalaryStructureDto> UpsertSalaryStructureAsync(Guid tenantId, UpsertSalaryStructureDto dto, CancellationToken ct = default);
    Task<SalaryStructureDto?> GetSalaryStructureAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default);
}

public sealed class PayrollService : IPayrollService
{
    private readonly IApplicationDbContext _context;
    private readonly IAutoPostingService _posting;
    private readonly ILogger<PayrollService> _logger;

    public PayrollService(IApplicationDbContext context, IAutoPostingService posting, ILogger<PayrollService> logger)
        => (_context, _posting, _logger) = (context, posting, logger);

    public async Task<PayrollRunDto> CreateRunAsync(Guid tenantId, CreatePayrollRunDto dto, CancellationToken ct = default)
    {
        var run = PayrollRun.Create(tenantId, dto.CompanyId, dto.Year, dto.Month);
        _context.PayrollRuns.Add(run);
        await _context.SaveChangesAsync(ct);
        return MapRun(run);
    }

    public async Task<PayrollRunDto> CalculateRunAsync(Guid tenantId, Guid runId, CancellationToken ct = default)
    {
        var run = await _context.PayrollRuns.FirstAsync(r => r.TenantId == tenantId && r.Id == runId, ct);
        var existing = await _context.PayrollPayslips.Where(p => p.PayrollRunId == runId).ToListAsync(ct);
        _context.PayrollPayslips.RemoveRange(existing);

        var employees = await _context.Employees.AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.CompanyId == run.CompanyId && e.Status == EmploymentStatus.Active)
            .ToListAsync(ct);

        decimal totalGross = 0, totalDed = 0, totalNet = 0;
        foreach (var emp in employees)
        {
            var salary = await _context.SalaryStructures.AsNoTracking()
                .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.EmployeeId == emp.Id && s.IsActive, ct);
            if (salary is null) continue;

            var monthStart = new DateOnly(run.Year, run.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var overtimePay = await CalcOvertimePayAsync(tenantId, emp.Id, monthStart, monthEnd, salary.BaseSalary, ct);
            var bonuses = await _context.PerformanceRecords.AsNoTracking()
                .Where(p => p.TenantId == tenantId && p.EmployeeId == emp.Id && p.RecordType == PerformanceRecordType.Reward
                    && p.RecordDate >= monthStart && p.RecordDate <= monthEnd)
                .SumAsync(p => p.Score ?? 0, ct);

            var gross = salary.GrossSalary + overtimePay + bonuses;
            var deductions = salary.FixedDeductions;
            var net = gross - deductions;

            var components = JsonSerializer.Serialize(new[]
            {
                new { Type = PayComponentType.BaseSalary, Amount = salary.BaseSalary },
                new { Type = PayComponentType.Allowance, Amount = salary.HousingAllowance + salary.TransportAllowance + salary.OtherAllowances },
                new { Type = PayComponentType.Overtime, Amount = overtimePay },
                new { Type = PayComponentType.Bonus, Amount = bonuses },
                new { Type = PayComponentType.Deduction, Amount = deductions }
            });

            _context.PayrollPayslips.Add(PayrollPayslip.Create(
                tenantId, runId, emp.Id, gross, deductions, overtimePay, bonuses, net, components, salary.Currency));

            totalGross += gross;
            totalDed += deductions;
            totalNet += net;
        }

        run.SetTotals(totalGross, totalDed, totalNet);
        await _context.SaveChangesAsync(ct);
        return MapRun(run);
    }

    public async Task<PayrollRunDto> ApproveRunAsync(Guid tenantId, Guid runId, Guid approverId, CancellationToken ct = default)
    {
        var run = await _context.PayrollRuns.FirstAsync(r => r.TenantId == tenantId && r.Id == runId, ct);
        run.Approve(approverId);
        var slips = await _context.PayrollPayslips.Where(p => p.PayrollRunId == runId).ToListAsync(ct);
        foreach (var s in slips) s.FinalizePayslip();
        await _context.SaveChangesAsync(ct);
        return MapRun(run);
    }

    public async Task<Result> PostRunAsync(Guid tenantId, Guid runId, Guid userId, CancellationToken ct = default)
    {
        var run = await _context.PayrollRuns.FirstAsync(r => r.TenantId == tenantId && r.Id == runId, ct);
        if (run.Status != PayrollRunStatus.Approved)
            return Result.Failure("InvalidStatus", "Payroll run must be approved before posting.");

        var result = await _posting.PostPayrollRunAsync(runId, userId, ct);
        if (!result.IsSuccess) return result;

        var journalId = await _context.AccountingTransactions.AsNoTracking()
            .Where(t => t.SourceModule == PostingSource.Payroll && t.SourceDocumentId == runId)
            .Select(t => t.JournalEntryId)
            .FirstOrDefaultAsync(ct);

        run.MarkPosted(journalId);
        var slips = await _context.PayrollPayslips.Where(p => p.PayrollRunId == runId).ToListAsync(ct);
        foreach (var s in slips) s.MarkPaid();
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Payroll run {RunId} posted for tenant {TenantId}", runId, tenantId);
        return Result.Success();
    }

    public async Task<IReadOnlyList<PayrollRunDto>> GetRunsAsync(Guid tenantId, Guid companyId, CancellationToken ct = default)
        => await _context.PayrollRuns.AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.CompanyId == companyId)
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
            .Select(r => MapRun(r)).ToListAsync(ct);

    public async Task<IReadOnlyList<PayslipDto>> GetPayslipsAsync(Guid tenantId, Guid runId, CancellationToken ct = default)
    {
        var slips = await _context.PayrollPayslips.AsNoTracking().Where(p => p.TenantId == tenantId && p.PayrollRunId == runId).ToListAsync(ct);
        return await MapPayslips(slips, ct);
    }

    public async Task<IReadOnlyList<PayslipDto>> GetPayslipsForEmployeeAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default)
    {
        var slips = await _context.PayrollPayslips.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.EmployeeId == employeeId)
            .OrderByDescending(p => p.CreatedAt).Take(12).ToListAsync(ct);
        return await MapPayslips(slips, ct);
    }

    public async Task<SalaryStructureDto> UpsertSalaryStructureAsync(Guid tenantId, UpsertSalaryStructureDto dto, CancellationToken ct = default)
    {
        var existing = await _context.SalaryStructures
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.EmployeeId == dto.EmployeeId && s.IsActive, ct);
        if (existing is not null)
            existing.Deactivate();

        var structure = SalaryStructure.Create(tenantId, dto.EmployeeId, dto.BaseSalary, dto.HousingAllowance,
            dto.TransportAllowance, dto.OtherAllowances, dto.FixedDeductions);
        _context.SalaryStructures.Add(structure);
        await _context.SaveChangesAsync(ct);
        return new SalaryStructureDto(structure.Id, structure.EmployeeId, structure.BaseSalary, structure.HousingAllowance,
            structure.TransportAllowance, structure.OtherAllowances, structure.FixedDeductions, structure.GrossSalary, structure.Currency);
    }

    public async Task<SalaryStructureDto?> GetSalaryStructureAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default)
    {
        var s = await _context.SalaryStructures.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EmployeeId == employeeId && x.IsActive, ct);
        return s is null ? null : new SalaryStructureDto(s.Id, s.EmployeeId, s.BaseSalary, s.HousingAllowance,
            s.TransportAllowance, s.OtherAllowances, s.FixedDeductions, s.GrossSalary, s.Currency);
    }

    private async Task<decimal> CalcOvertimePayAsync(Guid tenantId, Guid employeeId, DateOnly from, DateOnly to, decimal baseSalary, CancellationToken ct)
    {
        var otMinutes = await _context.AttendanceRecords.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.EmployeeId == employeeId && a.WorkDate >= from && a.WorkDate <= to)
            .SumAsync(a => a.OvertimeMinutes, ct);
        var hourly = baseSalary / 160m;
        return Math.Round(hourly * (otMinutes / 60m) * 1.5m, 2);
    }

    private async Task<IReadOnlyList<PayslipDto>> MapPayslips(List<PayrollPayslip> slips, CancellationToken ct)
    {
        var ids = slips.Select(s => s.EmployeeId).Distinct().ToList();
        var names = await _context.Employees.AsNoTracking().Where(e => ids.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.NameAr ?? e.Name, ct);
        return slips.Select(s => new PayslipDto(
            s.Id, s.PayrollRunId, s.EmployeeId, names.GetValueOrDefault(s.EmployeeId, "Unknown"),
            s.GrossPay, s.Deductions, s.OvertimePay, s.Bonuses, s.NetPay, s.Status, s.Currency)).ToList();
    }

    private static PayrollRunDto MapRun(PayrollRun r) => new(
        r.Id, r.CompanyId, r.Year, r.Month, r.Status, r.TotalGross, r.TotalDeductions, r.TotalNet, r.PostedJournalId);
}
