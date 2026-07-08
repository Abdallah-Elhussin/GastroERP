using System.Net;
using System.Net.Mail;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GastroErp.Infrastructure.Notifications;

/// <summary>
/// خدمة إرسال البريد الإلكتروني عبر SMTP
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> smtpOptions, ILogger<SmtpEmailSender> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
            {
                Credentials = new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password),
                EnableSsl = _smtpOptions.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpOptions.FromEmail, _smtpOptions.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage, cancellationToken);
            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw; // Or handle based on requirements
        }
    }
}
