using GastroErp.Application.Common.Interfaces.Logging;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Logging;

public class BusinessLogger : IBusinessLogger
{
    private readonly ILogger<BusinessLogger> _logger;

    public BusinessLogger(ILogger<BusinessLogger> logger)
    {
        _logger = logger;
    }

    public void LogImportantAction(string actionName, string userId, string details)
    {
        _logger.LogInformation("BUSINESS ACTION: {ActionName} by User {UserId}. Details: {Details}", actionName, userId, details);
    }
}
