using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Reporting;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

public sealed class RestaurantReportingSeeder : IDataSeeder
{
    private readonly ILogger<RestaurantReportingSeeder> _logger;

    public RestaurantReportingSeeder(ILogger<RestaurantReportingSeeder> logger) => _logger = logger;

    public int Order => 55;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        await SeedDashboardsAsync(tenantId, context, ct);
        await SeedReportsAsync(tenantId, context, ct);
        _logger.LogInformation("Reporting templates seeded for tenant {TenantId}", tenantId);
    }

    private static async Task SeedDashboardsAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        if (await context.ReportingDashboards.AnyAsync(d => d.TenantId == tenantId, ct))
        {
            return;
        }

        var dashboards = new (string Name, string Description, bool IsDefault)[]
        {
            ("Executive Dashboard", "High-level KPIs and trends", true),
            ("Sales Dashboard", "Sales performance and channels", false),
            ("Inventory Dashboard", "Stock levels and movements", false),
            ("Finance Dashboard", "Revenue, expenses and margins", false),
            ("HR Dashboard", "Attendance, leave and payroll", false)
        };

        foreach (var (name, description, isDefault) in dashboards)
        {
            var dashboard = Dashboard.Create(tenantId, name, description: description, isPublic: true);
            dashboard.SetDefault(isDefault);
            dashboard.AddWidget(WidgetType.KpiCard, "Today Sales", 0, 4, 2);
            dashboard.AddWidget(WidgetType.LineChart, "Trend", 1, 8, 4);
            context.ReportingDashboards.Add(dashboard);
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedReportsAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        if (await context.ReportDefinitions.AnyAsync(r => r.TenantId == tenantId, ct))
        {
            return;
        }

        var reports = new (string Name, string Code, ReportModule Module, ReportCategory Category)[]
        {
            ("Daily Sales Summary", "RPT-SALES-DAILY", ReportModule.Sales, ReportCategory.Operational),
            ("Inventory Valuation", "RPT-INV-VAL", ReportModule.Inventory, ReportCategory.Financial),
            ("Trial Balance", "RPT-FIN-TB", ReportModule.Finance, ReportCategory.Compliance),
            ("Payroll Summary", "RPT-HR-PAY", ReportModule.HR, ReportCategory.Operational),
            ("Executive KPI Pack", "RPT-EXEC-KPI", ReportModule.General, ReportCategory.Executive)
        };

        foreach (var (name, code, module, category) in reports)
        {
            context.ReportDefinitions.Add(ReportDefinition.Create(
                tenantId, name, code, module, category, dataSource: code));
        }

        await context.SaveChangesAsync(ct);
    }
}
