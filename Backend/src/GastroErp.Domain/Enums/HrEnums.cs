namespace GastroErp.Domain.Enums;

public enum AttendanceStatus
{
    CheckedIn = 1,
    OnBreak = 2,
    CheckedOut = 3,
    Absent = 4,
    Late = 5
}

public enum LeaveType
{
    Annual = 1,
    Sick = 2,
    Emergency = 3,
    Unpaid = 4
}

public enum LeaveRequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}

public enum ScheduleRole
{
    General = 1,
    Kitchen = 2,
    Waiter = 3,
    Driver = 4,
    Cashier = 5
}

public enum PayrollRunStatus
{
    Draft = 1,
    Calculated = 2,
    Approved = 3,
    Posted = 4,
    Cancelled = 5
}

public enum PayslipStatus
{
    Draft = 1,
    Finalized = 2,
    Paid = 3
}

public enum PayComponentType
{
    BaseSalary = 1,
    Allowance = 2,
    Deduction = 3,
    Overtime = 4,
    Bonus = 5
}

public enum PerformanceRecordType
{
    Kpi = 1,
    Evaluation = 2,
    Promotion = 3,
    Warning = 4,
    Reward = 5
}

public enum ApplicantStatus
{
    Applied = 1,
    Screening = 2,
    Interview = 3,
    Offered = 4,
    Hired = 5,
    Rejected = 6
}

public enum InterviewStatus
{
    Scheduled = 1,
    Completed = 2,
    Cancelled = 3,
    NoShow = 4
}

public enum TrainingStatus
{
    Planned = 1,
    InProgress = 2,
    Completed = 3,
    Expired = 4
}

public enum ContractType
{
    FullTime = 1,
    PartTime = 2,
    Temporary = 3,
    Probation = 4
}

public enum HrWorkflowRequestType
{
    Overtime = 1,
    Loan = 2,
    SalaryAdvance = 3,
    Resignation = 4,
    Promotion = 5,
    Transfer = 6
}

public enum HrWorkflowRequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}
