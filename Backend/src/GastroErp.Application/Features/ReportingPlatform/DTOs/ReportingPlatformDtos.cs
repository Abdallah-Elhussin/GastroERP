using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.ReportingPlatform.DTOs;

public record DashboardWidgetDto(
    Guid Id, WidgetType WidgetType, string Title, int Position, int Width, int Height, string? ConfigurationJson);

public record DashboardDto(
    Guid Id, string Name, string? Description, bool IsDefault, bool IsPublic, bool IsFavorite,
    string? LayoutJson, Guid? OwnerUserId, IReadOnlyList<DashboardWidgetDto> Widgets);

public record CreateDashboardDto(
    string Name, string? Description = null, bool IsPublic = false, string? LayoutJson = null,
    IReadOnlyList<CreateDashboardWidgetDto>? Widgets = null);

public record UpdateDashboardDto(
    string Name, string? Description, bool IsPublic, string? LayoutJson,
    IReadOnlyList<CreateDashboardWidgetDto>? Widgets = null);

public record CreateDashboardWidgetDto(
    WidgetType WidgetType, string Title, int Position, int Width, int Height, string? ConfigurationJson = null);

public record ShareDashboardDto(bool IsPublic, Guid? ShareWithUserId = null);

public record ReportDefinitionDto(
    Guid Id, string Name, string Code, ReportModule Module, ReportCategory Category,
    string DataSource, string? QueryDefinition, string? ParametersJson, bool IsPublished);

public record CreateReportDefinitionDto(
    string Name, string Code, ReportModule Module, ReportCategory Category,
    string DataSource, string? QueryDefinition = null, string? ParametersJson = null);

public record UpdateReportDefinitionDto(
    string Name, ReportCategory Category, string DataSource,
    string? QueryDefinition = null, string? ParametersJson = null);

public record ExecuteReportDto(
    Guid? ReportDefinitionId = null, string? DataSource = null,
    DateOnly? FromDate = null, DateOnly? ToDate = null, Guid? BranchId = null);

public record ReportExecutionDto(
    Guid Id, Guid ReportDefinitionId, Guid ExecutedBy, DateTimeOffset ExecutionDate,
    int DurationMs, ReportStatus Status, object? Result, string? ErrorMessage);

public record ReportExecutionHistoryDto(
    Guid Id, Guid ReportDefinitionId, string ReportName, DateTimeOffset ExecutionDate,
    ReportStatus Status, int DurationMs);

public record ScheduledReportDto(
    Guid Id, Guid ReportDefinitionId, ScheduleFrequency Frequency, string? CronExpression,
    ReportExportFormat ExportFormat, string? EmailRecipients, bool IsEnabled, DateTimeOffset? LastRunAt);

public record CreateScheduledReportDto(
    Guid ReportDefinitionId, ScheduleFrequency Frequency, ReportExportFormat ExportFormat,
    string? CronExpression = null, string? EmailRecipients = null);

public record UpdateScheduledReportDto(
    ScheduleFrequency Frequency, ReportExportFormat ExportFormat,
    string? CronExpression = null, string? EmailRecipients = null);

public record KpiDefinitionDto(
    Guid Id, string Name, string Code, string Formula, ReportModule Module,
    decimal? TargetValue, decimal? WarningValue, decimal? CriticalValue, bool IsActive);

public record CreateKpiDefinitionDto(
    string Name, string Code, string Formula, ReportModule Module,
    decimal? TargetValue = null, decimal? WarningValue = null, decimal? CriticalValue = null);

public record UpdateKpiDefinitionDto(
    string Name, string Formula, decimal? TargetValue, decimal? WarningValue, decimal? CriticalValue);

public record KpiValueDto(
    Guid KpiDefinitionId, string Name, string Code, decimal Value, KpiTrend Trend,
    decimal? TargetValue, string Status, DateOnly SnapshotDate);

public record KpiHistoryDto(DateOnly SnapshotDate, decimal Value, KpiTrend Trend);

public record ChartRequestDto(ChartType ChartType, string DataSource, DateOnly? FromDate, DateOnly? ToDate, Guid? BranchId);

public record ChartResultDto(ChartType ChartType, object ChartData);

public record PlatformExportRequestDto(
    Guid? ReportDefinitionId, string? ReportKey, ReportExportFormat Format,
    DateOnly? FromDate = null, DateOnly? ToDate = null, Guid? BranchId = null);

public record PlatformExportResultDto(byte[] Content, string ContentType, string FileName);

public record PowerBiWorkspaceConfigDto(string WorkspaceId, string DatasetId, bool IsConfigured);

public record PowerBiEmbedTokenDto(string Token, DateTimeOffset ExpiresAt, string EmbedUrl);

public record ReportingPlatformFilterDto(string? Search = null, ReportModule? Module = null, int Page = 1, int PageSize = 50);
