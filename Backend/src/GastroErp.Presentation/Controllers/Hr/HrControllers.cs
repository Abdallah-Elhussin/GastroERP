using Asp.Versioning;
using GastroErp.Application.Features.Hr.Commands;
using GastroErp.Application.Features.Hr.DTOs;
using GastroErp.Application.Features.Hr.Queries;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Hr;

[ApiVersion("1.0")]
[Tags("HR — Employees")]
public class EmployeesController : BaseApiController
{
    [HttpGet(ApiRoutes.Hr.Employees)]
    [HasPermission(Permissions.Hr.Employee.View)]
    public async Task<IActionResult> GetEmployees([FromQuery] HrFilterDto filter)
        => HandleResult(await Mediator.Send(new GetEmployeesQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Hr.Employees}/{{id:guid}}")]
    [HasPermission(Permissions.Hr.Employee.View)]
    public async Task<IActionResult> GetEmployee(Guid id)
        => HandleResult(await Mediator.Send(new GetEmployeeByIdQuery(TenantId, id)));

    [HttpPost(ApiRoutes.Hr.Employees)]
    [HasPermission(Permissions.Hr.Employee.Create)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto dto)
        => HandleResult(await Mediator.Send(new CreateEmployeeCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Hr.Employees}/{{id:guid}}")]
    [HasPermission(Permissions.Hr.Employee.Update)]
    public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeDto dto)
        => HandleResult(await Mediator.Send(new UpdateEmployeeCommand(TenantId, id, dto)));

    [HttpPost($"{ApiRoutes.Hr.Employees}/{{id:guid}}/terminate")]
    [HasPermission(Permissions.Hr.Employee.Update)]
    public async Task<IActionResult> TerminateEmployee(Guid id, [FromBody] TerminateEmployeeDto dto)
        => HandleResult(await Mediator.Send(new TerminateEmployeeCommand(TenantId, id, dto)));

    [HttpPost($"{ApiRoutes.Hr.Employees}/contracts")]
    [HasPermission(Permissions.Hr.Employee.Update)]
    public async Task<IActionResult> AddContract([FromBody] CreateContractDto dto)
        => HandleResult(await Mediator.Send(new AddEmployeeContractCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Hr.Employees}/emergency-contacts")]
    [HasPermission(Permissions.Hr.Employee.Update)]
    public async Task<IActionResult> AddEmergencyContact([FromBody] CreateEmergencyContactDto dto)
        => HandleResult(await Mediator.Send(new AddEmergencyContactCommand(TenantId, dto)));
}

[ApiVersion("1.0")]
[Tags("HR — Attendance")]
public class AttendanceController : BaseApiController
{
    [HttpGet(ApiRoutes.Hr.Attendance)]
    [HasPermission(Permissions.Hr.Attendance.View)]
    public async Task<IActionResult> GetAttendance([FromQuery] HrFilterDto filter, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
        => HandleResult(await Mediator.Send(new GetAttendanceQuery(TenantId, filter, from, to)));

    [HttpPost($"{ApiRoutes.Hr.Attendance}/check-in")]
    [HasPermission(Permissions.Hr.Attendance.Manage)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInDto dto)
        => HandleResult(await Mediator.Send(new CheckInCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Hr.Attendance}/check-out")]
    [HasPermission(Permissions.Hr.Attendance.Manage)]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutDto dto)
        => HandleResult(await Mediator.Send(new CheckOutCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Hr.Attendance}/{{id:guid}}/break/start")]
    [HasPermission(Permissions.Hr.Attendance.Manage)]
    public async Task<IActionResult> StartBreak(Guid id)
        => HandleResult(await Mediator.Send(new StartBreakCommand(TenantId, id)));

    [HttpPost($"{ApiRoutes.Hr.Attendance}/{{id:guid}}/break/end")]
    [HasPermission(Permissions.Hr.Attendance.Manage)]
    public async Task<IActionResult> EndBreak(Guid id, [FromQuery] int minutes = 0)
        => HandleResult(await Mediator.Send(new EndBreakCommand(TenantId, id, minutes)));
}

[ApiVersion("1.0")]
[Tags("HR — Scheduling")]
public class SchedulingController : BaseApiController
{
    [HttpGet(ApiRoutes.Hr.Schedules)]
    [HasPermission(Permissions.Hr.Schedule.View)]
    public async Task<IActionResult> GetSchedules([FromQuery] HrFilterDto filter, [FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] ScheduleRole? role)
        => HandleResult(await Mediator.Send(new GetSchedulesQuery(TenantId, filter, from, to, role)));

    [HttpPost(ApiRoutes.Hr.Schedules)]
    [HasPermission(Permissions.Hr.Schedule.Manage)]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleDto dto)
        => HandleResult(await Mediator.Send(new CreateScheduleCommand(TenantId, dto)));

    [HttpGet(ApiRoutes.Hr.Shifts)]
    [HasPermission(Permissions.Hr.Schedule.View)]
    public async Task<IActionResult> GetShifts([FromQuery] Guid? branchId)
        => HandleResult(await Mediator.Send(new GetWorkingShiftsQuery(TenantId, branchId)));

    [HttpPost(ApiRoutes.Hr.Shifts)]
    [HasPermission(Permissions.Hr.Schedule.Manage)]
    public async Task<IActionResult> CreateShift([FromBody] CreateWorkingShiftDto dto)
        => HandleResult(await Mediator.Send(new CreateWorkingShiftCommand(TenantId, dto)));

    [HttpGet(ApiRoutes.Hr.Positions)]
    [HasPermission(Permissions.Hr.Employee.View)]
    public async Task<IActionResult> GetPositions([FromQuery] Guid companyId)
        => HandleResult(await Mediator.Send(new GetPositionsQuery(TenantId, companyId)));

    [HttpPost(ApiRoutes.Hr.Positions)]
    [HasPermission(Permissions.Hr.Employee.Create)]
    public async Task<IActionResult> CreatePosition([FromBody] CreatePositionDto dto)
        => HandleResult(await Mediator.Send(new CreatePositionCommand(TenantId, dto)));
}

[ApiVersion("1.0")]
[Tags("HR — Leave")]
public class LeaveController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Hr.Leave)]
    [HasPermission(Permissions.Hr.Leave.View)]
    public async Task<IActionResult> GetLeaveRequests([FromQuery] HrFilterDto filter, [FromQuery] LeaveRequestStatus? status)
        => HandleResult(await Mediator.Send(new GetLeaveRequestsQuery(TenantId, filter, status)));

