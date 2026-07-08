using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Hr.DTOs;
using GastroErp.Domain.Enums;
using MediatR;

namespace GastroErp.Application.Features.Hr.Queries;

public record GetEmployeesQuery(Guid TenantId, HrFilterDto Filter) : IRequest<Result<IReadOnlyList<EmployeeDto>>>;
public record GetEmployeeByIdQuery(Guid TenantId, Guid Id) : IRequest<Result<EmployeeDto>>;

public record GetAttendanceQuery(Guid TenantId, HrFilterDto Filter, DateOnly? From, DateOnly? To) : IRequest<Result<IReadOnlyList<AttendanceDto>>>;

public record GetSchedulesQuery(Guid TenantId, HrFilterDto Filter, DateOnly From, DateOnly To, ScheduleRole? Role) : IRequest<Result<IReadOnlyList<ScheduleEntryDto>>>;
public record GetWorkingShiftsQuery(Guid TenantId, Guid? BranchId) : IRequest<Result<IReadOnlyList<WorkingShiftDto>>>;
public record GetPositionsQuery(Guid TenantId, Guid CompanyId) : IRequest<Result<IReadOnlyList<PositionDto>>>;

public record GetLeaveRequestsQuery(Guid TenantId, HrFilterDto Filter, LeaveRequestStatus? Status) : IRequest<Result<IReadOnlyList<LeaveRequestDto>>>;

public record GetPayrollRunsQuery(Guid TenantId, Guid CompanyId) : IRequest<Result<IReadOnlyList<PayrollRunDto>>>;
public record GetPayslipsQuery(Guid TenantId, Guid RunId) : IRequest<Result<IReadOnlyList<PayslipDto>>>;
public record GetSalaryStructureQuery(Guid TenantId, Guid EmployeeId) : IRequest<Result<SalaryStructureDto>>;

public record GetPerformanceRecordsQuery(Guid TenantId, Guid? EmployeeId) : IRequest<Result<IReadOnlyList<PerformanceDto>>>;
public record GetApplicantsQuery(Guid TenantId, Guid CompanyId) : IRequest<Result<IReadOnlyList<ApplicantDto>>>;
public record GetTrainingCoursesQuery(Guid TenantId) : IRequest<Result<IReadOnlyList<TrainingCourseDto>>>;

public record GetHrDashboardQuery(Guid TenantId, Guid? BranchId) : IRequest<Result<HrDashboardDto>>;
public record GetSelfServiceProfileQuery(Guid TenantId, Guid EmployeeId) : IRequest<Result<SelfServiceProfileDto>>;
