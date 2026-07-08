using GastroErp.Application.Common.Notifications;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Ai;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Ai.EventHandlers;

public sealed class FraudDetectedEventHandler : INotificationHandler<DomainEventNotification<FraudDetectedEvent>>
{
    private readonly ILogger<FraudDetectedEventHandler> _logger;
    public FraudDetectedEventHandler(ILogger<FraudDetectedEventHandler> logger) => _logger = logger;

    public Task Handle(DomainEventNotification<FraudDetectedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _logger.LogInformation("Fraud detected: {Type} score={Score} tenant={TenantId}",
            evt.AlertType, evt.RiskScore, evt.TenantId);
        return Task.CompletedTask;
    }
}

public sealed class CustomerSegmentUpdatedEventHandler : INotificationHandler<DomainEventNotification<CustomerSegmentUpdatedEvent>>
{
    private readonly ILogger<CustomerSegmentUpdatedEventHandler> _logger;
    public CustomerSegmentUpdatedEventHandler(ILogger<CustomerSegmentUpdatedEventHandler> logger) => _logger = logger;

    public Task Handle(DomainEventNotification<CustomerSegmentUpdatedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _logger.LogInformation("Customer segment updated: customer={CustomerId} segment={Segment}",
            evt.CustomerId, evt.Segment);
        return Task.CompletedTask;
    }
}

public sealed class ChurnPredictedEventHandler : INotificationHandler<DomainEventNotification<ChurnPredictedEvent>>
{
    private readonly ILogger<ChurnPredictedEventHandler> _logger;
    public ChurnPredictedEventHandler(ILogger<ChurnPredictedEventHandler> logger) => _logger = logger;

    public Task Handle(DomainEventNotification<ChurnPredictedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        if (evt.RiskLevel >= ChurnRiskLevel.High)
        {
            _logger.LogWarning("High churn risk: customer={CustomerId} probability={Probability}",
                evt.CustomerId, evt.ChurnProbability);
        }
        return Task.CompletedTask;
    }
}

public sealed class RecommendationGeneratedEventHandler : INotificationHandler<DomainEventNotification<RecommendationGeneratedEvent>>
{
    private readonly ILogger<RecommendationGeneratedEventHandler> _logger;
    public RecommendationGeneratedEventHandler(ILogger<RecommendationGeneratedEventHandler> logger) => _logger = logger;

    public Task Handle(DomainEventNotification<RecommendationGeneratedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _logger.LogDebug("Product recommendation generated: product={ProductId} type={Type}",
            evt.ProductId, evt.RecommendationType);
        return Task.CompletedTask;
    }
}
