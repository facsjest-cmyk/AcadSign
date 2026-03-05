using System.Text.Json;

namespace AcadSign.Backend.Domain.Entities;

public class WebhookDelivery
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public WebhookSubscription Subscription { get; set; } = null!;
    public string EventType { get; set; } = string.Empty;
    public JsonDocument? Payload { get; set; }
    public int? ResponseStatus { get; set; }
    public string? ResponseBody { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
