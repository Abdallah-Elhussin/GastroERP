using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Hr.DTOs;
using GastroErp.Application.Features.Hr.Queries;
using GastroErp.Application.Features.Hr.Services;
using MediatR;

namespace GastroErp.Application.Features.Hr.Queries;

public sealed class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, Result<IReadOnlyList<EmployeeDto>>>
{
    private readonly IEmployeeManagementService _service;
    public GetEmployeesQueryHandler(IEmployeeManagementService service) => _service = service;
    public async Task<Result<IReadOnlyList<EmployeeDto>>> Handle(GetEmployeesQuery request, CancellationToken ct)
        => Result<IReadOnlyList<EmployeeDto>>.Success(await _service.ListAsync(request.TenantId, request.Filter, ct));
}

public sealed class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>
{
    private readonly IEmployeeManagementService _service;
    public GetEmployeeByIdQueryHandler(IEmployeeManagementService service) => _service = service;
    public async Task<Result<EmployeeDto>> Handle(GetEmployeeByIdQuery request, CancellationToken ct)
    {
        var employee = await _service.GetByIdAsync(request.TenantId, request.Id, ct);
        return employee is null
            ? Result<EmployeeDto>.Failure("NotFound", "Employee not found.")
            : Result<EmployeeDto>.Success(employee);
    }
}

public sealed class GetAttendanceQueryHandler : IRequestHandler<GetAttendanceQuery, Result<IReadOnlyList<AttendanceDto>>>
{
    private readonly IAttendanceService _service;
    public GetAttendanceQueryHandler(IAttendanceService service) => _service = service;
    public async Task<Result<IReadOnlyList<AttendanceDto>>> Handle(GetAttendanceQuery request, CancellationToken ct)
        => Result<IReadOnlyList<AttendanceDto>>.Success(await _service.GetRecordsAsync(request.TenantId, request.Filter, request.From, request.To, ct));
}

public sealed class GetSchedulesQueryHandler : IRequestHandler<GetSchedulesQuery, Result<IReadOnlyList<ScheduleEntryDto>>>
{
    private readonly ISchedulingService _service;
    public GetSchedulesQueryHandler(ISchedulingService service) => _service = service;
    public async Task<Result<IReadOnlyList<ScheduleEntryDto>>> Handle(GetSchedulesQuery request, CancellationToken ct)
        => Result<IReadOnlyList<ScheduleEntryDto>>.Success(await _service.GetSchedulesAsync(request.TenantId, request.Filter, request.From, request.To, request.Role, ct));
}

