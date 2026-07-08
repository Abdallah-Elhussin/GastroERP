namespace GastroErp.Domain.Enums;

public enum NotificationChannel
{
    InApp = 1,
    Email = 2,
    Sms = 3,
    Push = 4,
    WhatsApp = 5
}

public enum NotificationType
{
    OrderCreated = 1,
    OrderReady = 2,
    OrderDelivered = 3,
    OrderCancelled = 4,
    LowStock = 5,
    ItemExpired = 6,
    PurchaseApproved = 7,
    MembershipExpiring = 8,
    LoyaltyReward = 9,
    CouponReminder = 10,
    FiscalPeriodClosed = 11,
    JournalPosted = 12,
    PaymentReceived = 13,
    HrWelcome = 14,
    HrLeaveRequested = 15,
    HrLeaveApproved = 16,
    HrLeaveRejected = 17,
    HrPayslipReady = 18,
    HrPayrollPosted = 19,
    HrTrainingAssigned = 20,
    HrCertificationExpired = 21,
    HrContractExpiry = 22,
    WorkflowStarted = 23,
    ApprovalRequested = 24,
    ApprovalApproved = 25,
    ApprovalRejected = 26,
    WorkflowCompleted = 27,
    WorkflowEscalated = 28,
    DelegationAssigned = 29,
    WorkflowAssigned = 30,
    WorkflowCancelled = 31,
    WorkflowReturned = 32,
    ReportReady = 33,
    ScheduledReportCompleted = 34,
    ScheduledReportFailed = 35,
    KpiThresholdExceeded = 36,
    DashboardShared = 37,
    System = 99
}

public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
    Read = 4,
    Archived = 5
}

public enum JobQueue
{
    Notifications = 1,
    Accounting = 2,
    Reporting = 3,
    Inventory = 4,
    Email = 5,
    Sms = 6,
    System = 7
}

public enum JobExecutionStatus
{
    Queued = 1,
    Running = 2,
    Succeeded = 3,
    Failed = 4,
    DeadLetter = 5,
    Cancelled = 6
}

public enum IntegrationProviderType
{
    PaymentGateway = 1,
    Sms = 2,
    Email = 3,
    Storage = 4,
    Maps = 5,
    Delivery = 6,
    AccountingExport = 7
}

public enum IntegrationProviderName
{
    Stripe = 1,
    MyFatoorah = 2,
    HyperPay = 3,
    Moyasar = 4,
    Twilio = 10,
    Unifonic = 11,
    SendGrid = 20,
    Smtp = 21,
    AzureBlob = 30,
    AwsS3 = 31,
    GoogleMaps = 40,
    MapBox = 41,
    Jahez = 50,
    HungerStation = 51,
    Mrsool = 52,
    Internal = 99
}
