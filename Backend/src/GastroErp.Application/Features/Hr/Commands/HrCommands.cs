using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Hr.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Hr.Commands;

public record CreateEmployeeCommand(Guid TenantId, CreateEmployeeDto Dto) : IRequest<Result<EmployeeDto>>;
public record UpdateEmployeeCommand(Guid TenantId, Guid Id, UpdateEmployeeDto Dto) : IRequest<Result<EmployeeDto>>;
public record TerminateEmployeeCommand(Guid TenantId, Guid Id, TerminateEmployeeDto Dto) : IRequest<Result>;
public record AddEmployeeContractCommand(Guid TenantId, CreateContractDto Dto) : IRequest<Result<ContractDto>>;
public record AddEmergencyContactCommand(Guid TenantId, CreateEmergencyContactDto Dto) : IRequest<Result<EmergencyContactDto>>;

public record CheckInCommand(Guid TenantId, CheckInDto Dto) : IRequest<Result<AttendanceDto>>;
public record CheckOutCommand(Guid TenantId, CheckOutDto Dto) : IRequest<Result<AttendanceDto>>;
public record StartBreakCommand(Guid TenantId, Guid AttendanceId) : IRequest<Result<AttendanceDto>>;
public record EndBreakCommand(Guid TenantId, Guid AttendanceId, int Minutes) : IRequest<Result<AttendanceDto>>;

public record CreateScheduleCommand(Guid TenantId, CreateScheduleDto Dto) : IRequest<Result<ScheduleEntryDto>>;
public record CreateWorkingShiftCommand(Guid TenantId, CreateWorkingShiftDto Dto) : IRequest<Result<WorkingShiftDto>>;
public record CreatePositionCommand(Guid TenantId, CreatePositionDto Dto) : IRequest<Result<PositionDto>>;

public record SubmitLeaveCommand(Guid TenantId, SubmitLeaveDto Dto) : IRequest<Result<LeaveRequestDto>>;
public record ApproveLeaveCommand(Guid TenantId, Guid ApproverId, ApproveLeaveDto Dto) : IRequest<Result<LeaveRequestDto>>;

public record UpsertSalaryStructureCommand(Guid TenantId, UpsertSalaryStructureDto Dto) : IRequest<Result<SalaryStructureDto>>;
public record CreatePayrollRunCommand(Guid TenantId, CreatePayrollRunDto Dto) : IRequest<Result<PayrollRunDto>>;
public record CalculatePayrollRunCommand(Guid TenantId, Guid RunId) : IRequest<Result<PayrollRunDto>>;
public record ApprovePayrollRunCommand(Guid TenantId, Guid RunId, Guid ApproverId) : IRequest<Result<PayrollRunDto>>;
public record PostPayrollRunCommand(Guid TenantId, Guid RunId, Guid UserId) : IRequest<Result>;

public record RecordPerformanceCommand(Guid TenantId, Guid? RecordedBy, CreatePerformanceDto Dto) : IRequest<Result<PerformanceDto>>;
public record ApplyApplicantCommand(Guid TenantId, CreateApplicantDto Dto) : IRequest<Result<ApplicantDto>>;
public record ScheduleInterviewCommand(Guid TenantId, ScheduleInterviewDto Dto) : IRequest<Result>;
public record HireApplicantCommand(Guid TenantId, HireApplicantDto Dto) : IRequest<Result<EmployeeDto>>;

public record CreateTrainingCourseCommand(Guid TenantId, CreateTrainingCourseDto Dto) : IRequest<Result<TrainingCourseDto>>;
public record EnrollTrainingCommand(Guid TenantId, EnrollTrainingDto Dto) : IRequest<Result>;
public record CompleteTrainingCommand(Guid TenantId, CompleteTrainingDto Dto) : IRequest<Result>;

public record UpdateSelfProfileCommand(Guid TenantId, Guid EmployeeId, UpdateEmployeeDto Dto) : IRequest<Result<EmployeeDto>>;
public record SubmitSelfLeaveCommand(Guid TenantId, Guid EmployeeId, SubmitLeaveDto Dto) : IRequest<Result<LeaveRequestDto>>;

public record SubmitHrWorkflowRequestCommand(Guid TenantId, SubmitHrWorkflowRequestDto Dto) : IRequest<Result<HrWorkflowRequestDto>>;
public record GetHrWorkflowRequestsQuery(Guid TenantId, Guid? EmployeeId = null, Domain.Enums.HrWorkflowRequestType? Type = null) : IRequest<Result<IReadOnlyList<HrWorkflowRequestDto>>>;
