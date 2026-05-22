namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class WebhookDeliveryStatsResponse
    {
        public int Delivered { get; set; }
        public int Failed { get; set; }
        public int Total { get; set; }
        public DateTime AsOfUtc { get; set; }
    }
}
