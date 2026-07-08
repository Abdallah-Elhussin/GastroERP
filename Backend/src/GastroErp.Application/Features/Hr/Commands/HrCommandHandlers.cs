using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Hr.Commands;
using GastroErp.Application.Features.Hr.DTOs;
using GastroErp.Application.Features.Hr.Services;
using GastroErp.Application.Features.Workflow.Services;
using GastroErp.Domain.Workflow;
using MediatR;

namespace GastroErp.Application.Features.Hr.Commands;

public sealed class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<EmployeeDto>>
{
    private readonly IEmployeeManagementService _service;
    public CreateEmployeeCommandHandler(IEmployeeManagementService service) => _service = service;
    public async Task<Result<EmployeeDto>> Handle(CreateEmployeeCommand request, CancellationToken ct)
        => Result<EmployeeDto>.Success(await _service.CreateAsync(request.TenantId, request.Dto, ct));
}

public sealed class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result<EmployeeDto>>
{
    private readonly IEmployeeManagementService _service;
    public UpdateEmployeeCommandHandler(IEmployeeManagementService service) => _service = service;
    public async Task<Result<EmployeeDto>> Handle(UpdateEmployeeCommand request, CancellationToken ct)
        => Result<EmployeeDto>.Success(await _service.UpdateAsync(request.TenantId, request.Id, request.Dto, ct));
}

public sealed class TerminateEmployeeCommandHandler : IRequestHandler<TerminateEmployeeCommand, Result>
{
    private readonly IEmployeeManagementService _service;
    public TerminateEmployeeCommandHandler(IEmployeeManagementService service) => _service = service;
    public async Task<Result> Handle(TerminateEmployeeCommand request, CancellationToken ct)
    {
        await _service.TerminateAsync(request.TenantId, request.Id, request.Dto, ct);
        return Result.Success();
    }
}

public sealed class AddEmployeeContractCommandHandler : IRequestHandler<AddEmployeeContractCommand, Result<ContractDto>>
{
    private readonly IEmployeeManagementService _service;
    public AddEmployeeContractCommandHandler(IEmployeeManagementService service) => _service = service;
    public async Task<Result<ContractDto>> Handle(AddEmployeeContractCommand request, CancellationToken ct)
        => Result<ContractDto>.Success(await _service.AddContractAsync(request.TenantId, request.Dto, ct));
}

public sealed class AddEmergencyContactCommandHandler : IRequestHandler<AddEmergencyContactCommand, Result<EmergencyContactDto>>
{
    private readonly IEmployeeManagementService _service;
    public AddEmergencyContactCommandHandler(IEmployeeManagementService service) => _service = service;
    public async Task<Result<EmergencyContactDto>> Handle(AddEmergencyContactCommand request, CancellationToken ct)
        => Result<EmergencyContactDto>.Success(await _service.AddEmergencyContactAsync(request.TenantId, request.Dto, ct));
}

public sealed class CheckInCommandHandler : IRequestHandler<CheckInCommand, Result<AttendanceDto>>
{
    private readonly IAttendanceService _service;
    public CheckInCommandHandler(IAttendanceService service) => _service = service;
    public async Task<Result<AttendanceDto>> Handle(CheckInCommand request, CancellationToken ct)
        => Result<AttendanceDto>.Success(await _service.CheckInAsync(request.TenantId, request.Dto, ct));
}

public sealed class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, Result<AttendanceDto>>
{
    private readonly IAttendanceService _service;
    public CheckOutCommandHandler(IAttendanceService service) => _service = service;
    public async Task<Result<AttendanceDto>> Handle(CheckOutCommand request, CancellationToken ct)
        => Result<AttendanceDto>.Success(await _service.CheckOutAsync(request.TenantId, request.Dto, ct));
}

public sealed class StartBreakCommandHandler : IRequestHandler<StartBreakCommand, Result<AttendanceDto>>
{
    private readonly IAttendanceService _service;
    public StartBreakCommandHandler(IAttendanceService service) => _service = service;
    public async Task<Result<AttendanceDto>> Handle(StartBreakCommand request, CancellationToken ct)
        => Result<AttendanceDto>.Success(await _service.StartBreakAsync(request.TenantId, request.AttendanceId, ct));
}

