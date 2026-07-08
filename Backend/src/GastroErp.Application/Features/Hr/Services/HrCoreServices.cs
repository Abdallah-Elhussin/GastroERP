using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Automation.Services;
using GastroErp.Application.Features.Hr.DTOs;
using GastroErp.Domain.Entities.HR;
using GastroErp.Domain.Entities.Organization;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Hr.Services;

public interface IEmployeeNumberGenerator
{
    Task<string> GenerateAsync(Guid tenantId, CancellationToken ct = default);
}

public interface IEmployeeManagementService
{
    Task<EmployeeDto> CreateAsync(Guid tenantId, CreateEmployeeDto dto, CancellationToken ct = default);
    Task<EmployeeDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeDto>> ListAsync(Guid tenantId, HrFilterDto filter, CancellationToken ct = default);
    Task<EmployeeDto> UpdateAsync(Guid tenantId, Guid id, UpdateEmployeeDto dto, CancellationToken ct = default);
    Task TerminateAsync(Guid tenantId, Guid id, TerminateEmployeeDto dto, CancellationToken ct = default);
    Task<ContractDto> AddContractAsync(Guid tenantId, CreateContractDto dto, CancellationToken ct = default);
    Task<EmergencyContactDto> AddEmergencyContactAsync(Guid tenantId, CreateEmergencyContactDto dto, CancellationToken ct = default);
}

public interface IAttendanceService
{
    Task<AttendanceDto> CheckInAsync(Guid tenantId, CheckInDto dto, CancellationToken ct = default);
    Task<AttendanceDto> CheckOutAsync(Guid tenantId, CheckOutDto dto, CancellationToken ct = default);
    Task<AttendanceDto> StartBreakAsync(Guid tenantId, Guid attendanceId, CancellationToken ct = default);
    Task<AttendanceDto> EndBreakAsync(Guid tenantId, Guid attendanceId, int minutes, CancellationToken ct = default);
    Task<IReadOnlyList<AttendanceDto>> GetRecordsAsync(Guid tenantId, HrFilterDto filter, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default);
}

