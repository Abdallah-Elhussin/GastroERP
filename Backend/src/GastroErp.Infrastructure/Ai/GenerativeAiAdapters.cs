using GastroErp.Application.Common.Interfaces.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace GastroErp.Infrastructure.Ai;

public sealed class AiOptions
{
    public const string SectionName = "Ai";
    public string Provider { get; set; } = "Heuristic";
    public string? OpenAiApiKey { get; set; }
    public string? OpenAiModel { get; set; } = "gpt-4o-mini";
    public string? AzureOpenAiEndpoint { get; set; }
    public string? AzureOpenAiDeployment { get; set; }
}

public sealed class HeuristicGenerativeAiAdapter : IGenerativeAiAdapter
{
    public Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        var reply = BuildHeuristicReply(systemPrompt, userPrompt);
        return Task.FromResult(reply);
    }

    public async IAsyncEnumerable<string> GenerateStreamAsync(
        string systemPrompt, string userPrompt,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var reply = BuildHeuristicReply(systemPrompt, userPrompt);
        foreach (var word in reply.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            ct.ThrowIfCancellationRequested();
            yield return word + " ";
            await Task.Delay(15, ct);
        }
    }

    private static string BuildHeuristicReply(string systemPrompt, string userPrompt)
    {
        var isArabic = systemPrompt.Contains("GastroERP", StringComparison.OrdinalIgnoreCase) is false
            && (systemPrompt.Contains("مساعد") || systemPrompt.Contains("محلل") || systemPrompt.Contains("استعلام"));

        if (userPrompt.Contains("Facts:", StringComparison.OrdinalIgnoreCase))
        {
            var facts = userPrompt.Split("Question:", 2, StringSplitOptions.None);
            var factLine = facts[0].Replace("Facts:", "", StringComparison.OrdinalIgnoreCase).Trim();
            return isArabic
                ? $"بناءً على البيانات المتاحة: {factLine}"
                : $"Based on available data: {factLine}";
        }

        if (userPrompt.Contains("KPI context:", StringComparison.OrdinalIgnoreCase) ||
            userPrompt.Contains("سياق KPI:", StringComparison.OrdinalIgnoreCase))
        {
            var lines = userPrompt.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var kpi = lines.FirstOrDefault(l => l.StartsWith('-')) ?? lines.LastOrDefault() ?? "";
            return isArabic
                ? $"إليك ملخص سريع من لوحة التحكم:\n{kpi}"
                : $"Here is a quick summary from your dashboard:\n{kpi}";
        }

        if (userPrompt.Contains("ALERT:", StringComparison.OrdinalIgnoreCase))
        {
            var alert = userPrompt.Split('\n').FirstOrDefault(l => l.Contains("ALERT:", StringComparison.OrdinalIgnoreCase)) ?? "";
            return isArabic
                ? $"ملخص الرؤى: {alert.Replace("ALERT:", "").Trim()}"
                : $"Insights summary: {alert.Replace("ALERT:", "").Trim()}";
        }

        return isArabic
            ? "تمت معالجة طلبك باستخدام محرك GastroERP الذكي (Heuristic)."
            : "Your request was processed using the GastroERP heuristic AI engine.";
    }
}

public sealed class OpenAiGenerativeAiAdapter : IGenerativeAiAdapter
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AiOptions _options;
    private readonly ILogger<OpenAiGenerativeAiAdapter> _logger;
    private readonly HeuristicGenerativeAiAdapter _fallback = new();

    public OpenAiGenerativeAiAdapter(
        IHttpClientFactory httpClientFactory, IOptions<AiOptions> options,
        ILogger<OpenAiGenerativeAiAdapter> logger)
        => (_httpClientFactory, _options, _logger) = (httpClientFactory, options.Value, logger);

    public async Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.OpenAiApiKey))
            return await _fallback.GenerateAsync(systemPrompt, userPrompt, ct);

        try
        {
            var client = _httpClientFactory.CreateClient("OpenAi");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.OpenAiApiKey);

            var payload = new
            {
                model = _options.OpenAiModel ?? "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            using var response = await client.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", payload, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenAI call failed with {StatusCode}", response.StatusCode);
                return await _fallback.GenerateAsync(systemPrompt, userPrompt, ct);
            }

            var doc = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: ct);
            return doc.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()
                   ?? await _fallback.GenerateAsync(systemPrompt, userPrompt, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OpenAI adapter failed, using heuristic fallback");
            return await _fallback.GenerateAsync(systemPrompt, userPrompt, ct);
        }
    }

    public IAsyncEnumerable<string> GenerateStreamAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
        => _fallback.GenerateStreamAsync(systemPrompt, userPrompt, ct);
}
