using GastroErp.Application.Features.Automation.Services;
using GastroErp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Integrations;

public sealed class MyFatoorahAdapter : IPaymentGatewayAdapter
{
    private readonly ILogger<MyFatoorahAdapter> _logger;
    public MyFatoorahAdapter(ILogger<MyFatoorahAdapter> logger) => _logger = logger;
    public IntegrationProviderName Provider => IntegrationProviderName.MyFatoorah;

    public Task<bool> TestConnectionAsync(string settingsJson, CancellationToken ct = default)
    {
        _logger.LogInformation("Testing MyFatoorah connection (stub)");
        return Task.FromResult(!string.IsNullOrWhiteSpace(settingsJson));
    }
}

public sealed class HyperPayAdapter : IPaymentGatewayAdapter
{
    private readonly ILogger<HyperPayAdapter> _logger;
    public HyperPayAdapter(ILogger<HyperPayAdapter> logger) => _logger = logger;
    public IntegrationProviderName Provider => IntegrationProviderName.HyperPay;

    public Task<bool> TestConnectionAsync(string settingsJson, CancellationToken ct = default)
    {
        _logger.LogInformation("Testing HyperPay connection (stub)");
        return Task.FromResult(!string.IsNullOrWhiteSpace(settingsJson));
    }
}

public sealed class MoyasarAdapter : IPaymentGatewayAdapter
{
    private readonly ILogger<MoyasarAdapter> _logger;
    public MoyasarAdapter(ILogger<MoyasarAdapter> logger) => _logger = logger;
    public IntegrationProviderName Provider => IntegrationProviderName.Moyasar;

    public Task<bool> TestConnectionAsync(string settingsJson, CancellationToken ct = default)
    {
        _logger.LogInformation("Testing Moyasar connection (stub)");
        return Task.FromResult(!string.IsNullOrWhiteSpace(settingsJson));
    }
}

public sealed class StripeAdapter : IPaymentGatewayAdapter
{
    private readonly ILogger<StripeAdapter> _logger;
    public StripeAdapter(ILogger<StripeAdapter> logger) => _logger = logger;
    public IntegrationProviderName Provider => IntegrationProviderName.Stripe;

    public Task<bool> TestConnectionAsync(string settingsJson, CancellationToken ct = default)
    {
        _logger.LogInformation("Testing Stripe connection (stub)");
        return Task.FromResult(!string.IsNullOrWhiteSpace(settingsJson));
    }
}
