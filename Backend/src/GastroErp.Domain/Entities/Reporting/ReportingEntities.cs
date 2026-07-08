using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Reporting;

namespace GastroErp.Domain.Entities.Reporting;

public sealed class Dashboard : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? OwnerUserId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsPublic { get; private set; }
    public bool IsFavorite { get; private set; }
    public string? LayoutJson { get; private set; }

    private readonly List<DashboardWidget> _widgets = [];
    public IReadOnlyCollection<DashboardWidget> Widgets => _widgets.AsReadOnly();

    private Dashboard() => Name = string.Empty;

    public static Dashboard Create(Guid tenantId, string name, Guid? ownerUserId = null,
        string? description = null, bool isPublic = false, string? layoutJson = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new BusinessException(ErrorCodes.RequiredField, "Name required.");
        var d = new Dashboard
        {
            TenantId = tenantId,
            OwnerUserId = ownerUserId,
            Name = name.Trim(),
            Description = description,
            IsPublic = isPublic,
            LayoutJson = layoutJson ?? "[]"
        };
        d.RaiseDomainEvent(new DashboardCreatedEvent(d.Id, tenantId, d.Name));
        return d;
    }

    public void Update(string name, string? description, bool isPublic, string? layoutJson)
    {
        Name = name.Trim();
        Description = description;
        IsPublic = isPublic;
        if (layoutJson is not null) LayoutJson = layoutJson;
        RaiseDomainEvent(new DashboardUpdatedEvent(Id, TenantId));
    }

    public void SetDefault(bool isDefault) => IsDefault = isDefault;
    public void SetFavorite(bool favorite) => IsFavorite = favorite;

    public DashboardWidget AddWidget(WidgetType type, string title, int position, int width, int height, string? configJson = null)
    {
        var w = DashboardWidget.Create(TenantId, Id, type, title, position, width, height, configJson);
        _widgets.Add(w);
        return w;
    }

    public void RemoveWidget(Guid widgetId)
    {
        var w = _widgets.FirstOrDefault(x => x.Id == widgetId)
            ?? throw new InvalidOperationException("Widget not found.");
        _widgets.Remove(w);
    }
}

public sealed class DashboardWidget : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid DashboardId { get; private set; }
    public WidgetType WidgetType { get; private set; }
    public string Title { get; private set; }
    public int Position { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public string? ConfigurationJson { get; private set; }

    private DashboardWidget() => Title = string.Empty;

    public static DashboardWidget Create(Guid tenantId, Guid dashboardId, WidgetType type, string title,
        int position, int width, int height, string? configJson = null)
        => new()
        {
            TenantId = tenantId,
            DashboardId = dashboardId,
            WidgetType = type,
            Title = title.Trim(),
            Position = position,
            Width = width,
            Height = height,
            ConfigurationJson = configJson
        };

    public void Update(string title, int position, int width, int height, string? configJson)
    {
        Title = title.Trim();
        Position = position;
        Width = width;
        Height = height;
        ConfigurationJson = configJson;
    }
}

public sealed class ReportDefinition : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public ReportModule Module { get; private set; }
    public ReportCategory Category { get; private set; }
    public string DataSource { get; private set; }
    public string? QueryDefinition { get; private set; }
    public string? ParametersJson { get; private set; }
    public bool IsPublished { get; private set; }

    private ReportDefinition()
    {
        Name = string.Empty;
        Code = string.Empty;
        DataSource = string.Empty;
    }

    public static ReportDefinition Create(Guid tenantId, string name, string code, ReportModule module,
        ReportCategory category, string dataSource, string? queryDefinition = null,
        string? parametersJson = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new BusinessException(ErrorCodes.RequiredField, "Name required.");
        if (string.IsNullOrWhiteSpace(code)) throw new BusinessException(ErrorCodes.RequiredField, "Code required.");
        return new ReportDefinition
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Module = module,
            Category = category,
            DataSource = dataSource.Trim(),
            QueryDefinition = queryDefinition,
            ParametersJson = parametersJson,
            IsPublished = false
        };
    }

    public void Update(string name, ReportCategory category, string dataSource, string? queryDefinition, string? parametersJson)
    {
        if (IsPublished) throw new InvalidOperationException("Published reports cannot be edited.");
        Name = name.Trim();
        Category = category;
        DataSource = dataSource;
        QueryDefinition = queryDefinition;
        ParametersJson = parametersJson;
    }

    public void Publish() => IsPublished = true;
    public void Unpublish() => IsPublished = false;
}

