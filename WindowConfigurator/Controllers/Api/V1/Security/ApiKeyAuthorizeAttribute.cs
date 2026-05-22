using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WindowConfigurator.Controllers.Api.V1.Models;
using WindowConfigurator.Data.Repositories;

namespace WindowConfigurator.Controllers.Api.V1.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthorizeAttribute : TypeFilterAttribute
    {
        public ApiKeyAuthorizeAttribute() : base(typeof(ApiKeyAuthorizeFilter))
        {
        }
    }

    public class ApiKeyAuthorizeFilter : IAsyncActionFilter
    {
        private readonly ITenantRepository _tenantRepository;
        public const string TenantIdItemKey = "AuthenticatedTenantId";

        public ApiKeyAuthorizeFilter(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var apiKeyValues))
            {
                context.Result = new UnauthorizedObjectResult(ApiErrorResponse.Validation(
                    "Authentication required.",
                    new ApiValidationError { Field = "x-api-key", Message = "Missing API key." }));
                return;
            }

            var apiKey = apiKeyValues.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                context.Result = new UnauthorizedObjectResult(ApiErrorResponse.Validation(
                    "Authentication required.",
                    new ApiValidationError { Field = "x-api-key", Message = "Missing API key." }));
                return;
            }

            var tenant = await _tenantRepository.GetByApiKeyAsync(apiKey);
            if (tenant == null)
            {
                context.Result = new UnauthorizedObjectResult(ApiErrorResponse.Validation(
                    "Authentication required.",
                    new ApiValidationError { Field = "x-api-key", Message = "Invalid API key." }));
                return;
            }

            if (tenant.ApiKeyRevokedAt.HasValue)
            {
                context.Result = new UnauthorizedObjectResult(ApiErrorResponse.Validation(
                    "Authentication required.",
                    new ApiValidationError { Field = "x-api-key", Message = "API key has been revoked." }));
                return;
            }

            if (tenant.ApiKeyExpiresAtUtc.HasValue && tenant.ApiKeyExpiresAtUtc.Value < DateTime.UtcNow)
            {
                context.Result = new UnauthorizedObjectResult(ApiErrorResponse.Validation(
                    "Authentication required.",
                    new ApiValidationError { Field = "x-api-key", Message = "API key has expired." }));
                return;
            }

            context.HttpContext.Items[TenantIdItemKey] = tenant.Id;
            await next();
        }
    }
}
