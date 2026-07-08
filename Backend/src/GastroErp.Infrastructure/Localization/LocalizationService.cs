using System.Globalization;
using System.Text.Json;
using GastroErp.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Localization;

/// <summary>
/// خدمة الترجمة (Localization Service)
/// تعتمد على قراءة ملفات JSON (ar.json / en.json)
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly ILogger<LocalizationService> _logger;
    private static readonly Dictionary<string, Dictionary<string, string>> _cache = new();
    private static readonly object _lock = new();

    public LocalizationService(ILogger<LocalizationService> logger)
    {
        _logger = logger;
        LoadResources();
    }

    private void LoadResources()
    {
        if (_cache.Count > 0) return;

        lock (_lock)
        {
            if (_cache.Count > 0) return;

            var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
                // Create dummy files for fallback
                File.WriteAllText(Path.Combine(basePath, "ar.json"), "{}");
                File.WriteAllText(Path.Combine(basePath, "en.json"), "{}");
            }

            var cultures = new[] { "ar", "en" };
            foreach (var culture in cultures)
            {
                var filePath = Path.Combine(basePath, $"{culture}.json");
                if (File.Exists(filePath))
                {
                    try
                    {
                        var json = File.ReadAllText(filePath);
                        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                        _cache[culture] = dict ?? new Dictionary<string, string>();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to load localization file for {Culture}", culture);
                        _cache[culture] = new Dictionary<string, string>();
                    }
                }
                else
                {
                    _cache[culture] = new Dictionary<string, string>();
                }
            }
        }
    }

    public string GetMessage(string key, params object[] args)
    {
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        
        // Default to English if culture not found
        if (!_cache.ContainsKey(culture))
        {
            culture = "en";
        }

        var dict = _cache.GetValueOrDefault(culture);
        if (dict != null && dict.TryGetValue(key, out var message))
        {
            if (args != null && args.Length > 0)
            {
                try
                {
                    return string.Format(message, args);
                }
                catch
                {
                    return message; // Fallback to raw message if string format fails
                }
            }
            return message;
        }

        // Fallback to key itself
        return key;
    }
}
