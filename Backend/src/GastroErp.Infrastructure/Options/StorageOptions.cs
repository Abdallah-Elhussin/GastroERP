namespace GastroErp.Infrastructure.Options;

/// <summary>
/// إعدادات التخزين (Storage Options)
/// </summary>
public class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "Local"; // Local, MinIo, AwsS3, AzureBlob
    public string BasePath { get; set; } = "Uploads";
    
    // AWS / MinIO
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    
    // Azure
    public string ConnectionString { get; set; } = string.Empty;
}