    [HttpPost(ApiRoutes.Hr.Leave)]
    [HasPermission(Permissions.Hr.Leave.Request)]
    public async Task<IActionResult> SubmitLeave([FromBody] SubmitLeaveDto dto)
        => HandleResult(await Mediator.Send(new SubmitLeaveCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Hr.Leave}/approve")]
    [HasPermission(Permissions.Hr.Leave.Approve)]
    public async Task<IActionResult> ApproveLeave([FromBody] ApproveLeaveDto dto)
        => HandleResult(await Mediator.Send(new ApproveLeaveCommand(TenantId, CurrentUserId, dto)));
}

[ApiVersion("1.0")]
[Tags("HR — Payroll")]
public class PayrollController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Hr.Payroll)]
    [HasPermission(Permissions.Hr.Payroll.View)]
    public async Task<IActionResult> GetRuns([FromQuery] Guid companyId)
        => HandleResult(await Mediator.Send(new GetPayrollRunsQuery(TenantId, companyId)));

    [HttpPost(ApiRoutes.Hr.Payroll)]
    [HasPermission(Permissions.Hr.Payroll.Generate)]
    public async Task<IActionResult> CreateRun([FromBody] CreatePayrollRunDto dto)
        => HandleResult(await Mediator.Send(new CreatePayrollRunCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Hr.Payroll}/{{id:guid}}/calculate")]
    [HasPermission(Permissions.Hr.Payroll.Generate)]
    public async Task<IActionResult> CalculateRun(Guid id)
        => HandleResult(await Mediator.Send(new CalculatePayrollRunCommand(TenantId, id)));

    [HttpPost($"{ApiRoutes.Hr.Payroll}/{{id:guid}}/approve")]
    [HasPermission(Permissions.Hr.Payroll.Approve)]
    public async Task<IActionResult> ApproveRun(Guid id)
        => HandleResult(await Mediator.Send(new ApprovePayrollRunCommand(TenantId, id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Hr.Payroll}/{{id:guid}}/post")]
    [HasPermission(Permissions.Hr.Payroll.Post)]
    public async Task<IActionResult> PostRun(Guid id)
        => HandleResult(await Mediator.Send(new PostPayrollRunCommand(TenantId, id, CurrentUserId)));

    [HttpGet($"{ApiRoutes.Hr.Payroll}/{{id:guid}}/payslips")]
    [HasPermission(Permissions.Hr.Payroll.View)]
    public async Task<IActionResult> GetPayslips(Guid id)
        => HandleResult(await Mediator.Send(new GetPayslipsQuery(TenantId, id)));

    [HttpGet($"{ApiRoutes.Hr.Payroll}/salary-structure/{{employeeId:guid}}")]
    [HasPermission(Permissions.Hr.Payroll.View)]
    public async Task<IActionResult> GetSalaryStructure(Guid employeeId)
        => HandleResult(await Mediator.Send(new GetSalaryStructureQuery(TenantId, employeeId)));

    [HttpPut($"{ApiRoutes.Hr.Payroll}/salary-structure")]
    [HasPermission(Permissions.Hr.Payroll.Generate)]
    public async Task<IActionResult> UpsertSalaryStructure([FromBody] UpsertSalaryStructureDto dto)
        => HandleResult(await Mediator.Send(new UpsertSalaryStructureCommand(TenantId, dto)));
}

[ApiVersion("1.0")]
[Tags("HR — Performance")]
public class PerformanceController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Hr.Performance)]
    [HasPermission(Permissions.Hr.Performance.View)]
    public async Task<IActionResult> GetRecords([FromQuery] Guid? employeeId)
        => HandleResult(await Mediator.Send(new GetPerformanceRecordsQuery(TenantId, employeeId)));

    [HttpPost(ApiRoutes.Hr.Performance)]
    [HasPermission(Permissions.Hr.Performance.Manage)]
    public async Task<IActionResult> Record([FromBody] CreatePerformanceDto dto)
        => HandleResult(await Mediator.Send(new RecordPerformanceCommand(TenantId, CurrentUserId, dto)));
}

