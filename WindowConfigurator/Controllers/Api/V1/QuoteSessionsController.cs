using Microsoft.AspNetCore.Mvc;
using WindowConfigurator.Controllers.Api.V1.Models;
using WindowConfigurator.Data.Catalog;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Repositories;
using WindowConfigurator.Data.Webhooks;

namespace WindowConfigurator.Controllers.Api.V1
{
    [ApiController]
    [Route("api/v1/quote-sessions")]
    public class QuoteSessionsController : ControllerBase
    {
        private readonly IQuoteSessionRepository _sessionRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly ICatalogService _catalog;
        private readonly IQuoteCompletionWebhookDispatcher _webhookDispatcher;
        private readonly IWebhookDeliveryAttemptRepository _deliveryAttemptRepository;

        public QuoteSessionsController(
            IQuoteSessionRepository sessionRepository,
            ITenantRepository tenantRepository,
            ICatalogService catalog,
            IQuoteCompletionWebhookDispatcher webhookDispatcher,
            IWebhookDeliveryAttemptRepository deliveryAttemptRepository)
        {
            _sessionRepository = sessionRepository;
            _tenantRepository = tenantRepository;
            _catalog = catalog;
            _webhookDispatcher = webhookDispatcher;
            _deliveryAttemptRepository = deliveryAttemptRepository;
        }

        [HttpPost]
        public async Task<ActionResult<QuoteSessionResponse>> Create([FromBody] CreateQuoteSessionRequest request)
        {
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);
            if (tenant == null)
                return BadRequest(ApiErrorResponse.Validation(
                    "The request is invalid.",
                    new ApiValidationError { Field = "tenantId", Message = "Unknown tenant." }));

            var defaultProductLineKey = request.DefaultProductLineKey?.Trim();
            if (!string.IsNullOrWhiteSpace(defaultProductLineKey))
            {
                var catalogEntry = _catalog.GetProductLine(defaultProductLineKey);
                if (catalogEntry == null)
                {
                    return BadRequest(ApiErrorResponse.Validation(
                        "The request is invalid.",
                        new ApiValidationError
                        {
                            Field = "defaultProductLineKey",
                            Message = "The selected default product line is not supported."
                        }));
                }

                if (tenant.AllowedProductLineKeys.Count > 0 &&
                    !tenant.AllowedProductLineKeys.Contains(catalogEntry.Key, StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(ApiErrorResponse.Validation(
                        "The request is invalid.",
                        new ApiValidationError
                        {
                            Field = "defaultProductLineKey",
                            Message = "The selected default product line is not allowed for this tenant."
                        }));
                }

                defaultProductLineKey = catalogEntry.Key;
            }

            var session = new QuoteSessionEntity
            {
                TenantId = tenant.Id,
                DefaultProductLineKey = defaultProductLineKey,
                ExternalReferenceId = string.IsNullOrWhiteSpace(request.ExternalReferenceId)
                    ? null
                    : request.ExternalReferenceId.Trim(),
                CustomerEmail = string.IsNullOrWhiteSpace(request.CustomerEmail)
                    ? null
                    : request.CustomerEmail.Trim()
            };

            await _sessionRepository.AddAsync(session);
            await _sessionRepository.SaveChangesAsync();

            var response = ToResponse(session);
            return CreatedAtAction(nameof(GetById), new { id = session.Id }, response);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<QuoteSessionResponse>> GetById(Guid id)
        {
            var session = await _sessionRepository.GetByIdAsync(id);
            if (session == null)
                return NotFound(ApiErrorResponse.NotFound("Quote session not found."));

            return Ok(ToResponse(session));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<QuoteSessionResponse>> Update(Guid id, [FromBody] UpdateQuoteSessionRequest request)
        {
            var session = await _sessionRepository.GetByIdAsync(id);
            if (session == null)
                return NotFound(ApiErrorResponse.NotFound("Quote session not found."));

            var tenant = await _tenantRepository.GetByIdAsync(session.TenantId);
            if (tenant == null)
            {
                return BadRequest(ApiErrorResponse.Validation(
                    "The request is invalid.",
                    new ApiValidationError { Field = "tenantId", Message = "Unknown tenant." }));
            }

            if (request.DefaultProductLineKey != null)
            {
                var validatedProductLineKey = ValidateAndNormalizeProductLineKey(request.DefaultProductLineKey, tenant);
                if (validatedProductLineKey == null && !string.IsNullOrWhiteSpace(request.DefaultProductLineKey))
                {
                    return BadRequest(ApiErrorResponse.Validation(
                        "The request is invalid.",
                        new ApiValidationError
                        {
                            Field = "defaultProductLineKey",
                            Message = "The selected default product line is not allowed for this tenant."
                        }));
                }

                session.DefaultProductLineKey = validatedProductLineKey;
            }

            if (request.ExternalReferenceId != null)
                session.ExternalReferenceId = string.IsNullOrWhiteSpace(request.ExternalReferenceId) ? null : request.ExternalReferenceId.Trim();

            if (request.CustomerEmail != null)
                session.CustomerEmail = string.IsNullOrWhiteSpace(request.CustomerEmail) ? null : request.CustomerEmail.Trim();

            await _sessionRepository.SaveChangesAsync();
            return Ok(ToResponse(session));
        }

        [HttpPost("{id:guid}/items")]
        public async Task<ActionResult<QuoteSessionItemResponse>> AddItem(Guid id, [FromBody] AddQuoteSessionItemRequest request)
        {
            var session = await _sessionRepository.GetByIdAsync(id);
            if (session == null)
                return NotFound(ApiErrorResponse.NotFound("Quote session not found."));

            var tenant = await _tenantRepository.GetByIdAsync(session.TenantId);
            if (tenant == null)
            {
                return BadRequest(ApiErrorResponse.Validation(
                    "The request is invalid.",
                    new ApiValidationError { Field = "tenantId", Message = "Unknown tenant." }));
            }

            var requestedProductLine = request.ProductLineKey ?? session.DefaultProductLineKey ?? string.Empty;
            var normalizedProductLine = ValidateAndNormalizeProductLineKey(requestedProductLine, tenant);
            if (string.IsNullOrWhiteSpace(normalizedProductLine))
            {
                return BadRequest(ApiErrorResponse.Validation(
                    "The request is invalid.",
                    new ApiValidationError
                    {
                        Field = "productLineKey",
                        Message = "A supported product line is required."
                    }));
            }

            var item = new ConfiguredWindowItemEntity
            {
                ProductLineKey = normalizedProductLine,
                Location = request.Location?.Trim() ?? string.Empty,
                LineItemNumber = request.LineItemNumber ?? NextLineItemNumber(session),
                MeetsEgress = request.MeetsEgress ?? false
            };

            await _sessionRepository.AddItemAsync(session.Id, item);
            if (!session.Items.Contains(item))
                session.Items.Add(item);
            await _sessionRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = session.Id }, ToItemResponse(session.Id, item));
        }

