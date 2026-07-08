using GastroErp.Application.Features.Hr.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GastroErp.Infrastructure.Health;

public sealed class HrWorkforceHealthCheck : IHealthCheck
{
    private readonly IEmployeeManagementService _employees;
    private readonly IPayrollService _payroll;
    private readonly IHrDashboardService _dashboard;

    public HrWorkforceHealthCheck(
        IEmployeeManagementService employees, IPayrollService payroll, IHrDashboardService dashboard)
        => (_employees, _payroll, _dashboard) = (employees, payroll, dashboard);

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var ready = _employees is not null && _payroll is not null && _dashboard is not null;
        return Task.FromResult(ready
            ? HealthCheckResult.Healthy("HR workforce services registered")
            : HealthCheckResult.Unhealthy("HR workforce services not available"));
    }
}
