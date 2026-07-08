using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Hr;

namespace GastroErp.Domain.Entities.HR;

public sealed class EmployeeContract : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public ContractType ContractType { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public decimal BaseSalary { get; private set; }
    public string Currency { get; private set; }
    public bool IsActive { get; private set; }

    private EmployeeContract() => Currency = "SAR";

    public static EmployeeContract Create(Guid tenantId, Guid employeeId, ContractType type,
        DateOnly startDate, decimal baseSalary, DateOnly? endDate = null, string currency = "SAR")
        => new()
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            ContractType = type,
            StartDate = startDate,
            EndDate = endDate,
            BaseSalary = baseSalary,
            Currency = currency,
            IsActive = true
        };

    public void Terminate(DateOnly endDate) { EndDate = endDate; IsActive = false; }
}

public sealed class EmployeeEmergencyContact : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string Name { get; private set; }
    public string Phone { get; private set; }
    public string? Relationship { get; private set; }

    private EmployeeEmergencyContact() { Name = string.Empty; Phone = string.Empty; }

    public static EmployeeEmergencyContact Create(Guid tenantId, Guid employeeId, string name, string phone, string? relationship = null)
    {
        EmployeeNameRules.EnsureTripleName(name);
        if (string.IsNullOrWhiteSpace(phone))
            throw new Domain.Common.Exceptions.BusinessException(Domain.Common.Localization.ErrorCodes.RequiredField, "Phone required.");
        return new()
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            Name = name.Trim(),
            Phone = phone.Trim(),
            Relationship = relationship?.Trim()
        };
    }
}

public sealed class EmployeeDocument : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string DocumentType { get; private set; }
    public string FileName { get; private set; }
    public string? StoragePath { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }

    private EmployeeDocument() { DocumentType = string.Empty; FileName = string.Empty; }

    public static EmployeeDocument Create(Guid tenantId, Guid employeeId, string documentType, string fileName,
        string? storagePath = null, DateOnly? expiryDate = null)
        => new() { TenantId = tenantId, EmployeeId = employeeId, DocumentType = documentType, FileName = fileName, StoragePath = storagePath, ExpiryDate = expiryDate };
}

public sealed class EmploymentHistoryEntry : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public string EventType { get; private set; }
    public string Description { get; private set; }
    public DateOnly EffectiveDate { get; private set; }

    private EmploymentHistoryEntry() { EventType = string.Empty; Description = string.Empty; }

    public static EmploymentHistoryEntry Create(Guid tenantId, Guid employeeId, string eventType, string description, DateOnly effectiveDate)
        => new() { TenantId = tenantId, EmployeeId = employeeId, EventType = eventType, Description = description, EffectiveDate = effectiveDate };
}

public sealed class AttendanceRecord : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid BranchId { get; private set; }
    public DateOnly WorkDate { get; private set; }
    public DateTimeOffset? CheckInAt { get; private set; }
    public DateTimeOffset? CheckOutAt { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public int BreakMinutes { get; private set; }
    public int OvertimeMinutes { get; private set; }
    public bool IsLate { get; private set; }
    public string? DeviceId { get; private set; }
    public string? Notes { get; private set; }

    private AttendanceRecord() { }

    public static AttendanceRecord Create(Guid tenantId, Guid employeeId, Guid branchId, DateOnly workDate, string? deviceId = null)
        => new()
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            BranchId = branchId,
            WorkDate = workDate,
            Status = AttendanceStatus.CheckedIn,
            CheckInAt = DateTimeOffset.UtcNow,
            DeviceId = deviceId
        };

    public void CheckOut(int overtimeMinutes = 0)
    {
        CheckOutAt = DateTimeOffset.UtcNow;
        OvertimeMinutes = overtimeMinutes;
        Status = AttendanceStatus.CheckedOut;
    }

    public void StartBreak() => Status = AttendanceStatus.OnBreak;
    public void EndBreak(int minutes)
    {
        BreakMinutes += minutes;
        Status = AttendanceStatus.CheckedIn;
    }

    public void MarkLate() => IsLate = true;
    public void MarkAbsent() => Status = AttendanceStatus.Absent;
}

