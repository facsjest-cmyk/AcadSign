using AcadSign.Backend.Domain.Entities;

namespace AcadSign.Backend.Application.Services;

public interface IWebhookRepository
{
    Task<List<WebhookSubscription>> GetActiveSubscriptionsByEventAsync(string eventType);
    Task<WebhookSubscription?> GetSubscriptionByIdAsync(Guid id);
    Task AddAsync(WebhookSubscription subscription);
    Task<WebhookDelivery?> GetDeliveryByIdAsync(Guid id);
    Task AddDeliveryAsync(WebhookDelivery delivery);
    Task UpdateDeliveryAsync(WebhookDelivery delivery);
}
