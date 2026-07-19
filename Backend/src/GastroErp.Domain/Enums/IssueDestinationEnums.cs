namespace GastroErp.Domain.Enums;

/// <summary>نوع جهة الصرف.</summary>
public enum IssueDestinationType : byte
{
    Kitchen = 1,
    Production = 2,
    Branch = 3,
    Administration = 4,
    Marketing = 5,
    Maintenance = 6,
    Waste = 7,
    StaffMeals = 8,
    Complimentary = 9,
    Assets = 10,
    Project = 11,
    CostCenter = 12,
    Expense = 13,
    Other = 99
}