public sealed class LeaveBalance : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public LeaveType LeaveType { get; private set; }
    public decimal TotalDays { get; private set; }
    public decimal UsedDays { get; private set; }
    public int Year { get; private set; }

    public decimal RemainingDays => TotalDays - UsedDays;

    private LeaveBalance() { }

    public static LeaveBalance Create(Guid tenantId, Guid employeeId, LeaveType type, decimal totalDays, int year)
        => new() { TenantId = tenantId, EmployeeId = employeeId, LeaveType = type, TotalDays = totalDays, Year = year };

    public void Use(decimal days) => UsedDays += days;
    public void Restore(decimal days) => UsedDays = Math.Max(0, UsedDays - days);
}

public sealed class LeaveRequest : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public LeaveType LeaveType { get; private set; }
    public DateOnly FromDate { get; private set; }
    public DateOnly ToDate { get; private set; }
    public decimal Days { get; private set; }
    public string Reason { get; private set; }
    public LeaveRequestStatus Status { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    private LeaveRequest() => Reason = string.Empty;

    public static LeaveRequest Submit(Guid tenantId, Guid employeeId, LeaveType type,
        DateOnly from, DateOnly to, decimal days, string reason)
    {
        var req = new LeaveRequest
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            LeaveType = type,
            FromDate = from,
            ToDate = to,
            Days = days,
            Reason = reason,
            Status = LeaveRequestStatus.Pending
        };
        req.RaiseDomainEvent(new LeaveRequestedEvent(req.Id, tenantId, employeeId, type));
        return req;
    }

    public void Approve(Guid approverId)
    {
        Status = LeaveRequestStatus.Approved;
        ApprovedBy = approverId;
        ApprovedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new LeaveApprovedEvent(Id, TenantId, EmployeeId, approverId));
    }

    public void Reject(string reason)
    {
        Status = LeaveRequestStatus.Rejected;
        RejectionReason = reason;
        RaiseDomainEvent(new LeaveRejectedEvent(Id, TenantId, EmployeeId, reason));
    }

    public void Cancel() => Status = LeaveRequestStatus.Cancelled;
}

public sealed class WorkScheduleEntry : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid? WorkingShiftId { get; private set; }
    public DateOnly ScheduleDate { get; private set; }
    public ScheduleRole Role { get; private set; }
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }
    public string? Notes { get; private set; }

    private WorkScheduleEntry() { }

    public static WorkScheduleEntry Create(Guid tenantId, Guid employeeId, Guid branchId,
        DateOnly date, ScheduleRole role, Guid? workingShiftId = null,
        TimeOnly? start = null, TimeOnly? end = null, string? notes = null)
        => new()
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            BranchId = branchId,
            ScheduleDate = date,
            Role = role,
            WorkingShiftId = workingShiftId,
            StartTime = start,
            EndTime = end,
            Notes = notes
        };
}

public sealed class SalaryStructure : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public decimal BaseSalary { get; private set; }
    public decimal HousingAllowance { get; private set; }
    public decimal TransportAllowance { get; private set; }
    public decimal OtherAllowances { get; private set; }
    public decimal FixedDeductions { get; private set; }
    public string Currency { get; private set; }
    public bool IsActive { get; private set; }

    private SalaryStructure() => Currency = "SAR";

    public decimal GrossSalary => BaseSalary + HousingAllowance + TransportAllowance + OtherAllowances;

    public static SalaryStructure Create(Guid tenantId, Guid employeeId, decimal baseSalary,
        decimal housing = 0, decimal transport = 0, decimal otherAllowances = 0, decimal deductions = 0, string currency = "SAR")
        => new()
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            BaseSalary = baseSalary,
            HousingAllowance = housing,
            TransportAllowance = transport,
            OtherAllowances = otherAllowances,
            FixedDeductions = deductions,
            Currency = currency,
            IsActive = true
        };

    public void Deactivate() => IsActive = false;
}

