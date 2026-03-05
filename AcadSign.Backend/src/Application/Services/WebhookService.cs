using Hangfire;
using Microsoft.Extensions.Logging;
using AcadSign.Backend.Domain.Entities;
using System.Text.Json;

namespace AcadSign.Backend.Application.Services;

public class WebhookService
{
    private readonly IWebhookRepository _webhookRepo;
    private readonly ILogger<WebhookService> _logger;
    
    public WebhookService(
        IWebhookRepository webhookRepo,
        ILogger<WebhookService> logger)
    {
        _webhookRepo = webhookRepo;
        _logger = logger;
    }
    
    public async Task TriggerWebhookAsync(string eventType, object payload)
    {
        var subscriptions = await _webhookRepo.GetActiveSubscriptionsByEventAsync(eventType);
        
        _logger.LogInformation("Triggering webhook for event {EventType} to {Count} subscriptions", 
            eventType, subscriptions.Count);
        
        foreach (var subscription in subscriptions)
        {
            await DeliverWebhookAsync(subscription, eventType, payload);
        }
    }
    
    private async Task DeliverWebhookAsync(
        WebhookSubscription subscription, 
        string eventType, 
        object payload)
    {
        var delivery = new WebhookDelivery
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            EventType = eventType,
            Payload = JsonSerializer.SerializeToDocument(payload),
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };
        
        await _webhookRepo.AddDeliveryAsync(delivery);
        
        BackgroundJob.Enqueue<WebhookDeliveryJob>(
            x => x.DeliverAsync(delivery.Id));
        
        _logger.LogInformation("Webhook delivery {DeliveryId} enqueued for subscription {SubscriptionId}", 
            delivery.Id, subscription.Id);
    }
}
