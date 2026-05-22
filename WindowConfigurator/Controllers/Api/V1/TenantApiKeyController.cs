using Microsoft.AspNetCore.Mvc;
using WindowConfigurator.Controllers.Api.V1.Models;
using WindowConfigurator.Controllers.Api.V1.Security;
using WindowConfigurator.Data.Repositories;

namespace WindowConfigurator.Controllers.Api.V1
{
    [ApiController]
    [ApiKeyAuthorize]
    [Route("api/v1/tenants")]
    public class TenantApiKeyController : ControllerBase
    {
        private readonly ITenantRepository _tenantRepository;

        public TenantApiKeyController(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        /// <summary>
        /// Issues a new API key for the tenant, immediately replacing the current one.
        /// Any previous revocation or expiry is cleared on the new key.
        /// The raw key value is only returned in this response — store it securely.
        /// </summary>
        [HttpPost("{id:guid}/api-key/rotate")]
        public async Task<ActionResult<RotateApiKeyResponse>> Rotate(Guid id, [FromBody] RotateApiKeyRequest request)
        {
            if (!TryGetAuthenticatedTenantId(out var authenticatedTenantId))
                return Unauthorized(ApiErrorResponse.Validation("Authentication required.", new ApiValidationError { Field = "x-api-key", Message = "Missing or invalid API key." }));
            if (id != authenticatedTenantId)
                return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Validation("Forbidden.", new ApiValidationError { Field = "tenantId", Message = "Tenant scope mismatch." }));

            var tenant = await _tenantRepository.GetByIdAsync(id);
            if (tenant == null)
                return NotFound(ApiErrorResponse.NotFound("Tenant not found."));

            var newKey = Guid.NewGuid().ToString("N");
            var issuedAt = DateTime.UtcNow;

            tenant.ApiKey = newKey;
            tenant.ApiKeyRevokedAt = null;
            tenant.ApiKeyExpiresAtUtc = request.ExpiresAtUtc;
            await _tenantRepository.SaveChangesAsync();

            return Ok(new RotateApiKeyResponse
            {
                TenantId = tenant.Id,
                ApiKey = newKey,
                IssuedAtUtc = issuedAt,
                ExpiresAtUtc = request.ExpiresAtUtc
            });
        }

        /// <summary>
        /// Revokes the tenant's current API key. The key remains in storage but is permanently
        /// rejected by the auth filter. Use rotate to issue a replacement.
        /// </summary>
        [HttpDelete("{id:guid}/api-key")]
        public async Task<IActionResult> Revoke(Guid id)
        {
            if (!TryGetAuthenticatedTenantId(out var authenticatedTenantId))
                return Unauthorized(ApiErrorResponse.Validation("Authentication required.", new ApiValidationError { Field = "x-api-key", Message = "Missing or invalid API key." }));
            if (id != authenticatedTenantId)
                return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Validation("Forbidden.", new ApiValidationError { Field = "tenantId", Message = "Tenant scope mismatch." }));

            var tenant = await _tenantRepository.GetByIdAsync(id);
            if (tenant == null)
                return NotFound(ApiErrorResponse.NotFound("Tenant not found."));

            tenant.ApiKeyRevokedAt ??= DateTime.UtcNow;
            await _tenantRepository.SaveChangesAsync();

            return NoContent();
        }

        private bool TryGetAuthenticatedTenantId(out Guid tenantId)
        {
            tenantId = Guid.Empty;
            if (HttpContext == null || !HttpContext.Items.TryGetValue(ApiKeyAuthorizeFilter.TenantIdItemKey, out var raw) || raw is not Guid parsed)
                return false;
            tenantId = parsed;
            return true;
        }
    }
}