        [HttpPut("{id:guid}/items/{itemId:guid}")]
        public async Task<ActionResult<QuoteSessionItemResponse>> UpdateItem(
            Guid id,
            Guid itemId,
            [FromBody] UpdateQuoteSessionItemRequest request)
        {
            var session = await _sessionRepository.GetByIdAsync(id);
            if (session == null)
                return NotFound(ApiErrorResponse.NotFound("Quote session not found."));

            var item = session.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                return NotFound(ApiErrorResponse.NotFound("Quote session item not found."));

            var tenant = await _tenantRepository.GetByIdAsync(session.TenantId);
            if (tenant == null)
            {
                return BadRequest(ApiErrorResponse.Validation(
                    "The request is invalid.",
                    new ApiValidationError { Field = "tenantId", Message = "Unknown tenant." }));
            }

            if (request.ProductLineKey != null)
            {
                var normalizedProductLine = ValidateAndNormalizeProductLineKey(request.ProductLineKey, tenant);
                if (string.IsNullOrWhiteSpace(normalizedProductLine))
                {
                    return BadRequest(ApiErrorResponse.Validation(
                        "The request is invalid.",
                        new ApiValidationError
                        {
                            Field = "productLineKey",
                            Message = "The selected product line is not allowed for this tenant."
                        }));
                }
                item.ProductLineKey = normalizedProductLine;
            }

            if (request.Location != null)
                item.Location = request.Location.Trim();

            if (request.LineItemNumber.HasValue)
                item.LineItemNumber = request.LineItemNumber.Value;

            if (request.MeetsEgress.HasValue)
                item.MeetsEgress = request.MeetsEgress.Value;

            await _sessionRepository.SaveChangesAsync();
            return Ok(ToItemResponse(session.Id, item));
        }

