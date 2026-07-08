using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Hr;

namespace GastroErp.Domain.Entities.HR;

/// <summary>Employee — الموظف (Aggregate Root)</summary>
public sealed class Employee : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public Guid? PositionId { get; private set; }
    public Guid? UserId { get; private set; }
    public string EmployeeNumber { get; private set; }
    /// <summary>Full name — at least three parts (e.g. first, father, family).</summary>
    public string Name { get; private set; }
    public string? NameAr { get; private set; }
    public string? NationalId { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public DateOnly? HireDate { get; private set; }
    public EmploymentStatus Status { get; private set; }

    public string DisplayName => !string.IsNullOrWhiteSpace(NameAr) ? NameAr : Name;

    private Employee()
    {
        EmployeeNumber = string.Empty;
        Name = string.Empty;
    }

    public static Employee Hire(
        Guid tenantId, Guid companyId, string employeeNumber,
        string name, Guid? branchId = null,
        Guid? departmentId = null, Guid? positionId = null, Guid? userId = null,
        string? nameAr = null,
        string? nationalId = null, string? email = null, string? phone = null,
        DateOnly? hireDate = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.");
        if (companyId == Guid.Empty) throw new ArgumentException("CompanyId cannot be empty.");
        if (string.IsNullOrWhiteSpace(employeeNumber)) throw new BusinessException(ErrorCodes.RequiredField, "Employee number required.");
        EmployeeNameRules.EnsureTripleName(name);
        EmployeeNameRules.EnsureOptionalTripleName(nameAr);

        var employee = new Employee
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            DepartmentId = departmentId,
            PositionId = positionId,
            UserId = userId,
            EmployeeNumber = employeeNumber,
            Name = name.Trim(),
            NameAr = nameAr?.Trim(),
            NationalId = nationalId,
            Email = email,
            PhoneNumber = phone,
            HireDate = hireDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Status = EmploymentStatus.Active
        };
        employee.RaiseDomainEvent(new EmployeeHiredEvent(employee.Id, tenantId, employeeNumber));
        return employee;
    }

    public void UpdateProfile(string name, string? nameAr, string? email, string? phone, string? nationalId)
    {
        EmployeeNameRules.EnsureTripleName(name);
        EmployeeNameRules.EnsureOptionalTripleName(nameAr);
        Name = name.Trim();
        NameAr = nameAr?.Trim();
        Email = email;
        PhoneNumber = phone;
        NationalId = nationalId;
    }

    public void AssignOrganization(Guid? branchId, Guid? departmentId, Guid? positionId)
    {
        BranchId = branchId;
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    public void LinkUser(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty.");
        UserId = userId;
    }

    public void MarkAsOnLeave()
    {
        if (Status == EmploymentStatus.Terminated)
            throw new InvalidOperationException("Cannot put a terminated employee on leave.");
        Status = EmploymentStatus.OnLeave;
    }

    public void ReturnFromLeave()
    {
        if (Status != EmploymentStatus.OnLeave)
            throw new InvalidOperationException("Employee is not on leave.");
        Status = EmploymentStatus.Active;
    }

    public void Terminate(string reason)
    {
        if (Status == EmploymentStatus.Terminated)
            throw new InvalidOperationException("Employee already terminated.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Termination reason required.", nameof(reason));
        Status = EmploymentStatus.Terminated;
        RaiseDomainEvent(new EmployeeTerminatedEvent(Id, TenantId, reason));
    }
}
