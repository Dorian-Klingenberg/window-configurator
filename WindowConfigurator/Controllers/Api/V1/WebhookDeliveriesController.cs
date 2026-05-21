using Microsoft.AspNetCore.Mvc;
using WindowConfigurator.Controllers.Api.V1.Models;
using WindowConfigurator.Data.Webhooks;

namespace WindowConfigurator.Controllers.Api.V1
{
    [ApiController]
    [Route("api/v1/webhook-deliveries")]
    public class WebhookDeliveriesController : ControllerBase
    {
        private readonly IWebhookRetryProcessor _retryProcessor;

        public WebhookDeliveriesController(IWebhookRetryProcessor retryProcessor)
        {
            _retryProcessor = retryProcessor;
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
    }
}
