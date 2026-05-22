using Microsoft.AspNetCore.Mvc;
using WindowConfigurator.Controllers.Api.V1.Models;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Repositories;

namespace WindowConfigurator.Controllers.Api.V1
{
    [ApiController]
    [Route("api/v1/prospect-sessions")]
    public class ProspectSessionsController : ControllerBase
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly IQuoteSessionRepository _sessionRepository;

        public ProspectSessionsController(ITenantRepository tenantRepository, IQuoteSessionRepository sessionRepository)
        {
            _tenantRepository = tenantRepository;
            _sessionRepository = sessionRepository;
        }

        [HttpPost]
        public async Task<ActionResult<StartProspectSessionResponse>> Start([FromBody] StartProspectSessionRequest request)
        {
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);
            if (tenant == null)
            {
                return BadRequest(ApiErrorResponse.Validation(
                    "The request is invalid.",
                    new ApiValidationError { Field = "tenantId", Message = "Unknown tenant." }));
            }

            if (string.IsNullOrWhiteSpace(request.CustomerEmail))
            {
                return BadRequest(ApiErrorResponse.Validation(
                    "The request is invalid.",
                    new ApiValidationError { Field = "customerEmail", Message = "Customer email is required." }));
            }

            var token = Guid.NewGuid().ToString("N");
            var expiresAt = DateTime.UtcNow.AddHours(24);
            var defaultProductLine = tenant.AllowedProductLineKeys.FirstOrDefault();

            var session = new QuoteSessionEntity
            {
                TenantId = tenant.Id,
                CustomerEmail = request.CustomerEmail.Trim(),
                MagicLinkToken = token,
                MagicLinkExpiresAt = expiresAt,
                ExpiresAt = expiresAt,
                DefaultProductLineKey = defaultProductLine
            };
            await _sessionRepository.AddAsync(session);
            await _sessionRepository.SaveChangesAsync();

            return Ok(new StartProspectSessionResponse
            {
                SessionId = session.Id,
                MagicLinkToken = token,
                ExpiresAtUtc = expiresAt
            });
        }

        [HttpGet("resume")]
        public async Task<ActionResult<ResumeProspectSessionResponse>> Resume([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(ApiErrorResponse.Validation("The request is invalid.", new ApiValidationError { Field = "token", Message = "Token is required." }));

            var session = await _sessionRepository.GetByMagicLinkTokenAsync(token);
            if (session == null)
                return NotFound(ApiErrorResponse.NotFound("Prospect session not found."));

            if (session.MagicLinkExpiresAt == null || session.MagicLinkExpiresAt < DateTime.UtcNow)
                return StatusCode(StatusCodes.Status410Gone, ApiErrorResponse.Validation("Token expired.", new ApiValidationError { Field = "token", Message = "Magic link token has expired." }));

            return Ok(new ResumeProspectSessionResponse
            {
                SessionId = session.Id,
                SessionUrl = "/" + session.Id.ToString(),
                ExpiresAtUtc = session.MagicLinkExpiresAt.Value
            });
        }
    }
}