[ApiVersion("1.0")]
[Tags("HR — Recruitment")]
public class RecruitmentController : BaseApiController
{
    [HttpGet(ApiRoutes.Hr.Recruitment)]
    [HasPermission(Permissions.Hr.Recruitment.View)]
    public async Task<IActionResult> GetApplicants([FromQuery] Guid companyId)
        => HandleResult(await Mediator.Send(new GetApplicantsQuery(TenantId, companyId)));

    [HttpPost(ApiRoutes.Hr.Recruitment)]
    [HasPermission(Permissions.Hr.Recruitment.Manage)]
    public async Task<IActionResult> Apply([FromBody] CreateApplicantDto dto)
        => HandleResult(await Mediator.Send(new ApplyApplicantCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Hr.Recruitment}/interviews")]
    [HasPermission(Permissions.Hr.Recruitment.Manage)]
    public async Task<IActionResult> ScheduleInterview([FromBody] ScheduleInterviewDto dto)
        => HandleResult(await Mediator.Send(new ScheduleInterviewCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Hr.Recruitment}/hire")]
    [HasPermission(Permissions.Hr.Recruitment.Manage)]
    public async Task<IActionResult> Hire([FromBody] HireApplicantDto dto)
        => HandleResult(await Mediator.Send(new HireApplicantCommand(TenantId, dto)));
}

[ApiVersion("1.0")]
[Tags("HR — Training")]
public class TrainingController : BaseApiController
{
    [HttpGet(ApiRoutes.Hr.Training)]
    [HasPermission(Permissions.Hr.Training.View)]
    public async Task<IActionResult> GetCourses()
        => HandleResult(await Mediator.Send(new GetTrainingCoursesQuery(TenantId)));

    [HttpPost(ApiRoutes.Hr.Training)]
    [HasPermission(Permissions.Hr.Training.Manage)]
    public async Task<IActionResult> CreateCourse([FromBody] CreateTrainingCourseDto dto)
        => HandleResult(await Mediator.Send(new CreateTrainingCourseCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Hr.Training}/enroll")]
    [HasPermission(Permissions.Hr.Training.Manage)]
    public async Task<IActionResult> Enroll([FromBody] EnrollTrainingDto dto)
        => HandleResult(await Mediator.Send(new EnrollTrainingCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Hr.Training}/complete")]
    [HasPermission(Permissions.Hr.Training.Manage)]
    public async Task<IActionResult> Complete([FromBody] CompleteTrainingDto dto)
        => HandleResult(await Mediator.Send(new CompleteTrainingCommand(TenantId, dto)));
}

[ApiVersion("1.0")]
[Tags("HR — Dashboard")]
public class HrDashboardController : BaseApiController
{
    [HttpGet(ApiRoutes.Hr.Dashboard)]
    [HasPermission(Permissions.Hr.Dashboard.View)]
    public async Task<IActionResult> GetDashboard([FromQuery] Guid? branchId)
        => HandleResult(await Mediator.Send(new GetHrDashboardQuery(TenantId, branchId)));
}

[ApiVersion("1.0")]
[Tags("HR — Self Service")]
public class HrSelfServiceController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Hr.SelfService}/{{employeeId:guid}}")]
    [HasPermission(Permissions.Hr.SelfService.Use)]
    public async Task<IActionResult> GetProfile(Guid employeeId)
        => HandleResult(await Mediator.Send(new GetSelfServiceProfileQuery(TenantId, employeeId)));

    [HttpPut($"{ApiRoutes.Hr.SelfService}/{{employeeId:guid}}")]
    [HasPermission(Permissions.Hr.SelfService.Use)]
    public async Task<IActionResult> UpdateProfile(Guid employeeId, [FromBody] UpdateEmployeeDto dto)
        => HandleResult(await Mediator.Send(new UpdateSelfProfileCommand(TenantId, employeeId, dto)));

    [HttpPost($"{ApiRoutes.Hr.SelfService}/{{employeeId:guid}}/leave")]
    [HasPermission(Permissions.Hr.SelfService.Use)]
    public async Task<IActionResult> SubmitLeave(Guid employeeId, [FromBody] SubmitLeaveDto dto)
        => HandleResult(await Mediator.Send(new SubmitSelfLeaveCommand(TenantId, employeeId, dto)));
}

[ApiVersion("1.0")]
[Tags("HR — Workflow Requests")]
public class HrWorkflowRequestsController : BaseApiController
{
    [HttpGet(ApiRoutes.Hr.WorkflowRequests)]
    [HasPermission(Permissions.Hr.Leave.View)]
    public async Task<IActionResult> List([FromQuery] Guid? employeeId, [FromQuery] Domain.Enums.HrWorkflowRequestType? type)
        => HandleResult(await Mediator.Send(new GetHrWorkflowRequestsQuery(TenantId, employeeId, type)));

    [HttpPost(ApiRoutes.Hr.WorkflowRequests)]
    [HasPermission(Permissions.Hr.Leave.Request)]
    public async Task<IActionResult> Submit([FromBody] SubmitHrWorkflowRequestDto dto)
        => HandleResult(await Mediator.Send(new SubmitHrWorkflowRequestCommand(TenantId, dto)));
}
