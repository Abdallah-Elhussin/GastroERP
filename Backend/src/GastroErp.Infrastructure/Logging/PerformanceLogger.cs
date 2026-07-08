using GastroErp.Application.Common.Interfaces.Logging;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Logging;

public class PerformanceLogger : IPerformanceLogger
{
    private readonly ILogger<PerformanceLogger> _logger;

    public PerformanceLogger(ILogger<PerformanceLogger> logger)
    {
        _logger = logger;
    }

    public void LogSlowQuery(string queryName, long elapsedMilliseconds)
    {
        _logger.LogWarning("PERFORMANCE SLOW QUERY: {QueryName} took {ElapsedMilliseconds} ms", queryName, elapsedMilliseconds);
    }

    public void LogSlowRequest(string requestName, long elapsedMilliseconds)
    {
        _logger.LogWarning("PERFORMANCE SLOW REQUEST: {RequestName} took {ElapsedMilliseconds} ms", requestName, elapsedMilliseconds);
    }
}
