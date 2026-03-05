using Hangfire;
using Microsoft.Extensions.Logging;
using AcadSign.Backend.Application.Services;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AcadSign.Backend.Application.BackgroundJobs;

public class WebhookDeliveryJob
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebhookRepository _webhookRepo;
    private readonly ILogger<WebhookDeliveryJob> _logger;
    
    public WebhookDeliveryJob(
        IHttpClientFactory httpClientFactory,
        IWebhookRepository webhookRepo,
        ILogger<WebhookDeliveryJob> logger)
    {
        _httpClientFactory = httpClientFactory;
        _webhookRepo = webhookRepo;
        _logger = logger;
    }
    
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task DeliverAsync(Guid deliveryId)
    {
        var delivery = await _webhookRepo.GetDeliveryByIdAsync(deliveryId);
        if (delivery == null)
        {
            _logger.LogWarning("Delivery {DeliveryId} not found", deliveryId);
            return;
        }
        
        var subscription = await _webhookRepo.GetSubscriptionByIdAsync(delivery.SubscriptionId);
        if (subscription == null || !subscription.IsActive)
        {
            _logger.LogWarning("Subscription {SubscriptionId} not found or inactive", delivery.SubscriptionId);
            return;
        }
        
        var httpClient = _httpClientFactory.CreateClient();
        
        var webhookPayload = new
        {
            @event = delivery.EventType,
            data = delivery.Payload,
            timestamp = delivery.CreatedAt
        };
        
        var jsonPayload = JsonSerializer.Serialize(webhookPayload);
        
        var signature = ComputeHmacSignature(jsonPayload, subscription.Secret);
        
        var request = new HttpRequestMessage(HttpMethod.Post, subscription.Url);
        request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        request.Headers.Add("X-AcadSign-Signature", signature);
        request.Headers.Add("X-AcadSign-Event", delivery.EventType);
        
        try
        {
            var response = await httpClient.SendAsync(request);
            
            delivery.ResponseStatus = (int)response.StatusCode;
            delivery.ResponseBody = await response.Content.ReadAsStringAsync();
            delivery.AttemptCount++;
            
            if (response.IsSuccessStatusCode)
            {
                delivery.DeliveredAt = DateTime.UtcNow;
                _logger.LogInformation("Webhook delivery {DeliveryId} successful", deliveryId);
            }
            else
            {
                _logger.LogWarning("Webhook delivery {DeliveryId} failed with status {Status}", 
                    deliveryId, response.StatusCode);
            }
            
            await _webhookRepo.UpdateDeliveryAsync(delivery);
            
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            delivery.AttemptCount++;
            await _webhookRepo.UpdateDeliveryAsync(delivery);
            
            _logger.LogError(ex, "Webhook delivery {DeliveryId} failed", deliveryId);
            throw;
        }
    }
    
    private string ComputeHmacSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }
}
