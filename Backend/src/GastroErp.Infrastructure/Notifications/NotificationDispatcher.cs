using GastroErp.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Notifications;

/// <summary>
/// خدمة إرسال الإشعارات الشاملة (للنظام، Push، In-App)
/// </summary>
public class NotificationDispatcher : INotificationService
{
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(ILogger<NotificationDispatcher> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string message, string? userId = null, CancellationToken cancellationToken = default)
    {
        // مستقبلا يمكن ربطه مع SignalR لرسائل In-App
        // أو Firebase للـ Push Notifications
        _logger.LogInformation("Notification Sent: {Message} to User: {UserId}", message, userId ?? "All");
        return Task.CompletedTask;
    }
}
