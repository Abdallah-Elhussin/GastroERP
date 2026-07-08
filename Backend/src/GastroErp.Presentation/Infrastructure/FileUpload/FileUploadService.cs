namespace GastroErp.Presentation.Infrastructure.FileUpload;

public interface IFileUploadService
{
    Task<string> UploadFileAsync(IFormFile file, string directory, Guid tenantId);
    Task DeleteFileAsync(string fileUrl);
}

public class LocalFileUploadService : IFileUploadService
{
    public Task<string> UploadFileAsync(IFormFile file, string directory, Guid tenantId)
    {
        // Passive structure
        return Task.FromResult($"/uploads/{tenantId}/{directory}/{file.FileName}");
    }

    public Task DeleteFileAsync(string fileUrl)
    {
        return Task.CompletedTask;
    }
}