public sealed class ReportExecution : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid ReportDefinitionId { get; private set; }
    public Guid ExecutedBy { get; private set; }
    public DateTimeOffset ExecutionDate { get; private set; }
    public int DurationMs { get; private set; }
    public ReportStatus Status { get; private set; }
    public string? ResultJson { get; private set; }
    public string? ErrorMessage { get; private set; }

    private ReportExecution() { }

    public static ReportExecution Start(Guid tenantId, Guid reportDefinitionId, Guid executedBy)
        => new()
        {
            TenantId = tenantId,
            ReportDefinitionId = reportDefinitionId,
            ExecutedBy = executedBy,
            ExecutionDate = DateTimeOffset.UtcNow,
            Status = ReportStatus.Running
        };

    public void Complete(int durationMs, string? resultJson)
    {
        Status = ReportStatus.Completed;
        DurationMs = durationMs;
        ResultJson = resultJson;
        RaiseDomainEvent(new ReportGeneratedEvent(Id, TenantId, ReportDefinitionId, ExecutedBy));
    }

    public void Fail(int durationMs, string error)
    {
        Status = ReportStatus.Failed;
        DurationMs = durationMs;
        ErrorMessage = error;
    }
}

public sealed class ScheduledReport : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid ReportDefinitionId { get; private set; }
    public ScheduleFrequency Frequency { get; private set; }
    public string? CronExpression { get; private set; }
    public ReportExportFormat ExportFormat { get; private set; }
    public string? EmailRecipients { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset? LastRunAt { get; private set; }

    private ScheduledReport() { }

    public static ScheduledReport Create(Guid tenantId, Guid reportDefinitionId, ScheduleFrequency frequency,
        ReportExportFormat exportFormat, string? cronExpression = null, string? emailRecipients = null)
        => new()
        {
            TenantId = tenantId,
            ReportDefinitionId = reportDefinitionId,
            Frequency = frequency,
            CronExpression = cronExpression,
            ExportFormat = exportFormat,
            EmailRecipients = emailRecipients,
            IsEnabled = true
        };

    public void Update(ScheduleFrequency frequency, ReportExportFormat format, string? cron, string? recipients)
    {
        Frequency = frequency;
        ExportFormat = format;
        CronExpression = cron;
        EmailRecipients = recipients;
    }

    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;

    public void MarkRun(bool succeeded)
    {
        LastRunAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ScheduledReportExecutedEvent(Id, TenantId, succeeded));
    }
}

public sealed class KpiDefinition : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Formula { get; private set; }
    public ReportModule Module { get; private set; }
    public decimal? TargetValue { get; private set; }
    public decimal? WarningValue { get; private set; }
    public decimal? CriticalValue { get; private set; }
    public bool IsActive { get; private set; }

    private KpiDefinition()
    {
        Name = string.Empty;
        Code = string.Empty;
        Formula = string.Empty;
    }

    public static KpiDefinition Create(Guid tenantId, string name, string code, string formula, ReportModule module,
        decimal? target = null, decimal? warning = null, decimal? critical = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new BusinessException(ErrorCodes.RequiredField, "Name required.");
        if (string.IsNullOrWhiteSpace(code)) throw new BusinessException(ErrorCodes.RequiredField, "Code required.");
        return new KpiDefinition
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Formula = formula.Trim(),
            Module = module,
            TargetValue = target,
            WarningValue = warning,
            CriticalValue = critical,
            IsActive = true
        };
    }

    public void Update(string name, string formula, decimal? target, decimal? warning, decimal? critical)
    {
        Name = name.Trim();
        Formula = formula.Trim();
        TargetValue = target;
        WarningValue = warning;
        CriticalValue = critical;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

public sealed class KpiSnapshot : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid KpiDefinitionId { get; private set; }
    public decimal Value { get; private set; }
    public KpiTrend Trend { get; private set; }
    public DateOnly SnapshotDate { get; private set; }

    private KpiSnapshot() { }

    public static KpiSnapshot Record(Guid tenantId, Guid kpiDefinitionId, decimal value, KpiTrend trend, DateOnly date)
    {
        var snap = new KpiSnapshot
        {
            TenantId = tenantId,
            KpiDefinitionId = kpiDefinitionId,
            Value = value,
            Trend = trend,
            SnapshotDate = date
        };
        snap.RaiseDomainEvent(new KpiCalculatedEvent(kpiDefinitionId, tenantId, value, trend));
        return snap;
    }
}
