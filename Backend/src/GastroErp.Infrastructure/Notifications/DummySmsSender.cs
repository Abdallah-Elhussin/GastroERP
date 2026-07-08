using GastroErp.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Notifications;

/// <summary>
/// خدمة إرسال الرسائل النصية (مبدئية)
/// </summary>
public class DummySmsSender : ISmsSender
{
    private readonly ILogger<DummySmsSender> _logger;

    public DummySmsSender(ILogger<DummySmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Dummy SMS sent to {PhoneNumber}: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }
}
