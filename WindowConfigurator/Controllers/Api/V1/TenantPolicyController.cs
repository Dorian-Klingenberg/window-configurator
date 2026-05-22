using Microsoft.AspNetCore.Mvc;
using WindowConfigurator.Controllers.Api.V1.Models;
using WindowConfigurator.Controllers.Api.V1.Security;
using WindowConfigurator.Data.Repositories;

namespace WindowConfigurator.Controllers.Api.V1
{
    [ApiController]
    [ApiKeyAuthorize]
    [Route("api/v1/tenants")]
    public class TenantPolicyController : ControllerBase
    {
        private readonly ITenantRepository _tenantRepository;

        public TenantPolicyController(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        [HttpGet("{id:guid}/policy")]
        public async Task<ActionResult<TenantPolicySettingsResponse>> GetPolicy(Guid id)
        {
            if (!TryGetAuthenticatedTenantId(out var authenticatedTenantId))
                return Unauthorized(ApiErrorResponse.Validation("Authentication required.", new ApiValidationError { Field = "x-api-key", Message = "Missing or invalid API key." }));
            if (id != authenticatedTenantId)
                return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Validation("Forbidden.", new ApiValidationError { Field = "tenantId", Message = "Tenant scope mismatch." }));

            var tenant = await _tenantRepository.GetByIdAsync(id);
            if (tenant == null)
                return NotFound(ApiErrorResponse.NotFound("Tenant not found."));

            return Ok(ToResponse(tenant));
        }

        [HttpPut("{id:guid}/policy")]
        public async Task<ActionResult<TenantPolicySettingsResponse>> UpdatePolicy(Guid id, [FromBody] TenantPolicySettingsRequest request)
        {
            if (!TryGetAuthenticatedTenantId(out var authenticatedTenantId))
                return Unauthorized(ApiErrorResponse.Validation("Authentication required.", new ApiValidationError { Field = "x-api-key", Message = "Missing or invalid API key." }));
            if (id != authenticatedTenantId)
                return StatusCode(StatusCodes.Status403Forbidden, ApiErrorResponse.Validation("Forbidden.", new ApiValidationError { Field = "tenantId", Message = "Tenant scope mismatch." }));

            var tenant = await _tenantRepository.GetByIdAsync(id);
            if (tenant == null)
                return NotFound(ApiErrorResponse.NotFound("Tenant not found."));

            tenant.AllowedProductLineKeys = request.AllowedProductLineKeys
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            tenant.MixedProductLinesAllowed = request.MixedProductLinesAllowed;
            tenant.Branding.LogoUrl = request.BrandingLogoUrl ?? tenant.Branding.LogoUrl;
            tenant.Branding.PrimaryColor = request.BrandingPrimaryColor ?? tenant.Branding.PrimaryColor;
            tenant.Branding.AccentColor = request.BrandingAccentColor ?? tenant.Branding.AccentColor;

            await _tenantRepository.SaveChangesAsync();
            return Ok(ToResponse(tenant));
        }

        private static TenantPolicySettingsResponse ToResponse(Data.Entities.TenantEntity tenant)
        {
            return new TenantPolicySettingsResponse
            {
                TenantId = tenant.Id,
                AllowedProductLineKeys = tenant.AllowedProductLineKeys.ToList(),
                MixedProductLinesAllowed = tenant.MixedProductLinesAllowed,
                BrandingLogoUrl = tenant.Branding.LogoUrl,
                BrandingPrimaryColor = tenant.Branding.PrimaryColor,
                BrandingAccentColor = tenant.Branding.AccentColor
            };
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