public sealed class EndBreakCommandHandler : IRequestHandler<EndBreakCommand, Result<AttendanceDto>>
{
    private readonly IAttendanceService _service;
    public EndBreakCommandHandler(IAttendanceService service) => _service = service;
    public async Task<Result<AttendanceDto>> Handle(EndBreakCommand request, CancellationToken ct)
        => Result<AttendanceDto>.Success(await _service.EndBreakAsync(request.TenantId, request.AttendanceId, request.Minutes, ct));
}

public sealed class CreateScheduleCommandHandler : IRequestHandler<CreateScheduleCommand, Result<ScheduleEntryDto>>
{
    private readonly ISchedulingService _service;
    public CreateScheduleCommandHandler(ISchedulingService service) => _service = service;
    public async Task<Result<ScheduleEntryDto>> Handle(CreateScheduleCommand request, CancellationToken ct)
        => Result<ScheduleEntryDto>.Success(await _service.CreateScheduleAsync(request.TenantId, request.Dto, ct));
}

public sealed class CreateWorkingShiftCommandHandler : IRequestHandler<CreateWorkingShiftCommand, Result<WorkingShiftDto>>
{
    private readonly ISchedulingService _service;
    public CreateWorkingShiftCommandHandler(ISchedulingService service) => _service = service;
    public async Task<Result<WorkingShiftDto>> Handle(CreateWorkingShiftCommand request, CancellationToken ct)
        => Result<WorkingShiftDto>.Success(await _service.CreateWorkingShiftAsync(request.TenantId, request.Dto, ct));
}

public sealed class CreatePositionCommandHandler : IRequestHandler<CreatePositionCommand, Result<PositionDto>>
{
    private readonly ISchedulingService _service;
    public CreatePositionCommandHandler(ISchedulingService service) => _service = service;
    public async Task<Result<PositionDto>> Handle(CreatePositionCommand request, CancellationToken ct)
        => Result<PositionDto>.Success(await _service.CreatePositionAsync(request.TenantId, request.Dto, ct));
}

public sealed class SubmitLeaveCommandHandler : IRequestHandler<SubmitLeaveCommand, Result<LeaveRequestDto>>
{
    private readonly ILeaveManagementService _service;
    public SubmitLeaveCommandHandler(ILeaveManagementService service) => _service = service;
    public async Task<Result<LeaveRequestDto>> Handle(SubmitLeaveCommand request, CancellationToken ct)
        => Result<LeaveRequestDto>.Success(await _service.SubmitAsync(request.TenantId, request.Dto, ct));
}

public sealed class ApproveLeaveCommandHandler : IRequestHandler<ApproveLeaveCommand, Result<LeaveRequestDto>>
{
    private readonly IWorkflowIntegrationService _workflow;

    public ApproveLeaveCommandHandler(IWorkflowIntegrationService workflow) => _workflow = workflow;

    public async Task<Result<LeaveRequestDto>> Handle(ApproveLeaveCommand request, CancellationToken ct)
    {
        var instance = await _workflow.GetStatusByReferenceAsync(
            request.TenantId, WorkflowIntegrationReferenceTypes.LeaveRequest, request.Dto.LeaveRequestId, ct);
        if (instance is not null && instance.Status == Domain.Enums.WorkflowStatus.InProgress)
            return Result<LeaveRequestDto>.Failure("WorkflowRequired", "Leave approval must go through the workflow engine.");
        return Result<LeaveRequestDto>.Failure("WorkflowRequired", "No active workflow found. Use POST /api/v1/workflow/instances/approve.");
    }
}

public sealed class UpsertSalaryStructureCommandHandler : IRequestHandler<UpsertSalaryStructureCommand, Result<SalaryStructureDto>>
{
    private readonly IPayrollService _service;
    public UpsertSalaryStructureCommandHandler(IPayrollService service) => _service = service;
    public async Task<Result<SalaryStructureDto>> Handle(UpsertSalaryStructureCommand request, CancellationToken ct)
        => Result<SalaryStructureDto>.Success(await _service.UpsertSalaryStructureAsync(request.TenantId, request.Dto, ct));
}

