namespace GastroErp.Infrastructure.Options;

/// <summary>
/// إعدادات ذاكرة التخزين المؤقت (Cache Options)
/// </summary>
public class CacheOptions
{
    public const string SectionName = "Cache";

    public string Provider { get; set; } = "Memory"; // Memory, Redis
    public string RedisConnectionString { get; set; } = string.Empty;
    public int DefaultExpirationMinutes { get; set; } = 60;
}
