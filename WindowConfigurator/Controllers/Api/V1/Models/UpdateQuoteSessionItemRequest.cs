namespace WindowConfigurator.Controllers.Api.V1.Models
{
    public class UpdateQuoteSessionItemRequest
    {
        public string? ProductLineKey { get; set; }
        public string? Location { get; set; }
        public int? LineItemNumber { get; set; }
        public bool? MeetsEgress { get; set; }
    }
}
