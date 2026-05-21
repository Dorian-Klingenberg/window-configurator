using Microsoft.AspNetCore.Mvc;
using WindowConfigurator.Controllers.Api.V1.Models;
using WindowConfigurator.Data.Repositories;

namespace WindowConfigurator.Controllers.Api.V1
{
    [ApiController]
    [Route("api/v1/tenants")]
    public class TenantIntegrationController : ControllerBase
    {
        private readonly ITenantRepository _tenantRepository;

        public TenantIntegrationController(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        [HttpGet("{id:guid}/integration")]
        public async Task<ActionResult<TenantIntegrationSettingsResponse>> GetIntegration(Guid id)
        {
            var tenant = await _tenantRepository.GetByIdAsync(id);
            if (tenant == null)
                return NotFound(ApiErrorResponse.NotFound("Tenant not found."));

            return Ok(new TenantIntegrationSettingsResponse
            {
                TenantId = tenant.Id,
                WebhookCallbackUrl = tenant.WebhookCallbackUrl
            });
        }

        [HttpPut("{id:guid}/integration")]
        public async Task<ActionResult<TenantIntegrationSettingsResponse>> UpdateIntegration(
            Guid id,
            [FromBody] TenantIntegrationSettingsRequest request)
        {
            var tenant = await _tenantRepository.GetByIdAsync(id);
            if (tenant == null)
                return NotFound(ApiErrorResponse.NotFound("Tenant not found."));

            if (!Uri.TryCreate(request.WebhookCallbackUrl, UriKind.Absolute, out _))
            {
                return BadRequest(ApiErrorResponse.Validation(
                    "The request is invalid.",
                    new ApiValidationError
                    {
                        Field = "webhookCallbackUrl",
                        Message = "Webhook callback URL must be an absolute URL."
                    }));
            }

            tenant.WebhookCallbackUrl = request.WebhookCallbackUrl.Trim();
            await _tenantRepository.SaveChangesAsync();

            return Ok(new TenantIntegrationSettingsResponse
            {
                TenantId = tenant.Id,
                WebhookCallbackUrl = tenant.WebhookCallbackUrl
            });
        }
    }
}
