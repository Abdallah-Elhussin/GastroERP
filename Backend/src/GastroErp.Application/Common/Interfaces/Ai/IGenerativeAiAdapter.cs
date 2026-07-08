namespace GastroErp.Application.Common.Interfaces.Ai;

public interface IGenerativeAiAdapter
{
    Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);
    IAsyncEnumerable<string> GenerateStreamAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);
}
