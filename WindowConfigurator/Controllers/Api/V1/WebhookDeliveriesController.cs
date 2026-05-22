using Microsoft.AspNetCore.Mvc;
using WindowConfigurator.Controllers.Api.V1.Models;
using WindowConfigurator.Controllers.Api.V1.Security;
using WindowConfigurator.Data.Repositories;
using WindowConfigurator.Data.Webhooks;

namespace WindowConfigurator.Controllers.Api.V1
{
    [ApiController]
    [ApiKeyAuthorize]
    [Route("api/v1/webhook-deliveries")]
    public class WebhookDeliveriesController : ControllerBase
    {
        private readonly IWebhookRetryProcessor _retryProcessor;
        private readonly IWebhookDeliveryAttemptRepository _deliveryAttemptRepository;

        public WebhookDeliveriesController(
            IWebhookRetryProcessor retryProcessor,
            IWebhookDeliveryAttemptRepository deliveryAttemptRepository)
        {
            _retryProcessor = retryProcessor;
            _deliveryAttemptRepository = deliveryAttemptRepository;
        }

        [HttpPost("retry-due")]
        public async Task<ActionResult<RetryWebhookDeliveriesResponse>> RetryDue([FromQuery] int maxAttempts = 50)
        {
            var processed = await _retryProcessor.ProcessDueRetriesAsync(maxAttempts);
            return Ok(new RetryWebhookDeliveriesResponse
            {
                ProcessedAttempts = processed,
                ProcessedAtUtc = DateTime.UtcNow
            });
        }

        [HttpGet("stats")]
        public async Task<ActionResult<WebhookDeliveryStatsResponse>> GetStats()
        {
            var stats = await _deliveryAttemptRepository.GetStatsAsync();
            return Ok(new WebhookDeliveryStatsResponse
            {
                Delivered = stats.Delivered,
                Failed = stats.Failed,
                Total = stats.Total,
                AsOfUtc = DateTime.UtcNow
            });
        }
    }
}