public sealed class CreatePayrollRunCommandHandler : IRequestHandler<CreatePayrollRunCommand, Result<PayrollRunDto>>
{
    private readonly IPayrollService _service;
    public CreatePayrollRunCommandHandler(IPayrollService service) => _service = service;
    public async Task<Result<PayrollRunDto>> Handle(CreatePayrollRunCommand request, CancellationToken ct)
        => Result<PayrollRunDto>.Success(await _service.CreateRunAsync(request.TenantId, request.Dto, ct));
}

public sealed class CalculatePayrollRunCommandHandler : IRequestHandler<CalculatePayrollRunCommand, Result<PayrollRunDto>>
{
    private readonly IPayrollService _service;
    public CalculatePayrollRunCommandHandler(IPayrollService service) => _service = service;
    public async Task<Result<PayrollRunDto>> Handle(CalculatePayrollRunCommand request, CancellationToken ct)
        => Result<PayrollRunDto>.Success(await _service.CalculateRunAsync(request.TenantId, request.RunId, ct));
}

public sealed class ApprovePayrollRunCommandHandler : IRequestHandler<ApprovePayrollRunCommand, Result<PayrollRunDto>>
{
    private readonly IWorkflowIntegrationService _workflow;

    public ApprovePayrollRunCommandHandler(IWorkflowIntegrationService workflow) => _workflow = workflow;

    public async Task<Result<PayrollRunDto>> Handle(ApprovePayrollRunCommand request, CancellationToken ct)
    {
        var instance = await _workflow.GetStatusByReferenceAsync(
            request.TenantId, WorkflowIntegrationReferenceTypes.PayrollRun, request.RunId, ct);
        if (instance is not null && instance.Status == Domain.Enums.WorkflowStatus.InProgress)
            return Result<PayrollRunDto>.Failure("WorkflowRequired", "Payroll approval must go through the workflow engine.");
        return Result<PayrollRunDto>.Failure("WorkflowRequired", "No active workflow found. Use POST /api/v1/workflow/instances/approve.");
    }
}

public sealed class PostPayrollRunCommandHandler : IRequestHandler<PostPayrollRunCommand, Result>
{
    private readonly IPayrollService _service;
    public PostPayrollRunCommandHandler(IPayrollService service) => _service = service;
    public async Task<Result> Handle(PostPayrollRunCommand request, CancellationToken ct)
        => await _service.PostRunAsync(request.TenantId, request.RunId, request.UserId, ct);
}

public sealed class RecordPerformanceCommandHandler : IRequestHandler<RecordPerformanceCommand, Result<PerformanceDto>>
{
    private readonly IPerformanceManagementService _service;
    public RecordPerformanceCommandHandler(IPerformanceManagementService service) => _service = service;
    public async Task<Result<PerformanceDto>> Handle(RecordPerformanceCommand request, CancellationToken ct)
        => Result<PerformanceDto>.Success(await _service.RecordAsync(request.TenantId, request.RecordedBy, request.Dto, ct));
}

public sealed class ApplyApplicantCommandHandler : IRequestHandler<ApplyApplicantCommand, Result<ApplicantDto>>
{
    private readonly IRecruitmentService _service;
    public ApplyApplicantCommandHandler(IRecruitmentService service) => _service = service;
    public async Task<Result<ApplicantDto>> Handle(ApplyApplicantCommand request, CancellationToken ct)
        => Result<ApplicantDto>.Success(await _service.ApplyAsync(request.TenantId, request.Dto, ct));
}

public sealed class ScheduleInterviewCommandHandler : IRequestHandler<ScheduleInterviewCommand, Result>
{
    private readonly IRecruitmentService _service;
    public ScheduleInterviewCommandHandler(IRecruitmentService service) => _service = service;
    public async Task<Result> Handle(ScheduleInterviewCommand request, CancellationToken ct)
    {
        await _service.ScheduleInterviewAsync(request.TenantId, request.Dto, ct);
        return Result.Success();
    }
}

