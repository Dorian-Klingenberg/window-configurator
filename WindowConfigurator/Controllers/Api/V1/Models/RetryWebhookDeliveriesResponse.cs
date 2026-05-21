namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class RetryWebhookDeliveriesResponse
    {
        public int ProcessedAttempts { get; set; }
        public DateTime ProcessedAtUtc { get; set; }
    }
}
