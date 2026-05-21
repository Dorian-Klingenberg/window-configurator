namespace WindowConfigurator.Data.Webhooks
{
    public class QuoteCompletionWebhookDispatchResult
    {
        public bool Succeeded { get; set; }
        public int? StatusCode { get; set; }
        public string? Error { get; set; }
    }
}