public interface ISchedulingService
{
    Task<ScheduleEntryDto> CreateScheduleAsync(Guid tenantId, CreateScheduleDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ScheduleEntryDto>> GetSchedulesAsync(Guid tenantId, HrFilterDto filter, DateOnly from, DateOnly to, ScheduleRole? role = null, CancellationToken ct = default);
    Task<WorkingShiftDto> CreateWorkingShiftAsync(Guid tenantId, CreateWorkingShiftDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<WorkingShiftDto>> GetWorkingShiftsAsync(Guid tenantId, Guid? branchId, CancellationToken ct = default);
    Task<PositionDto> CreatePositionAsync(Guid tenantId, CreatePositionDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<PositionDto>> GetPositionsAsync(Guid tenantId, Guid companyId, CancellationToken ct = default);
}

public interface ILeaveManagementService
{
    Task<LeaveRequestDto> SubmitAsync(Guid tenantId, SubmitLeaveDto dto, CancellationToken ct = default);
    Task<LeaveRequestDto> ProcessApprovalAsync(Guid tenantId, Guid approverId, ApproveLeaveDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequestDto>> GetRequestsAsync(Guid tenantId, HrFilterDto filter, LeaveRequestStatus? status = null, CancellationToken ct = default);
    Task EnsureLeaveBalanceAsync(Guid tenantId, Guid employeeId, LeaveType type, decimal totalDays, int year, CancellationToken ct = default);
}

public interface IPerformanceManagementService
{
    Task<PerformanceDto> RecordAsync(Guid tenantId, Guid? recordedBy, CreatePerformanceDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<PerformanceDto>> GetRecordsAsync(Guid tenantId, Guid? employeeId, CancellationToken ct = default);
}

public interface IRecruitmentService
{
    Task<ApplicantDto> ApplyAsync(Guid tenantId, CreateApplicantDto dto, CancellationToken ct = default);
    Task ScheduleInterviewAsync(Guid tenantId, ScheduleInterviewDto dto, CancellationToken ct = default);
    Task<EmployeeDto> HireApplicantAsync(Guid tenantId, HireApplicantDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicantDto>> GetApplicantsAsync(Guid tenantId, Guid companyId, CancellationToken ct = default);
}

public interface ITrainingService
{
    Task<TrainingCourseDto> CreateCourseAsync(Guid tenantId, CreateTrainingCourseDto dto, CancellationToken ct = default);
    Task EnrollAsync(Guid tenantId, EnrollTrainingDto dto, CancellationToken ct = default);
    Task CompleteAsync(Guid tenantId, CompleteTrainingDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<TrainingCourseDto>> GetCoursesAsync(Guid tenantId, CancellationToken ct = default);
}

public interface IHrDashboardService
{
    Task<HrDashboardDto> GetDashboardAsync(Guid tenantId, Guid? branchId = null, CancellationToken ct = default);
}

public interface IHrSelfService
{
    Task<SelfServiceProfileDto> GetProfileAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default);
    Task<EmployeeDto> UpdateMyProfileAsync(Guid tenantId, Guid employeeId, UpdateEmployeeDto dto, CancellationToken ct = default);
    Task<LeaveRequestDto> SubmitMyLeaveAsync(Guid tenantId, Guid employeeId, SubmitLeaveDto dto, CancellationToken ct = default);
}

public interface IHrJobExecutor
{
    Task RunAttendanceSummaryAsync(Guid tenantId, CancellationToken ct = default);
    Task SyncAttendanceAnomaliesAsync(Guid tenantId, CancellationToken ct = default);
    Task RefreshLeaveBalancesAsync(Guid tenantId, CancellationToken ct = default);
    Task RunOvertimeSummaryAsync(Guid tenantId, CancellationToken ct = default);
    Task RunPayrollGenerationAsync(Guid tenantId, CancellationToken ct = default);
    Task RunPayrollPostingReminderAsync(Guid tenantId, CancellationToken ct = default);
    Task RunPerformanceRemindersAsync(Guid tenantId, CancellationToken ct = default);
    Task RunContractExpiryAlertsAsync(Guid tenantId, CancellationToken ct = default);
    Task RunProbationExpiryAlertsAsync(Guid tenantId, CancellationToken ct = default);
    Task RunCertificationExpiryAlertsAsync(Guid tenantId, CancellationToken ct = default);
}

public sealed class EmployeeNumberGenerator : IEmployeeNumberGenerator
{
    private readonly IApplicationDbContext _context;
    public EmployeeNumberGenerator(IApplicationDbContext context) => _context = context;

    public async Task<string> GenerateAsync(Guid tenantId, CancellationToken ct = default)
    {
        var count = await _context.Employees.CountAsync(e => e.TenantId == tenantId, ct);
        return $"EMP-{DateTime.UtcNow:yyyyMM}-{(count + 1):D4}";
    }
}

public sealed class EmployeeManagementService : IEmployeeManagementService
{
    private readonly IApplicationDbContext _context;
    private readonly IEmployeeNumberGenerator _numbers;

    public EmployeeManagementService(IApplicationDbContext context, IEmployeeNumberGenerator numbers)
        => (_context, _numbers) = (context, numbers);

    public async Task<EmployeeDto> CreateAsync(Guid tenantId, CreateEmployeeDto dto, CancellationToken ct = default)
    {
        var number = await _numbers.GenerateAsync(tenantId, ct);
        var employee = Employee.Hire(tenantId, dto.CompanyId, number, dto.Name,
            dto.BranchId, dto.DepartmentId, dto.PositionId, dto.UserId,
            dto.NameAr, dto.NationalId, dto.Email, dto.Phone, dto.HireDate);
        _context.Employees.Add(employee);
        _context.EmploymentHistoryEntries.Add(EmploymentHistoryEntry.Create(
            tenantId, employee.Id, "Hired", $"Employee hired as {number}", employee.HireDate ?? DateOnly.FromDateTime(DateTime.UtcNow)));
        await _context.SaveChangesAsync(ct);
        return MapEmployee(employee);
    }

    public async Task<EmployeeDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var e = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        return e is null ? null : MapEmployee(e);
    }

    public async Task<IReadOnlyList<EmployeeDto>> ListAsync(Guid tenantId, HrFilterDto filter, CancellationToken ct = default)
    {
        var q = _context.Employees.AsNoTracking().Where(e => e.TenantId == tenantId);
        if (filter.CompanyId.HasValue) q = q.Where(e => e.CompanyId == filter.CompanyId);
        if (filter.BranchId.HasValue) q = q.Where(e => e.BranchId == filter.BranchId);
        if (filter.DepartmentId.HasValue) q = q.Where(e => e.DepartmentId == filter.DepartmentId);
        var items = await q.OrderBy(e => e.EmployeeNumber)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(ct);
        return items.Select(MapEmployee).ToList();
    }

    public async Task<EmployeeDto> UpdateAsync(Guid tenantId, Guid id, UpdateEmployeeDto dto, CancellationToken ct = default)
    {
        var e = await _context.Employees.FirstAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        e.UpdateProfile(dto.Name, dto.NameAr, dto.Email, dto.Phone, dto.NationalId);
        e.AssignOrganization(dto.BranchId, dto.DepartmentId, dto.PositionId);
        await _context.SaveChangesAsync(ct);
        return MapEmployee(e);
    }

    public async Task TerminateAsync(Guid tenantId, Guid id, TerminateEmployeeDto dto, CancellationToken ct = default)
    {
        var e = await _context.Employees.FirstAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        e.Terminate(dto.Reason);
        _context.EmploymentHistoryEntries.Add(EmploymentHistoryEntry.Create(
            tenantId, id, "Terminated", dto.Reason, DateOnly.FromDateTime(DateTime.UtcNow)));
        await _context.SaveChangesAsync(ct);
    }

    public async Task<ContractDto> AddContractAsync(Guid tenantId, CreateContractDto dto, CancellationToken ct = default)
    {
        var contract = EmployeeContract.Create(tenantId, dto.EmployeeId, dto.ContractType, dto.StartDate, dto.BaseSalary, dto.EndDate);
        _context.EmployeeContracts.Add(contract);
        await _context.SaveChangesAsync(ct);
        return new ContractDto(contract.Id, contract.EmployeeId, contract.ContractType, contract.StartDate, contract.EndDate, contract.BaseSalary, contract.Currency, contract.IsActive);
    }

    public async Task<EmergencyContactDto> AddEmergencyContactAsync(Guid tenantId, CreateEmergencyContactDto dto, CancellationToken ct = default)
    {
        var c = EmployeeEmergencyContact.Create(tenantId, dto.EmployeeId, dto.Name, dto.Phone, dto.Relationship);
        _context.EmployeeEmergencyContacts.Add(c);
        await _context.SaveChangesAsync(ct);
        return new EmergencyContactDto(c.Id, c.EmployeeId, c.Name, c.Phone, c.Relationship);
    }

    private static EmployeeDto MapEmployee(Employee e) => new(
        e.Id, e.EmployeeNumber, e.Name, e.NameAr, e.Status,
        e.CompanyId, e.BranchId, e.DepartmentId, e.PositionId, e.UserId, e.Email, e.PhoneNumber, e.HireDate, e.CreatedAt);
}

public sealed class AttendanceService : IAttendanceService
{
    private readonly IApplicationDbContext _context;

    public AttendanceService(IApplicationDbContext context) => _context = context;

    public async Task<AttendanceDto> CheckInAsync(Guid tenantId, CheckInDto dto, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var existing = await _context.AttendanceRecords
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.EmployeeId == dto.EmployeeId && a.WorkDate == today, ct);
        if (existing is not null)
            return await MapAttendance(existing, ct);

        var record = AttendanceRecord.Create(tenantId, dto.EmployeeId, dto.BranchId, today, dto.DeviceId);
        var shift = await _context.WorkScheduleEntries.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.EmployeeId == dto.EmployeeId && s.ScheduleDate == today, ct);
        if (shift?.StartTime is not null)
        {
            var now = TimeOnly.FromDateTime(DateTime.UtcNow);
            if (now > shift.StartTime.Value.AddMinutes(15))
                record.MarkLate();
        }
        _context.AttendanceRecords.Add(record);
        await _context.SaveChangesAsync(ct);
        return await MapAttendance(record, ct);
    }

    public async Task<AttendanceDto> CheckOutAsync(Guid tenantId, CheckOutDto dto, CancellationToken ct = default)
    {
        var record = await _context.AttendanceRecords.FirstAsync(a => a.TenantId == tenantId && a.Id == dto.AttendanceId, ct);
        record.CheckOut(dto.OvertimeMinutes);
        await _context.SaveChangesAsync(ct);
        return await MapAttendance(record, ct);
    }

    public async Task<AttendanceDto> StartBreakAsync(Guid tenantId, Guid attendanceId, CancellationToken ct = default)
    {
        var record = await _context.AttendanceRecords.FirstAsync(a => a.TenantId == tenantId && a.Id == attendanceId, ct);
        record.StartBreak();
        await _context.SaveChangesAsync(ct);
        return await MapAttendance(record, ct);
    }

    public async Task<AttendanceDto> EndBreakAsync(Guid tenantId, Guid attendanceId, int minutes, CancellationToken ct = default)
    {
        var record = await _context.AttendanceRecords.FirstAsync(a => a.TenantId == tenantId && a.Id == attendanceId, ct);
        record.EndBreak(minutes);
        await _context.SaveChangesAsync(ct);
        return await MapAttendance(record, ct);
    }

    public async Task<IReadOnlyList<AttendanceDto>> GetRecordsAsync(
        Guid tenantId, HrFilterDto filter, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
    {
        var q = _context.AttendanceRecords.AsNoTracking().Where(a => a.TenantId == tenantId);
        if (filter.BranchId.HasValue) q = q.Where(a => a.BranchId == filter.BranchId);
        if (from.HasValue) q = q.Where(a => a.WorkDate >= from);
        if (to.HasValue) q = q.Where(a => a.WorkDate <= to);
        var items = await q.OrderByDescending(a => a.WorkDate).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(ct);
        var result = new List<AttendanceDto>();
        foreach (var item in items) result.Add(await MapAttendance(item, ct));
        return result;
    }

    private async Task<AttendanceDto> MapAttendance(AttendanceRecord r, CancellationToken ct)
    {
        var name = await _context.Employees.AsNoTracking().Where(e => e.Id == r.EmployeeId).Select(e => e.NameAr ?? e.Name).FirstOrDefaultAsync(ct) ?? "Unknown";
        return new AttendanceDto(r.Id, r.EmployeeId, name, r.BranchId, r.WorkDate, r.CheckInAt, r.CheckOutAt, r.Status, r.BreakMinutes, r.OvertimeMinutes, r.IsLate);
    }
}

public sealed class SchedulingService : ISchedulingService
{
    private readonly IApplicationDbContext _context;
    public SchedulingService(IApplicationDbContext context) => _context = context;

    public async Task<ScheduleEntryDto> CreateScheduleAsync(Guid tenantId, CreateScheduleDto dto, CancellationToken ct = default)
    {
        var entry = WorkScheduleEntry.Create(tenantId, dto.EmployeeId, dto.BranchId, dto.ScheduleDate, dto.Role, dto.WorkingShiftId, dto.StartTime, dto.EndTime, dto.Notes);
        _context.WorkScheduleEntries.Add(entry);
        await _context.SaveChangesAsync(ct);
        return await MapSchedule(entry, ct);
    }

    public async Task<IReadOnlyList<ScheduleEntryDto>> GetSchedulesAsync(
        Guid tenantId, HrFilterDto filter, DateOnly from, DateOnly to, ScheduleRole? role = null, CancellationToken ct = default)
    {
        var q = _context.WorkScheduleEntries.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.ScheduleDate >= from && s.ScheduleDate <= to);
        if (filter.BranchId.HasValue) q = q.Where(s => s.BranchId == filter.BranchId);
        if (role.HasValue) q = q.Where(s => s.Role == role);
        var items = await q.OrderBy(s => s.ScheduleDate).ToListAsync(ct);
        var result = new List<ScheduleEntryDto>();
        foreach (var item in items) result.Add(await MapSchedule(item, ct));
        return result;
    }

    public async Task<WorkingShiftDto> CreateWorkingShiftAsync(Guid tenantId, CreateWorkingShiftDto dto, CancellationToken ct = default)
    {
        var shift = new WorkingShift(tenantId, dto.BranchId, dto.Name, dto.StartTime, dto.EndTime, dto.NameAr);
        _context.WorkingShifts.Add(shift);
        await _context.SaveChangesAsync(ct);
        return new WorkingShiftDto(shift.Id, shift.BranchId, shift.Name, shift.NameAr, shift.StartTime, shift.EndTime, shift.IsActive);
    }

    public async Task<IReadOnlyList<WorkingShiftDto>> GetWorkingShiftsAsync(Guid tenantId, Guid? branchId, CancellationToken ct = default)
    {
        var q = _context.WorkingShifts.AsNoTracking().Where(s => s.TenantId == tenantId && s.IsActive);
        if (branchId.HasValue) q = q.Where(s => s.BranchId == branchId);
        return await q.Select(s => new WorkingShiftDto(s.Id, s.BranchId, s.Name, s.NameAr, s.StartTime, s.EndTime, s.IsActive)).ToListAsync(ct);
    }

    public async Task<PositionDto> CreatePositionAsync(Guid tenantId, CreatePositionDto dto, CancellationToken ct = default)
    {
        var pos = new EmployeePosition(tenantId, dto.CompanyId, dto.Name, dto.DepartmentId, dto.NameAr, dto.Description);
        _context.EmployeePositions.Add(pos);
        await _context.SaveChangesAsync(ct);
        return new PositionDto(pos.Id, pos.CompanyId, pos.DepartmentId, pos.Name, pos.NameAr, pos.IsActive);
    }

    public async Task<IReadOnlyList<PositionDto>> GetPositionsAsync(Guid tenantId, Guid companyId, CancellationToken ct = default)
        => await _context.EmployeePositions.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.CompanyId == companyId && p.IsActive)
            .Select(p => new PositionDto(p.Id, p.CompanyId, p.DepartmentId, p.Name, p.NameAr, p.IsActive))
            .ToListAsync(ct);

    private async Task<ScheduleEntryDto> MapSchedule(WorkScheduleEntry s, CancellationToken ct)
    {
        var name = await _context.Employees.AsNoTracking().Where(e => e.Id == s.EmployeeId).Select(e => e.NameAr ?? e.Name).FirstOrDefaultAsync(ct) ?? "Unknown";
        return new ScheduleEntryDto(s.Id, s.EmployeeId, name, s.BranchId, s.ScheduleDate, s.Role, s.WorkingShiftId, s.StartTime, s.EndTime);
    }
}

public sealed class LeaveManagementService : ILeaveManagementService
{
    private readonly IApplicationDbContext _context;
    public LeaveManagementService(IApplicationDbContext context) => _context = context;

    public async Task<LeaveRequestDto> SubmitAsync(Guid tenantId, SubmitLeaveDto dto, CancellationToken ct = default)
    {
        var days = dto.ToDate.DayNumber - dto.FromDate.DayNumber + 1;
        var req = LeaveRequest.Submit(tenantId, dto.EmployeeId, dto.LeaveType, dto.FromDate, dto.ToDate, days, dto.Reason);
        _context.LeaveRequests.Add(req);
        await _context.SaveChangesAsync(ct);
        return await MapLeave(req, ct);
    }

    public async Task<LeaveRequestDto> ProcessApprovalAsync(Guid tenantId, Guid approverId, ApproveLeaveDto dto, CancellationToken ct = default)
    {
        var req = await _context.LeaveRequests.FirstAsync(r => r.TenantId == tenantId && r.Id == dto.LeaveRequestId, ct);
        if (dto.Approve)
        {
            req.Approve(approverId);
            var balance = await _context.LeaveBalances.FirstOrDefaultAsync(
                b => b.TenantId == tenantId && b.EmployeeId == req.EmployeeId && b.LeaveType == req.LeaveType && b.Year == req.FromDate.Year, ct);
            if (balance is not null) balance.Use(req.Days);
            var emp = await _context.Employees.FirstAsync(e => e.Id == req.EmployeeId, ct);
            emp.MarkAsOnLeave();
        }
        else req.Reject(dto.RejectionReason ?? "Rejected");
        await _context.SaveChangesAsync(ct);
        return await MapLeave(req, ct);
    }

    public async Task<IReadOnlyList<LeaveRequestDto>> GetRequestsAsync(
        Guid tenantId, HrFilterDto filter, LeaveRequestStatus? status = null, CancellationToken ct = default)
    {
        var q = _context.LeaveRequests.AsNoTracking().Where(r => r.TenantId == tenantId);
        if (status.HasValue) q = q.Where(r => r.Status == status);
        var items = await q.OrderByDescending(r => r.CreatedAt).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(ct);
        var result = new List<LeaveRequestDto>();
        foreach (var item in items) result.Add(await MapLeave(item, ct));
        return result;
    }

    public async Task EnsureLeaveBalanceAsync(Guid tenantId, Guid employeeId, LeaveType type, decimal totalDays, int year, CancellationToken ct = default)
    {
        var exists = await _context.LeaveBalances.AnyAsync(b => b.TenantId == tenantId && b.EmployeeId == employeeId && b.LeaveType == type && b.Year == year, ct);
        if (!exists)
        {
            _context.LeaveBalances.Add(LeaveBalance.Create(tenantId, employeeId, type, totalDays, year));
            await _context.SaveChangesAsync(ct);
        }
    }

    private async Task<LeaveRequestDto> MapLeave(LeaveRequest r, CancellationToken ct)
    {
        var name = await _context.Employees.AsNoTracking().Where(e => e.Id == r.EmployeeId).Select(e => e.NameAr ?? e.Name).FirstOrDefaultAsync(ct) ?? "Unknown";
        return new LeaveRequestDto(r.Id, r.EmployeeId, name, r.LeaveType, r.FromDate, r.ToDate, r.Days, r.Status, r.Reason);
    }
}

public sealed class PerformanceManagementService : IPerformanceManagementService
{
    private readonly IApplicationDbContext _context;
    public PerformanceManagementService(IApplicationDbContext context) => _context = context;

    public async Task<PerformanceDto> RecordAsync(Guid tenantId, Guid? recordedBy, CreatePerformanceDto dto, CancellationToken ct = default)
    {
        var rec = PerformanceRecord.Create(tenantId, dto.EmployeeId, dto.RecordType, dto.Title, dto.Description, dto.RecordDate, dto.Score, recordedBy);
        _context.PerformanceRecords.Add(rec);
        await _context.SaveChangesAsync(ct);
        return await Map(rec, ct);
    }

    public async Task<IReadOnlyList<PerformanceDto>> GetRecordsAsync(Guid tenantId, Guid? employeeId, CancellationToken ct = default)
    {
        var q = _context.PerformanceRecords.AsNoTracking().Where(p => p.TenantId == tenantId);
        if (employeeId.HasValue) q = q.Where(p => p.EmployeeId == employeeId);
        var items = await q.OrderByDescending(p => p.RecordDate).ToListAsync(ct);
        var result = new List<PerformanceDto>();
        foreach (var item in items) result.Add(await Map(item, ct));
        return result;
    }

    private async Task<PerformanceDto> Map(PerformanceRecord p, CancellationToken ct)
    {
        var name = await _context.Employees.AsNoTracking().Where(e => e.Id == p.EmployeeId).Select(e => e.NameAr ?? e.Name).FirstOrDefaultAsync(ct) ?? "Unknown";
        return new PerformanceDto(p.Id, p.EmployeeId, name, p.RecordType, p.Title, p.Score, p.RecordDate);
    }
}

public sealed class RecruitmentService : IRecruitmentService
{
    private readonly IApplicationDbContext _context;
    private readonly IEmployeeManagementService _employees;

    public RecruitmentService(IApplicationDbContext context, IEmployeeManagementService employees)
        => (_context, _employees) = (context, employees);

    public async Task<ApplicantDto> ApplyAsync(Guid tenantId, CreateApplicantDto dto, CancellationToken ct = default)
    {
        var app = JobApplicant.Apply(tenantId, dto.CompanyId, dto.Name, dto.Email, dto.PositionId, dto.Phone, nameAr: dto.NameAr);
        _context.JobApplicants.Add(app);
        await _context.SaveChangesAsync(ct);
        return new ApplicantDto(app.Id, app.Name, app.NameAr, app.Email, app.Status, app.PositionId, app.HiredEmployeeId);
    }

    public async Task ScheduleInterviewAsync(Guid tenantId, ScheduleInterviewDto dto, CancellationToken ct = default)
    {
        var interview = InterviewRecord.Schedule(tenantId, dto.ApplicantId, dto.ScheduledAt, dto.InterviewerName);
        _context.InterviewRecords.Add(interview);
        var app = await _context.JobApplicants.FirstAsync(a => a.Id == dto.ApplicantId, ct);
        app.Advance(ApplicantStatus.Interview);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<EmployeeDto> HireApplicantAsync(Guid tenantId, HireApplicantDto dto, CancellationToken ct = default)
    {
        var employee = await _employees.CreateAsync(tenantId, dto.Employee, ct);
        var app = await _context.JobApplicants.FirstAsync(a => a.TenantId == tenantId && a.Id == dto.ApplicantId, ct);
        app.MarkHired(employee.Id);
        await _context.SaveChangesAsync(ct);
        return employee;
    }

    public async Task<IReadOnlyList<ApplicantDto>> GetApplicantsAsync(Guid tenantId, Guid companyId, CancellationToken ct = default)
        => await _context.JobApplicants.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.CompanyId == companyId)
            .Select(a => new ApplicantDto(a.Id, a.Name, a.NameAr, a.Email, a.Status, a.PositionId, a.HiredEmployeeId))
            .ToListAsync(ct);
}

public sealed class TrainingService : ITrainingService
{
    private readonly IApplicationDbContext _context;
    public TrainingService(IApplicationDbContext context) => _context = context;

    public async Task<TrainingCourseDto> CreateCourseAsync(Guid tenantId, CreateTrainingCourseDto dto, CancellationToken ct = default)
    {
        var c = TrainingCourse.Create(tenantId, dto.Title, dto.Description, dto.DurationHours, dto.TitleAr, dto.RequiresCertification);
        _context.TrainingCourses.Add(c);
        await _context.SaveChangesAsync(ct);
        return new TrainingCourseDto(c.Id, c.Title, c.TitleAr, c.DurationHours, c.RequiresCertification, c.IsActive);
    }

    public async Task EnrollAsync(Guid tenantId, EnrollTrainingDto dto, CancellationToken ct = default)
    {
        _context.EmployeeTrainingRecords.Add(EmployeeTrainingRecord.Enroll(tenantId, dto.EmployeeId, dto.CourseId));
        await _context.SaveChangesAsync(ct);
    }

    public async Task CompleteAsync(Guid tenantId, CompleteTrainingDto dto, CancellationToken ct = default)
    {
        var rec = await _context.EmployeeTrainingRecords.FirstAsync(r => r.TenantId == tenantId && r.Id == dto.RecordId, ct);
        rec.Complete(DateOnly.FromDateTime(DateTime.UtcNow), dto.Score, dto.CertificationExpiry);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TrainingCourseDto>> GetCoursesAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.TrainingCourses.AsNoTracking().Where(c => c.TenantId == tenantId && c.IsActive)
            .Select(c => new TrainingCourseDto(c.Id, c.Title, c.TitleAr, c.DurationHours, c.RequiresCertification, c.IsActive)).ToListAsync(ct);
}

public sealed class HrDashboardService : IHrDashboardService
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public HrDashboardService(IApplicationDbContext context, ICacheService cache)
        => (_context, _cache) = (context, cache);

    public async Task<HrDashboardDto> GetDashboardAsync(Guid tenantId, Guid? branchId = null, CancellationToken ct = default)
    {
        var cacheKey = $"hr:dashboard:{tenantId}:{branchId}";
        var cached = await _cache.GetAsync<HrDashboardDto>(cacheKey, ct);
        if (cached is not null) return cached;

        var empQ = _context.Employees.AsNoTracking().Where(e => e.TenantId == tenantId);
        if (branchId.HasValue) empQ = empQ.Where(e => e.BranchId == branchId);
        var active = await empQ.CountAsync(e => e.Status == EmploymentStatus.Active, ct);
        var onLeave = await empQ.CountAsync(e => e.Status == EmploymentStatus.OnLeave, ct);
        var pendingLeave = await _context.LeaveRequests.CountAsync(r => r.TenantId == tenantId && r.Status == LeaveRequestStatus.Pending, ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var attQ = _context.AttendanceRecords.AsNoTracking().Where(a => a.TenantId == tenantId && a.WorkDate == today);
        if (branchId.HasValue) attQ = attQ.Where(a => a.BranchId == branchId);
        var present = await attQ.CountAsync(a => a.Status != AttendanceStatus.Absent, ct);
        var absent = await attQ.CountAsync(a => a.Status == AttendanceStatus.Absent, ct);
        var openPayroll = await _context.PayrollRuns.CountAsync(p => p.TenantId == tenantId && p.Status != PayrollRunStatus.Posted && p.Status != PayrollRunStatus.Cancelled, ct);
        var dto = new HrDashboardDto(active, onLeave, pendingLeave, present, absent, openPayroll, DateTimeOffset.UtcNow);
        await _cache.SetAsync(cacheKey, dto, CacheDuration, ct);
        return dto;
    }
}

public sealed class HrSelfServiceImpl : IHrSelfService
{
    private readonly IEmployeeManagementService _employees;
    private readonly ILeaveManagementService _leave;
    private readonly IAttendanceService _attendance;
    private readonly IPayrollService _payroll;
    private readonly IApplicationDbContext _context;

    public HrSelfServiceImpl(
        IEmployeeManagementService employees, ILeaveManagementService leave,
        IAttendanceService attendance, IPayrollService payroll, IApplicationDbContext context)
        => (_employees, _leave, _attendance, _payroll, _context) = (employees, leave, attendance, payroll, context);

    public async Task<SelfServiceProfileDto> GetProfileAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default)
    {
        var emp = await _employees.GetByIdAsync(tenantId, employeeId, ct) ?? throw new InvalidOperationException("Employee not found.");
        var payslips = await _payroll.GetPayslipsForEmployeeAsync(tenantId, employeeId, ct);
        var leaves = await _leave.GetRequestsAsync(tenantId, new HrFilterDto(PageSize: 20), null, ct);
        var attendance = await _attendance.GetRecordsAsync(tenantId, new HrFilterDto(PageSize: 30), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)), null, ct);
        return new SelfServiceProfileDto(emp, payslips, leaves.Where(l => l.EmployeeId == employeeId).ToList(), attendance.Where(a => a.EmployeeId == employeeId).ToList());
    }