public sealed class PayrollRun : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid CompanyId { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; }
    public PayrollRunStatus Status { get; private set; }
    public decimal TotalGross { get; private set; }
    public decimal TotalDeductions { get; private set; }
    public decimal TotalNet { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? PostedJournalId { get; private set; }

    private PayrollRun() { }

    public static PayrollRun Create(Guid tenantId, Guid companyId, int year, int month)
        => new() { TenantId = tenantId, CompanyId = companyId, Year = year, Month = month, Status = PayrollRunStatus.Draft };

    public void SetTotals(decimal gross, decimal deductions, decimal net)
    {
        TotalGross = gross;
        TotalDeductions = deductions;
        TotalNet = net;
        Status = PayrollRunStatus.Calculated;
        RaiseDomainEvent(new PayrollGeneratedEvent(Id, TenantId, CompanyId, Year, Month, net));
    }

    public void Approve(Guid userId)
    {
        Status = PayrollRunStatus.Approved;
        ApprovedBy = userId;
        ApprovedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new PayrollApprovedEvent(Id, TenantId, TotalNet));
    }

    public void MarkPosted(Guid journalId)
    {
        Status = PayrollRunStatus.Posted;
        PostedJournalId = journalId;
        RaiseDomainEvent(new PayrollPostedEvent(Id, TenantId, TotalNet, journalId));
        RaiseDomainEvent(new PayrollRunCompletedEvent(Id, TenantId, TotalNet, Status));
    }

    public void Cancel() => Status = PayrollRunStatus.Cancelled;
}

public sealed class PayrollPayslip : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid PayrollRunId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public PayslipStatus Status { get; private set; }
    public decimal GrossPay { get; private set; }
    public decimal Deductions { get; private set; }
    public decimal OvertimePay { get; private set; }
    public decimal Bonuses { get; private set; }
    public decimal NetPay { get; private set; }
    public string Currency { get; private set; }
    public string ComponentsJson { get; private set; }

    private PayrollPayslip() { Currency = "SAR"; ComponentsJson = "[]"; }

    public static PayrollPayslip Create(Guid tenantId, Guid runId, Guid employeeId,
        decimal gross, decimal deductions, decimal overtime, decimal bonuses, decimal net, string componentsJson, string currency = "SAR")
        => new()
        {
            TenantId = tenantId,
            PayrollRunId = runId,
            EmployeeId = employeeId,
            GrossPay = gross,
            Deductions = deductions,
            OvertimePay = overtime,
            Bonuses = bonuses,
            NetPay = net,
            ComponentsJson = componentsJson,
            Currency = currency,
            Status = PayslipStatus.Draft
        };

    public void FinalizePayslip() => Status = PayslipStatus.Finalized;
    public void MarkPaid() => Status = PayslipStatus.Paid;
}

public sealed class PerformanceRecord : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public PerformanceRecordType RecordType { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public decimal? Score { get; private set; }
    public Guid? RecordedBy { get; private set; }
    public DateOnly RecordDate { get; private set; }

    private PerformanceRecord() { Title = string.Empty; Description = string.Empty; }

    public static PerformanceRecord Create(Guid tenantId, Guid employeeId, PerformanceRecordType type,
        string title, string description, DateOnly date, decimal? score = null, Guid? recordedBy = null)
    {
        var record = new PerformanceRecord
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            RecordType = type,
            Title = title,
            Description = description,
            RecordDate = date,
            Score = score,
            RecordedBy = recordedBy
        };
        record.RaiseDomainEvent(new PerformanceEvaluatedEvent(record.Id, tenantId, employeeId, type));
        return record;
    }
}

public sealed class JobApplicant : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid? PositionId { get; private set; }
    public string Name { get; private set; }
    public string? NameAr { get; private set; }
    public string Email { get; private set; }
    public string? Phone { get; private set; }
    public ApplicantStatus Status { get; private set; }
    public string? ResumePath { get; private set; }
    public Guid? HiredEmployeeId { get; private set; }

    public string DisplayName => !string.IsNullOrWhiteSpace(NameAr) ? NameAr : Name;

    private JobApplicant() { Name = string.Empty; Email = string.Empty; }

    public static JobApplicant Apply(Guid tenantId, Guid companyId, string name, string email,
        Guid? positionId = null, string? phone = null, string? resumePath = null, string? nameAr = null)
    {
        EmployeeNameRules.EnsureTripleName(name);
        EmployeeNameRules.EnsureOptionalTripleName(nameAr);
        if (string.IsNullOrWhiteSpace(email))
            throw new Domain.Common.Exceptions.BusinessException(Domain.Common.Localization.ErrorCodes.RequiredField, "Email required.");

        return new()
        {
            TenantId = tenantId,
            CompanyId = companyId,
            PositionId = positionId,
            Name = name.Trim(),
            NameAr = nameAr?.Trim(),
            Email = email.Trim(),
            Phone = phone?.Trim(),
            ResumePath = resumePath,
            Status = ApplicantStatus.Applied
        };
    }

    public void Advance(ApplicantStatus status)
    {
        Status = status;
        if (status == ApplicantStatus.Offered)
            RaiseDomainEvent(new RecruitmentApprovalRequestedEvent(Id, TenantId, CompanyId));
    }

    public void MarkHired(Guid employeeId)
    {
        Status = ApplicantStatus.Hired;
        HiredEmployeeId = employeeId;
        RaiseDomainEvent(new ApplicantHiredEvent(Id, TenantId, employeeId));
    }
}

