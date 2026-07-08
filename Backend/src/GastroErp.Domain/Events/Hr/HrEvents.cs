using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Events.Hr;

public sealed record EmployeeHiredEvent(Guid EmployeeId, Guid TenantId, string EmployeeNumber) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record EmployeeTerminatedEvent(Guid EmployeeId, Guid TenantId, string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record AttendanceRecordedEvent(Guid RecordId, Guid TenantId, Guid EmployeeId, AttendanceStatus Status) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record LeaveRequestedEvent(Guid LeaveId, Guid TenantId, Guid EmployeeId, LeaveType Type) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record LeaveApprovedEvent(Guid LeaveId, Guid TenantId, Guid EmployeeId, Guid ApprovedBy) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record LeaveRejectedEvent(Guid LeaveId, Guid TenantId, Guid EmployeeId, string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record PayrollGeneratedEvent(Guid RunId, Guid TenantId, Guid CompanyId, int Year, int Month, decimal TotalNet) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record PayrollApprovedEvent(Guid RunId, Guid TenantId, decimal TotalNet) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record PayrollPostedEvent(Guid RunId, Guid TenantId, decimal TotalNet, Guid? JournalId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record PayrollRunCompletedEvent(Guid RunId, Guid TenantId, decimal TotalNet, PayrollRunStatus Status) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record PerformanceEvaluatedEvent(Guid RecordId, Guid TenantId, Guid EmployeeId, PerformanceRecordType RecordType) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record TrainingCompletedEvent(Guid RecordId, Guid TenantId, Guid EmployeeId, Guid CourseId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record ApplicantHiredEvent(Guid ApplicantId, Guid TenantId, Guid EmployeeId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record HrWorkflowRequestSubmittedEvent(Guid RequestId, Guid TenantId, Guid EmployeeId, HrWorkflowRequestType Type, decimal? Amount) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record RecruitmentApprovalRequestedEvent(Guid ApplicantId, Guid TenantId, Guid CompanyId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
