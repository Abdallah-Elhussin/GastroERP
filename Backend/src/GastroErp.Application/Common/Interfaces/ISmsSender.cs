using System.Threading;
using System.Threading.Tasks;

namespace GastroErp.Application.Common.Interfaces;

public interface ISmsSender
{
    Task SendSmsAsync(string to, string message, CancellationToken cancellationToken = default);
}
