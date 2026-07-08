using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Hr.DTOs;

public record HrFilterDto(Guid? CompanyId = null, Guid? BranchId = null, Guid? DepartmentId = null, int Page = 1, int PageSize = 50);

public record CreateEmployeeDto(
    Guid CompanyId, string Name, Guid? BranchId = null,
    Guid? DepartmentId = null, Guid? PositionId = null, Guid? UserId = null,
    string? NameAr = null, string? NationalId = null,
    string? Email = null, string? Phone = null, DateOnly? HireDate = null);

public record UpdateEmployeeDto(
    string Name, string? NameAr,
    string? Email, string? Phone, string? NationalId,
    Guid? BranchId, Guid? DepartmentId, Guid? PositionId);

public record EmployeeDto(
    Guid Id, string EmployeeNumber, string Name, string? NameAr, EmploymentStatus Status,
    Guid CompanyId, Guid? BranchId, Guid? DepartmentId, Guid? PositionId, Guid? UserId,
    string? Email, string? Phone, DateOnly? HireDate, DateTimeOffset CreatedAt);

public record CreateContractDto(Guid EmployeeId, ContractType ContractType, DateOnly StartDate, decimal BaseSalary, DateOnly? EndDate = null);
public record ContractDto(Guid Id, Guid EmployeeId, ContractType ContractType, DateOnly StartDate, DateOnly? EndDate, decimal BaseSalary, string Currency, bool IsActive);

public record EmergencyContactDto(Guid Id, Guid EmployeeId, string Name, string Phone, string? Relationship);
public record CreateEmergencyContactDto(Guid EmployeeId, string Name, string Phone, string? Relationship);

public record CheckInDto(Guid EmployeeId, Guid BranchId, string? DeviceId = null);
public record CheckOutDto(Guid AttendanceId, int OvertimeMinutes = 0);
public record AttendanceDto(
    Guid Id, Guid EmployeeId, string EmployeeName, Guid BranchId, DateOnly WorkDate,
    DateTimeOffset? CheckInAt, DateTimeOffset? CheckOutAt, AttendanceStatus Status,
    int BreakMinutes, int OvertimeMinutes, bool IsLate);

public record SubmitLeaveDto(Guid EmployeeId, LeaveType LeaveType, DateOnly FromDate, DateOnly ToDate, string Reason);
public record LeaveRequestDto(
    Guid Id, Guid EmployeeId, string EmployeeName, LeaveType LeaveType,
    DateOnly FromDate, DateOnly ToDate, decimal Days, LeaveRequestStatus Status, string Reason);

public record ApproveLeaveDto(Guid LeaveRequestId, bool Approve, string? RejectionReason = null);

public record CreateScheduleDto(
    Guid EmployeeId, Guid BranchId, DateOnly ScheduleDate, ScheduleRole Role,
    Guid? WorkingShiftId = null, TimeOnly? StartTime = null, TimeOnly? EndTime = null, string? Notes = null);

public record ScheduleEntryDto(
    Guid Id, Guid EmployeeId, string EmployeeName, Guid BranchId, DateOnly ScheduleDate,
    ScheduleRole Role, Guid? WorkingShiftId, TimeOnly? StartTime, TimeOnly? EndTime);

public record CreateWorkingShiftDto(Guid BranchId, string Name, TimeOnly StartTime, TimeOnly EndTime, string? NameAr = null);
public record WorkingShiftDto(Guid Id, Guid BranchId, string Name, string? NameAr, TimeOnly StartTime, TimeOnly EndTime, bool IsActive);

public record CreatePositionDto(Guid CompanyId, string Name, Guid? DepartmentId = null, string? NameAr = null, string? Description = null);
public record PositionDto(Guid Id, Guid CompanyId, Guid? DepartmentId, string Name, string? NameAr, bool IsActive);

public record SalaryStructureDto(
    Guid Id, Guid EmployeeId, decimal BaseSalary, decimal HousingAllowance,
    decimal TransportAllowance, decimal OtherAllowances, decimal FixedDeductions, decimal GrossSalary, string Currency);

public record UpsertSalaryStructureDto(
    Guid EmployeeId, decimal BaseSalary, decimal HousingAllowance = 0,
    decimal TransportAllowance = 0, decimal OtherAllowances = 0, decimal FixedDeductions = 0);

public record CreatePayrollRunDto(Guid CompanyId, int Year, int Month);
public record PayrollRunDto(
    Guid Id, Guid CompanyId, int Year, int Month, PayrollRunStatus Status,
    decimal TotalGross, decimal TotalDeductions, decimal TotalNet, Guid? PostedJournalId);

public record PayslipDto(
    Guid Id, Guid PayrollRunId, Guid EmployeeId, string EmployeeName,
    decimal GrossPay, decimal Deductions, decimal OvertimePay, decimal Bonuses, decimal NetPay,
    PayslipStatus Status, string Currency);

public record CreatePerformanceDto(
    Guid EmployeeId, PerformanceRecordType RecordType, string Title, string Description,
    DateOnly RecordDate, decimal? Score = null);

public record PerformanceDto(Guid Id, Guid EmployeeId, string EmployeeName, PerformanceRecordType RecordType, string Title, decimal? Score, DateOnly RecordDate);

public record CreateApplicantDto(Guid CompanyId, string Name, string Email, Guid? PositionId = null, string? Phone = null, string? NameAr = null);
public record ApplicantDto(Guid Id, string Name, string? NameAr, string Email, ApplicantStatus Status, Guid? PositionId, Guid? HiredEmployeeId);

public record ScheduleInterviewDto(Guid ApplicantId, DateTimeOffset ScheduledAt, string? InterviewerName = null);
public record HireApplicantDto(Guid ApplicantId, CreateEmployeeDto Employee);

public record CreateTrainingCourseDto(string Title, string Description, int DurationHours, string? TitleAr = null, bool RequiresCertification = false);
public record TrainingCourseDto(Guid Id, string Title, string? TitleAr, int DurationHours, bool RequiresCertification, bool IsActive);

public record EnrollTrainingDto(Guid EmployeeId, Guid CourseId);
public record CompleteTrainingDto(Guid RecordId, decimal? Score = null, DateOnly? CertificationExpiry = null);

public record HrDashboardDto(
    int ActiveEmployees, int OnLeaveEmployees, int PendingLeaveRequests,
    int TodayPresent, int TodayAbsent, int OpenPayrollRuns, DateTimeOffset GeneratedAt);

public record SelfServiceProfileDto(
    EmployeeDto Employee, IReadOnlyList<PayslipDto> RecentPayslips,
    IReadOnlyList<LeaveRequestDto> LeaveRequests, IReadOnlyList<AttendanceDto> RecentAttendance);

public record TerminateEmployeeDto(string Reason);

public record SubmitHrWorkflowRequestDto(
    Guid EmployeeId, HrWorkflowRequestType RequestType, string Title,
    string? Description = null, decimal? Amount = null, string? MetadataJson = null);

public record HrWorkflowRequestDto(
    Guid Id, Guid EmployeeId, HrWorkflowRequestType RequestType, HrWorkflowRequestStatus Status,
    string Title, decimal? Amount, DateTimeOffset CreatedAt);
