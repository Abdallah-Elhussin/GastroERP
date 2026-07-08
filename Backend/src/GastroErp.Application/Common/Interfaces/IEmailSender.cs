using System.Threading;
using System.Threading.Tasks;

namespace GastroErp.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
