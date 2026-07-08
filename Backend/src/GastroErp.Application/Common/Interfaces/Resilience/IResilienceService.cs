namespace GastroErp.Application.Common.Interfaces.Resilience;

public interface IResilienceService
{
    Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, int maxRetries = 3);
    Task<T> ExecuteWithCircuitBreakerAsync<T>(Func<Task<T>> action);
    Task<T> ExecuteWithTimeoutAsync<T>(Func<Task<T>> action, TimeSpan timeout);
    Task<T> ExecuteWithFallbackAsync<T>(Func<Task<T>> action, Func<Task<T>> fallback);
}
