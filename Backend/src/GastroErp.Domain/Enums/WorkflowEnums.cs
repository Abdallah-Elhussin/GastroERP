namespace GastroErp.Domain.Enums;

public enum WorkflowStatus
{
    Draft = 1,
    Active = 2,
    InProgress = 3,
    Approved = 4,
    Rejected = 5,
    Cancelled = 6,
    Completed = 7
}

public enum ApprovalStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Delegated = 4,
    Escalated = 5,
    Skipped = 6
}

public enum ApprovalDecision
{
    Approve = 1,
    Reject = 2,
    Delegate = 3,
    Skip = 4
}

public enum WorkflowModule
{
    General = 1,
    HR = 2,
    Finance = 3,
    Purchasing = 4,
    CRM = 5,
    Sales = 6,
    Inventory = 7
}

public enum WorkflowTrigger
{
    Manual = 1,
    OnCreate = 2,
    OnSubmit = 3,
    Scheduled = 4
}

public enum WorkflowPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

public enum ApprovalType
{
    AnyOne = 1,
    All = 2,
    Manager = 3,
    RoleBased = 4
}

public enum WorkflowConditionOperator
{
    Equals = 1,
    NotEquals = 2,
    GreaterThan = 3,
    GreaterThanOrEqual = 4,
    LessThan = 5,
    LessThanOrEqual = 6,
    Contains = 7
}