public sealed class InterviewRecord : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid ApplicantId { get; private set; }
    public DateTimeOffset ScheduledAt { get; private set; }
    public InterviewStatus Status { get; private set; }
    public string? InterviewerName { get; private set; }
    public string? Notes { get; private set; }
    public decimal? Rating { get; private set; }

    private InterviewRecord() { }

    public static InterviewRecord Schedule(Guid tenantId, Guid applicantId, DateTimeOffset scheduledAt, string? interviewer = null)
        => new()
        {
            TenantId = tenantId,
            ApplicantId = applicantId,
            ScheduledAt = scheduledAt,
            InterviewerName = interviewer,
            Status = InterviewStatus.Scheduled
        };

    public void Complete(decimal? rating, string? notes)
    {
        Status = InterviewStatus.Completed;
        Rating = rating;
        Notes = notes;
    }
}

public sealed class TrainingCourse : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Title { get; private set; }
    public string? TitleAr { get; private set; }
    public string Description { get; private set; }
    public int DurationHours { get; private set; }
    public bool RequiresCertification { get; private set; }
    public bool IsActive { get; private set; }

    private TrainingCourse() { Title = string.Empty; Description = string.Empty; }

    public static TrainingCourse Create(Guid tenantId, string title, string description, int durationHours,
        string? titleAr = null, bool requiresCertification = false)
        => new()
        {
            TenantId = tenantId,
            Title = title,
            TitleAr = titleAr,
            Description = description,
            DurationHours = durationHours,
            RequiresCertification = requiresCertification,
            IsActive = true
        };
}

public sealed class EmployeeTrainingRecord : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Guid CourseId { get; private set; }
    public TrainingStatus Status { get; private set; }
    public DateOnly? CompletedDate { get; private set; }
    public DateOnly? CertificationExpiry { get; private set; }
    public decimal? Score { get; private set; }

    private EmployeeTrainingRecord() { }

    public static EmployeeTrainingRecord Enroll(Guid tenantId, Guid employeeId, Guid courseId)
        => new() { TenantId = tenantId, EmployeeId = employeeId, CourseId = courseId, Status = TrainingStatus.Planned };

    public void Complete(DateOnly date, decimal? score = null, DateOnly? certExpiry = null)
    {
        Status = TrainingStatus.Completed;
        CompletedDate = date;
        Score = score;
        CertificationExpiry = certExpiry;
        RaiseDomainEvent(new TrainingCompletedEvent(Id, TenantId, EmployeeId, CourseId));
    }
}

public sealed class HrWorkflowRequest : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public HrWorkflowRequestType RequestType { get; private set; }
    public HrWorkflowRequestStatus Status { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public decimal? Amount { get; private set; }
    public string? MetadataJson { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public string? RejectionReason { get; private set; }

    private HrWorkflowRequest() => Title = string.Empty;

    public static HrWorkflowRequest Submit(Guid tenantId, Guid employeeId, HrWorkflowRequestType type,
        string title, string? description = null, decimal? amount = null, string? metadataJson = null)
    {
        var req = new HrWorkflowRequest
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            RequestType = type,
            Title = title.Trim(),
            Description = description,
            Amount = amount,
            MetadataJson = metadataJson,
            Status = HrWorkflowRequestStatus.Pending
        };
        req.RaiseDomainEvent(new HrWorkflowRequestSubmittedEvent(req.Id, tenantId, employeeId, type, amount));
        return req;
    }

    public void Approve(Guid approverId)
    {
        Status = HrWorkflowRequestStatus.Approved;
        ApprovedBy = approverId;
    }

    public void Reject(string reason)
    {
        Status = HrWorkflowRequestStatus.Rejected;
        RejectionReason = reason;
    }

    public void Cancel() => Status = HrWorkflowRequestStatus.Cancelled;
}