        [HttpPost("{id:guid}/complete")]
        public async Task<ActionResult<CompleteQuoteSessionResponse>> Complete(
            Guid id,
            [FromBody] CompleteQuoteSessionRequest? request)
        {
            var session = await _sessionRepository.GetByIdAsync(id);
            if (session == null)
                return NotFound(ApiErrorResponse.NotFound("Quote session not found."));

            var allItemsCompleted = session.Items.Count > 0 &&
                                    session.Items.All(i => i.Status == ConfiguredWindowItemStatus.Completed);
            if (!allItemsCompleted || session.Status != QuoteSessionStatus.Completed)
            {
                return BadRequest(ApiErrorResponse.Validation(
                    "The request is invalid.",
                    new ApiValidationError
                    {
                        Field = "status",
                        Message = "Session completion requires all items to be completed first."
                    }));
            }

            session.Status = QuoteSessionStatus.Submitted;
            await _sessionRepository.SaveChangesAsync();

            var tenant = await _tenantRepository.GetByIdAsync(session.TenantId);
            var dispatch = tenant == null
                ? new QuoteCompletionWebhookDispatchResult
                {
                    Succeeded = false,
                    Error = "Unknown tenant."
                }
                : await _webhookDispatcher.DispatchQuoteCompletedAsync(session, tenant, _catalog);

            var attemptedAtUtc = DateTime.UtcNow;
            await _deliveryAttemptRepository.AddAsync(new WebhookDeliveryAttemptEntity
            {
                QuoteSessionId = session.Id,
                TenantId = session.TenantId,
                EventType = "quote.completed",
                Status = dispatch.Succeeded ? "Delivered" : "Failed",
                AttemptCount = 1,
                StatusCode = dispatch.StatusCode,
                Error = dispatch.Error,
                AttemptedAtUtc = attemptedAtUtc,
                NextRetryAtUtc = dispatch.Succeeded ? null : attemptedAtUtc.AddMinutes(5)
            });
            await _deliveryAttemptRepository.SaveChangesAsync();

            return Ok(new CompleteQuoteSessionResponse
            {
                SessionId = session.Id,
                Status = session.Status.ToString(),
                SubmittedAt = request?.RequestedAtUtc ?? DateTime.UtcNow,
                CompletedItemCount = session.Items.Count,
                WebhookDispatchStatus = dispatch.Succeeded ? "delivered" : "failed",
                WebhookStatusCode = dispatch.StatusCode,
                WebhookError = dispatch.Error
            });
        }

        private static QuoteSessionResponse ToResponse(QuoteSessionEntity session)
        {
            return new QuoteSessionResponse
            {
                SessionId = session.Id,
                TenantId = session.TenantId,
                Status = session.Status.ToString(),
                DefaultProductLineKey = session.DefaultProductLineKey,
                ExternalReferenceId = session.ExternalReferenceId,
                CustomerEmail = session.CustomerEmail,
                SessionUrl = "/" + session.Id.ToString(),
                CreatedAt = session.CreatedAt,
                Items = session.Items
                    .OrderBy(i => i.LineItemNumber)
                    .Select(i => ToItemResponse(session.Id, i))
                    .ToList()
            };
        }

        private string? ValidateAndNormalizeProductLineKey(string? requestedProductLineKey, TenantEntity tenant)
        {
            if (string.IsNullOrWhiteSpace(requestedProductLineKey))
                return null;

            var catalogEntry = _catalog.GetProductLine(requestedProductLineKey.Trim());
            if (catalogEntry == null)
                return null;

            if (tenant.AllowedProductLineKeys.Count > 0 &&
                !tenant.AllowedProductLineKeys.Contains(catalogEntry.Key, StringComparer.OrdinalIgnoreCase))
            {
                return null;
            }

            return catalogEntry.Key;
        }

        private static int NextLineItemNumber(QuoteSessionEntity session)
        {
            return session.Items.Count == 0 ? 1 : session.Items.Max(i => i.LineItemNumber) + 1;
        }

        private static QuoteSessionItemResponse ToItemResponse(Guid sessionId, ConfiguredWindowItemEntity item)
        {
            return new QuoteSessionItemResponse
            {
                ItemId = item.Id,
                SessionId = sessionId,
                Status = item.Status.ToString(),
                ProductLineKey = item.ProductLineKey,
                Location = item.Location,
                LineItemNumber = item.LineItemNumber,
                MeetsEgress = item.MeetsEgress
            };
        }
    }
}