public sealed class HireApplicantCommandHandler : IRequestHandler<HireApplicantCommand, Result<EmployeeDto>>
{
    private readonly IRecruitmentService _service;
    public HireApplicantCommandHandler(IRecruitmentService service) => _service = service;
    public async Task<Result<EmployeeDto>> Handle(HireApplicantCommand request, CancellationToken ct)
        => Result<EmployeeDto>.Success(await _service.HireApplicantAsync(request.TenantId, request.Dto, ct));
}

public sealed class CreateTrainingCourseCommandHandler : IRequestHandler<CreateTrainingCourseCommand, Result<TrainingCourseDto>>
{
    private readonly ITrainingService _service;
    public CreateTrainingCourseCommandHandler(ITrainingService service) => _service = service;
    public async Task<Result<TrainingCourseDto>> Handle(CreateTrainingCourseCommand request, CancellationToken ct)
        => Result<TrainingCourseDto>.Success(await _service.CreateCourseAsync(request.TenantId, request.Dto, ct));
}

public sealed class EnrollTrainingCommandHandler : IRequestHandler<EnrollTrainingCommand, Result>
{
    private readonly ITrainingService _service;
    public EnrollTrainingCommandHandler(ITrainingService service) => _service = service;
    public async Task<Result> Handle(EnrollTrainingCommand request, CancellationToken ct)
    {
        await _service.EnrollAsync(request.TenantId, request.Dto, ct);
        return Result.Success();
    }
}

public sealed class CompleteTrainingCommandHandler : IRequestHandler<CompleteTrainingCommand, Result>
{
    private readonly ITrainingService _service;
    public CompleteTrainingCommandHandler(ITrainingService service) => _service = service;
    public async Task<Result> Handle(CompleteTrainingCommand request, CancellationToken ct)
    {
        await _service.CompleteAsync(request.TenantId, request.Dto, ct);
        return Result.Success();
    }
}

public sealed class UpdateSelfProfileCommandHandler : IRequestHandler<UpdateSelfProfileCommand, Result<EmployeeDto>>
{
    private readonly IHrSelfService _service;
    public UpdateSelfProfileCommandHandler(IHrSelfService service) => _service = service;
    public async Task<Result<EmployeeDto>> Handle(UpdateSelfProfileCommand request, CancellationToken ct)
        => Result<EmployeeDto>.Success(await _service.UpdateMyProfileAsync(request.TenantId, request.EmployeeId, request.Dto, ct));
}

public sealed class SubmitSelfLeaveCommandHandler : IRequestHandler<SubmitSelfLeaveCommand, Result<LeaveRequestDto>>
{
    private readonly IHrSelfService _service;
    public SubmitSelfLeaveCommandHandler(IHrSelfService service) => _service = service;
    public async Task<Result<LeaveRequestDto>> Handle(SubmitSelfLeaveCommand request, CancellationToken ct)
        => Result<LeaveRequestDto>.Success(await _service.SubmitMyLeaveAsync(request.TenantId, request.EmployeeId, request.Dto, ct));
}

public sealed class SubmitHrWorkflowRequestCommandHandler : IRequestHandler<SubmitHrWorkflowRequestCommand, Result<HrWorkflowRequestDto>>
{
    private readonly IHrWorkflowRequestService _service;
    public SubmitHrWorkflowRequestCommandHandler(IHrWorkflowRequestService service) => _service = service;
    public async Task<Result<HrWorkflowRequestDto>> Handle(SubmitHrWorkflowRequestCommand request, CancellationToken ct)
        => Result<HrWorkflowRequestDto>.Success(await _service.SubmitAsync(request.TenantId, request.Dto, ct));
}

public sealed class GetHrWorkflowRequestsQueryHandler : IRequestHandler<GetHrWorkflowRequestsQuery, Result<IReadOnlyList<HrWorkflowRequestDto>>>
{
    private readonly IHrWorkflowRequestService _service;
    public GetHrWorkflowRequestsQueryHandler(IHrWorkflowRequestService service) => _service = service;
    public async Task<Result<IReadOnlyList<HrWorkflowRequestDto>>> Handle(GetHrWorkflowRequestsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<HrWorkflowRequestDto>>.Success(
            await _service.ListAsync(request.TenantId, request.EmployeeId, request.Type, ct));
}
