namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class CompleteQuoteSessionResponse
    {
        public Guid SessionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }
        public int CompletedItemCount { get; set; }
        public string WebhookDispatchStatus { get; set; } = "not_attempted";
        public int? WebhookStatusCode { get; set; }
        public string? WebhookError { get; set; }
    }
}