    public Task<EmployeeDto> UpdateMyProfileAsync(Guid tenantId, Guid employeeId, UpdateEmployeeDto dto, CancellationToken ct)
        => _employees.UpdateAsync(tenantId, employeeId, dto, ct);

    public Task<LeaveRequestDto> SubmitMyLeaveAsync(Guid tenantId, Guid employeeId, SubmitLeaveDto dto, CancellationToken ct)
        => _leave.SubmitAsync(tenantId, dto with { EmployeeId = employeeId }, ct);
}

public sealed class HrJobExecutor : IHrJobExecutor
{
    private readonly IApplicationDbContext _context;
    private readonly ILeaveManagementService _leave;
    private readonly IPayrollService _payroll;
    private readonly INotificationOrchestrator _notifications;

    public HrJobExecutor(
        IApplicationDbContext context, ILeaveManagementService leave,
        IPayrollService payroll, INotificationOrchestrator notifications)
        => (_context, _leave, _payroll, _notifications) = (context, leave, payroll, notifications);

    public async Task RunAttendanceSummaryAsync(Guid tenantId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var present = await _context.AttendanceRecords.AsNoTracking()
            .CountAsync(a => a.TenantId == tenantId && a.WorkDate == today && a.Status != AttendanceStatus.Absent, ct);
        var absent = await _context.AttendanceRecords.AsNoTracking()
            .CountAsync(a => a.TenantId == tenantId && a.WorkDate == today && a.Status == AttendanceStatus.Absent, ct);
        await _notifications.SendAsync(tenantId, new Features.Automation.DTOs.SendNotificationDto(
            "Attendance Summary", $"Today: {present} present, {absent} absent.",
            NotificationType.System, NotificationChannel.InApp), ct);
    }

    public async Task SyncAttendanceAnomaliesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var scheduled = await _context.WorkScheduleEntries.AsNoTracking()
            .Where(s => s.TenantId == tenantId && s.ScheduleDate == today).Select(s => s.EmployeeId).ToListAsync(ct);
        foreach (var empId in scheduled)
        {
            var hasAtt = await _context.AttendanceRecords.AnyAsync(a => a.TenantId == tenantId && a.EmployeeId == empId && a.WorkDate == today, ct);
            if (!hasAtt)
            {
                var emp = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == empId, ct);
                if (emp?.BranchId is null) continue;
                var absent = AttendanceRecord.Create(tenantId, empId, emp.BranchId.Value, today);
                absent.MarkAbsent();
                _context.AttendanceRecords.Add(absent);
            }
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task RefreshLeaveBalancesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var employees = await _context.Employees.AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.Status == EmploymentStatus.Active).Select(e => e.Id).ToListAsync(ct);
        foreach (var id in employees)
        {
            await _leave.EnsureLeaveBalanceAsync(tenantId, id, LeaveType.Annual, 21, year, ct);
            await _leave.EnsureLeaveBalanceAsync(tenantId, id, LeaveType.Sick, 10, year, ct);
        }
    }

    public async Task RunOvertimeSummaryAsync(Guid tenantId, CancellationToken ct = default)
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var otMinutes = await _context.AttendanceRecords.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.WorkDate >= from)
            .SumAsync(a => a.OvertimeMinutes, ct);
        if (otMinutes <= 0) return;
        await _notifications.SendAsync(tenantId, new Features.Automation.DTOs.SendNotificationDto(
            "Overtime Summary", $"Weekly overtime: {otMinutes / 60.0:N1} hours.",
            NotificationType.System, NotificationChannel.InApp), ct);
    }

    public async Task RunPayrollGenerationAsync(Guid tenantId, CancellationToken ct = default)
    {
        var companyId = await _context.Companies.AsNoTracking()
            .Where(c => c.TenantId == tenantId).Select(c => c.Id).FirstOrDefaultAsync(ct);
        if (companyId == Guid.Empty) return;

        var now = DateTime.UtcNow;
        var exists = await _context.PayrollRuns.AnyAsync(
            r => r.TenantId == tenantId && r.CompanyId == companyId && r.Year == now.Year && r.Month == now.Month, ct);
        if (exists) return;

        var run = await _payroll.CreateRunAsync(tenantId, new CreatePayrollRunDto(companyId, now.Year, now.Month), ct);
        await _payroll.CalculateRunAsync(tenantId, run.Id, ct);
    }

    public async Task RunPayrollPostingReminderAsync(Guid tenantId, CancellationToken ct = default)
    {
        var pending = await _context.PayrollRuns.AsNoTracking()
            .CountAsync(r => r.TenantId == tenantId && r.Status == PayrollRunStatus.Approved, ct);
        if (pending == 0) return;
        await _notifications.SendAsync(tenantId, new Features.Automation.DTOs.SendNotificationDto(
            "Payroll Posting", $"{pending} approved payroll run(s) awaiting finance posting.",
            NotificationType.HrPayrollPosted, NotificationChannel.InApp), ct);
    }

    public async Task RunPerformanceRemindersAsync(Guid tenantId, CancellationToken ct = default)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
        var employees = await _context.Employees.AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.Status == EmploymentStatus.Active).Select(e => e.Id).ToListAsync(ct);
        var evaluated = await _context.PerformanceRecords.AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.RecordDate >= cutoff)
            .Select(p => p.EmployeeId).Distinct().ToListAsync(ct);
        var missing = employees.Count - evaluated.Count;
        if (missing <= 0) return;
        await _notifications.SendAsync(tenantId, new Features.Automation.DTOs.SendNotificationDto(
            "Performance Review", $"{missing} employee(s) without evaluation in the last 90 days.",
            NotificationType.System, NotificationChannel.InApp), ct);
    }

    public async Task RunContractExpiryAlertsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var threshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiring = await _context.EmployeeContracts.AsNoTracking()
            .CountAsync(c => c.TenantId == tenantId && c.IsActive && c.EndDate != null && c.EndDate <= threshold && c.EndDate >= today, ct);
        if (expiring == 0) return;
        await _notifications.SendAsync(tenantId, new Features.Automation.DTOs.SendNotificationDto(
            "Contract Expiry", $"{expiring} contract(s) expiring within 30 days.",
            NotificationType.HrContractExpiry, NotificationChannel.InApp), ct);
    }

    public async Task RunProbationExpiryAlertsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var threshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiring = await _context.EmployeeContracts.AsNoTracking()
            .CountAsync(c => c.TenantId == tenantId && c.IsActive && c.ContractType == ContractType.Probation
                && c.EndDate != null && c.EndDate <= threshold && c.EndDate >= today, ct);
        if (expiring == 0) return;
        await _notifications.SendAsync(tenantId, new Features.Automation.DTOs.SendNotificationDto(
            "Probation Expiry", $"{expiring} probation contract(s) ending within 14 days.",
            NotificationType.HrContractExpiry, NotificationChannel.InApp), ct);
    }

    public async Task RunCertificationExpiryAlertsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var threshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiring = await _context.EmployeeTrainingRecords.AsNoTracking()
            .CountAsync(t => t.TenantId == tenantId && t.CertificationExpiry != null
                && t.CertificationExpiry <= threshold && t.CertificationExpiry >= today, ct);
        if (expiring == 0) return;
        await _notifications.SendAsync(tenantId, new Features.Automation.DTOs.SendNotificationDto(
            "Certification Expiry", $"{expiring} training certification(s) expiring within 30 days.",
            NotificationType.HrCertificationExpired, NotificationChannel.InApp), ct);
    }
}
