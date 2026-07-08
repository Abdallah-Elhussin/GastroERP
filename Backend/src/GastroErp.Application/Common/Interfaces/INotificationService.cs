using System.Threading;
using System.Threading.Tasks;

namespace GastroErp.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendAsync(string message, string? userId = null, CancellationToken cancellationToken = default);
}
