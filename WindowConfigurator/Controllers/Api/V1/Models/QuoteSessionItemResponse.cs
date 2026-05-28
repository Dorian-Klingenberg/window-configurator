namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class QuoteSessionItemResponse
    {
        public Guid ItemId { get; set; }
        public Guid SessionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ProductLineKey { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int LineItemNumber { get; set; }
        public bool MeetsEgress { get; set; }
        public decimal? AuthoritativePrice { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
