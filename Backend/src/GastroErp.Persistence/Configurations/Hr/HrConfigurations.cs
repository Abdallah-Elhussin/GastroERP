using GastroErp.Domain.Entities.HR;
using GastroErp.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Hr;

internal static class HrConfigHelper
{
    public static void ConfigureAuditable<T>(EntityTypeBuilder<T> builder) where T : Domain.Common.AuditableBaseEntity
    {
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmployeeNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(300);
        builder.Property(x => x.NameAr).HasMaxLength(300);
        builder.Property(x => x.NationalId).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.PhoneNumber).HasMaxLength(30);
        HrConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.Status }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class EmployeeContractConfiguration : IEntityTypeConfiguration<EmployeeContract>
{
    public void Configure(EntityTypeBuilder<EmployeeContract> builder)
    {
        builder.ToTable("EmployeeContracts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BaseSalary).HasPrecision(18, 4);
        builder.Property(x => x.Currency).HasMaxLength(3);
        HrConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.IsActive }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class EmployeeEmergencyContactConfiguration : IEntityTypeConfiguration<EmployeeEmergencyContact>
{
    public void Configure(EntityTypeBuilder<EmployeeEmergencyContact> builder)
    {
        builder.ToTable("EmployeeEmergencyContacts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Phone).IsRequired().HasMaxLength(30);
        HrConfigHelper.ConfigureAuditable(builder);
    }
}

public sealed class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.ToTable("EmployeeDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(300);
        builder.Property(x => x.StoragePath).HasMaxLength(500);
        HrConfigHelper.ConfigureAuditable(builder);
    }
}

public sealed class EmploymentHistoryEntryConfiguration : IEntityTypeConfiguration<EmploymentHistoryEntry>
{
    public void Configure(EntityTypeBuilder<EmploymentHistoryEntry> builder)
    {
        builder.ToTable("EmploymentHistoryEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(1000);
        HrConfigHelper.ConfigureAuditable(builder);
    }
}

public sealed class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DeviceId).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(500);
        HrConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.WorkDate }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalDays).HasPrecision(8, 2);
        builder.Property(x => x.UsedDays).HasPrecision(8, 2);
        HrConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.LeaveType, x.Year }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Days).HasPrecision(8, 2);
        builder.Property(x => x.RejectionReason).HasMaxLength(500);
        HrConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.Status }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class WorkScheduleEntryConfiguration : IEntityTypeConfiguration<WorkScheduleEntry>
{
    public void Configure(EntityTypeBuilder<WorkScheduleEntry> builder)
    {
        builder.ToTable("WorkScheduleEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(500);
        HrConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.ScheduleDate }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class SalaryStructureConfiguration : IEntityTypeConfiguration<SalaryStructure>
{
    public void Configure(EntityTypeBuilder<SalaryStructure> builder)
    {
        builder.ToTable("SalaryStructures");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BaseSalary).HasPrecision(18, 4);
        builder.Property(x => x.HousingAllowance).HasPrecision(18, 4);
        builder.Property(x => x.TransportAllowance).HasPrecision(18, 4);
        builder.Property(x => x.OtherAllowances).HasPrecision(18, 4);
        builder.Property(x => x.FixedDeductions).HasPrecision(18, 4);
        builder.Property(x => x.Currency).HasMaxLength(3);
        HrConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.IsActive }).HasFilter("[IsDeleted] = 0 AND [IsActive] = 1");
    }
}