public sealed class GetWorkingShiftsQueryHandler : IRequestHandler<GetWorkingShiftsQuery, Result<IReadOnlyList<WorkingShiftDto>>>
{
    private readonly ISchedulingService _service;
    public GetWorkingShiftsQueryHandler(ISchedulingService service) => _service = service;
    public async Task<Result<IReadOnlyList<WorkingShiftDto>>> Handle(GetWorkingShiftsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<WorkingShiftDto>>.Success(await _service.GetWorkingShiftsAsync(request.TenantId, request.BranchId, ct));
}

public sealed class GetPositionsQueryHandler : IRequestHandler<GetPositionsQuery, Result<IReadOnlyList<PositionDto>>>
{
    private readonly ISchedulingService _service;
    public GetPositionsQueryHandler(ISchedulingService service) => _service = service;
    public async Task<Result<IReadOnlyList<PositionDto>>> Handle(GetPositionsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<PositionDto>>.Success(await _service.GetPositionsAsync(request.TenantId, request.CompanyId, ct));
}

public sealed class GetLeaveRequestsQueryHandler : IRequestHandler<GetLeaveRequestsQuery, Result<IReadOnlyList<LeaveRequestDto>>>
{
    private readonly ILeaveManagementService _service;
    public GetLeaveRequestsQueryHandler(ILeaveManagementService service) => _service = service;
    public async Task<Result<IReadOnlyList<LeaveRequestDto>>> Handle(GetLeaveRequestsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<LeaveRequestDto>>.Success(await _service.GetRequestsAsync(request.TenantId, request.Filter, request.Status, ct));
}

public sealed class GetPayrollRunsQueryHandler : IRequestHandler<GetPayrollRunsQuery, Result<IReadOnlyList<PayrollRunDto>>>
{
    private readonly IPayrollService _service;
    public GetPayrollRunsQueryHandler(IPayrollService service) => _service = service;
    public async Task<Result<IReadOnlyList<PayrollRunDto>>> Handle(GetPayrollRunsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<PayrollRunDto>>.Success(await _service.GetRunsAsync(request.TenantId, request.CompanyId, ct));
}

public sealed class GetPayslipsQueryHandler : IRequestHandler<GetPayslipsQuery, Result<IReadOnlyList<PayslipDto>>>
{
    private readonly IPayrollService _service;
    public GetPayslipsQueryHandler(IPayrollService service) => _service = service;
    public async Task<Result<IReadOnlyList<PayslipDto>>> Handle(GetPayslipsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<PayslipDto>>.Success(await _service.GetPayslipsAsync(request.TenantId, request.RunId, ct));
}

public sealed class GetSalaryStructureQueryHandler : IRequestHandler<GetSalaryStructureQuery, Result<SalaryStructureDto>>
{
    private readonly IPayrollService _service;
    public GetSalaryStructureQueryHandler(IPayrollService service) => _service = service;
    public async Task<Result<SalaryStructureDto>> Handle(GetSalaryStructureQuery request, CancellationToken ct)
    {
        var structure = await _service.GetSalaryStructureAsync(request.TenantId, request.EmployeeId, ct);
        return structure is null
            ? Result<SalaryStructureDto>.Failure("NotFound", "Salary structure not found.")
            : Result<SalaryStructureDto>.Success(structure);
    }
}

public sealed class GetPerformanceRecordsQueryHandler : IRequestHandler<GetPerformanceRecordsQuery, Result<IReadOnlyList<PerformanceDto>>>
{
    private readonly IPerformanceManagementService _service;
    public GetPerformanceRecordsQueryHandler(IPerformanceManagementService service) => _service = service;
    public async Task<Result<IReadOnlyList<PerformanceDto>>> Handle(GetPerformanceRecordsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<PerformanceDto>>.Success(await _service.GetRecordsAsync(request.TenantId, request.EmployeeId, ct));
}

public sealed class GetApplicantsQueryHandler : IRequestHandler<GetApplicantsQuery, Result<IReadOnlyList<ApplicantDto>>>
{
    private readonly IRecruitmentService _service;
    public GetApplicantsQueryHandler(IRecruitmentService service) => _service = service;
    public async Task<Result<IReadOnlyList<ApplicantDto>>> Handle(GetApplicantsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<ApplicantDto>>.Success(await _service.GetApplicantsAsync(request.TenantId, request.CompanyId, ct));
}

public sealed class GetTrainingCoursesQueryHandler : IRequestHandler<GetTrainingCoursesQuery, Result<IReadOnlyList<TrainingCourseDto>>>
{
    private readonly ITrainingService _service;
    public GetTrainingCoursesQueryHandler(ITrainingService service) => _service = service;
    public async Task<Result<IReadOnlyList<TrainingCourseDto>>> Handle(GetTrainingCoursesQuery request, CancellationToken ct)
        => Result<IReadOnlyList<TrainingCourseDto>>.Success(await _service.GetCoursesAsync(request.TenantId, ct));
}

public sealed class GetHrDashboardQueryHandler : IRequestHandler<GetHrDashboardQuery, Result<HrDashboardDto>>
{
    private readonly IHrDashboardService _service;
    public GetHrDashboardQueryHandler(IHrDashboardService service) => _service = service;
    public async Task<Result<HrDashboardDto>> Handle(GetHrDashboardQuery request, CancellationToken ct)
        => Result<HrDashboardDto>.Success(await _service.GetDashboardAsync(request.TenantId, request.BranchId, ct));
}

public sealed class GetSelfServiceProfileQueryHandler : IRequestHandler<GetSelfServiceProfileQuery, Result<SelfServiceProfileDto>>
{
    private readonly IHrSelfService _service;
    public GetSelfServiceProfileQueryHandler(IHrSelfService service) => _service = service;
    public async Task<Result<SelfServiceProfileDto>> Handle(GetSelfServiceProfileQuery request, CancellationToken ct)
        => Result<SelfServiceProfileDto>.Success(await _service.GetProfileAsync(request.TenantId, request.EmployeeId, ct));
}
