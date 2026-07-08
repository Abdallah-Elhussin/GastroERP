using FluentValidation;
using GastroErp.Application.Features.Automation.Commands;
using GastroErp.Application.Features.Automation.DTOs;

namespace GastroErp.Application.Features.Automation.Validators;

public sealed class ExecuteJobValidator : AbstractValidator<ExecuteJobCommand>
{
    private static readonly HashSet<string> AllowedJobs =
    [
        "AutoCloseFiscalPeriod", "LowStockCheck", "LoyaltyPointsExpiry",
        "CancelExpiredOrders", "SyncDeliveryStatus", "KitchenDelayedAlert", "CacheCleanup",
        "AiWarehouseSync", "AiFeatureCompute", "AiDataQualityCheck", "AiForecastRefresh", "AiRecommendationRefresh",
        "AiIntelligenceFraudAnalysis", "AiIntelligenceSegmentation", "AiIntelligenceChurnPrediction",
        "AiIntelligenceProductRecommendations",
        "HrAttendanceSummary", "HrLeaveBalanceUpdate", "HrMissingAttendanceReport", "HrOvertimeSummary",
        "HrPayrollGeneration", "HrPayrollPostingReminder", "HrPerformanceReminder",
        "HrContractExpiry", "HrProbationExpiry", "HrCertificationExpiry",
        "WorkflowEscalationJob", "WorkflowReminderJob", "WorkflowCleanupJob", "DelegationExpiryJob",
        "WorkflowRetryJob", "WorkflowTimeoutJob"
    ];

    public ExecuteJobValidator()
    {
        RuleFor(x => x.Dto.JobName).NotEmpty().MaximumLength(100)
            .Must(n => AllowedJobs.Contains(n)).WithMessage("Unknown job name.");
    }
}

public sealed class SendNotificationValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationValidator()
    {
        RuleFor(x => x.Dto.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Body).NotEmpty().MaximumLength(2000);
    }
}

public sealed class UpsertIntegrationValidator : AbstractValidator<UpsertIntegrationCommand>
{
    public UpsertIntegrationValidator()
    {
        RuleFor(x => x.Dto.SettingsJson).NotEmpty();
    }
}

public sealed class TestIntegrationValidator : AbstractValidator<TestIntegrationCommand>
{
    public TestIntegrationValidator()
    {
        RuleFor(x => x.Dto.ProviderType).IsInEnum();
        RuleFor(x => x.Dto.ProviderName).IsInEnum();
    }
}