public sealed class PayrollRunConfiguration : IEntityTypeConfiguration<PayrollRun>
{
    public void Configure(EntityTypeBuilder<PayrollRun> builder)
    {
        builder.ToTable("PayrollRuns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalGross).HasPrecision(18, 4);
        builder.Property(x => x.TotalDeductions).HasPrecision(18, 4);
        builder.Property(x => x.TotalNet).HasPrecision(18, 4);
        HrConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.Year, x.Month }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class PayrollPayslipConfiguration : IEntityTypeConfiguration<PayrollPayslip>
{
    public void Configure(EntityTypeBuilder<PayrollPayslip> builder)
    {
        builder.ToTable("PayrollPayslips");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.GrossPay).HasPrecision(18, 4);
        builder.Property(x => x.Deductions).HasPrecision(18, 4);
        builder.Property(x => x.OvertimePay).HasPrecision(18, 4);
        builder.Property(x => x.Bonuses).HasPrecision(18, 4);
        builder.Property(x => x.NetPay).HasPrecision(18, 4);
        builder.Property(x => x.Currency).HasMaxLength(3);
        builder.Property(x => x.ComponentsJson).IsRequired();
        HrConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.PayrollRunId, x.EmployeeId }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class PerformanceRecordConfiguration : IEntityTypeConfiguration<PerformanceRecord>
{
    public void Configure(EntityTypeBuilder<PerformanceRecord> builder)
    {
        builder.ToTable("PerformanceRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.Score).HasPrecision(8, 2);
        HrConfigHelper.ConfigureAuditable(builder);
    }
}

public sealed class JobApplicantConfiguration : IEntityTypeConfiguration<JobApplicant>
{
    public void Configure(EntityTypeBuilder<JobApplicant> builder)
    {
        builder.ToTable("JobApplicants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(300);
        builder.Property(x => x.NameAr).HasMaxLength(300);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.ResumePath).HasMaxLength(500);
        HrConfigHelper.ConfigureAuditable(builder);
    }
}

public sealed class InterviewRecordConfiguration : IEntityTypeConfiguration<InterviewRecord>
{
    public void Configure(EntityTypeBuilder<InterviewRecord> builder)
    {
        builder.ToTable("InterviewRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InterviewerName).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.Rating).HasPrecision(5, 2);
        HrConfigHelper.ConfigureAuditable(builder);
    }
}

public sealed class TrainingCourseConfiguration : IEntityTypeConfiguration<TrainingCourse>
{
    public void Configure(EntityTypeBuilder<TrainingCourse> builder)
    {
        builder.ToTable("TrainingCourses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.TitleAr).HasMaxLength(200);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(2000);
        HrConfigHelper.ConfigureAuditable(builder);
    }
}

public sealed class EmployeeTrainingRecordConfiguration : IEntityTypeConfiguration<EmployeeTrainingRecord>
{
    public void Configure(EntityTypeBuilder<EmployeeTrainingRecord> builder)
    {
        builder.ToTable("EmployeeTrainingRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Score).HasPrecision(8, 2);
        HrConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.CourseId }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class WorkingShiftConfiguration : IEntityTypeConfiguration<WorkingShift>
{
    public void Configure(EntityTypeBuilder<WorkingShift> builder)
    {
        builder.ToTable("WorkingShifts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameAr).HasMaxLength(100);
        HrConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.Name }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class EmployeePositionConfiguration : IEntityTypeConfiguration<EmployeePosition>
{
    public void Configure(EntityTypeBuilder<EmployeePosition> builder)
    {
        builder.ToTable("EmployeePositions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameAr).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        HrConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.Name }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("Holidays");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NameAr).HasMaxLength(200);
        HrConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.Date }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class HrWorkflowRequestConfiguration : IEntityTypeConfiguration<HrWorkflowRequest>
{
    public void Configure(EntityTypeBuilder<HrWorkflowRequest> builder)
    {
        builder.ToTable("HrWorkflowRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.MetadataJson).HasMaxLength(4000);
        builder.Property(x => x.Amount).HasPrecision(18, 4);
        builder.Property(x => x.RejectionReason).HasMaxLength(1000);
        HrConfigHelper.ConfigureAuditable(builder);
        builder.HasIndex(x => new { x.TenantId, x.EmployeeId, x.RequestType, x.Status }).HasFilter("[IsDeleted] = 0");
    }
}
