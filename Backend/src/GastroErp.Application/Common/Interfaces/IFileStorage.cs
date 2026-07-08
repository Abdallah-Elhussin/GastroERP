using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GastroErp.Application.Common.Interfaces;

public interface IFileStorage
{
    Task<string> UploadAsync(string fileName, Stream fileStream, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
}
