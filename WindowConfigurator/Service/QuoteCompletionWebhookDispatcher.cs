using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using WindowConfigurator.Data.Catalog;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Webhooks;

namespace WindowConfigurator.Web.Service
{
    public class QuoteCompletionWebhookDispatcher : IQuoteCompletionWebhookDispatcher
    {
        private readonly HttpClient _httpClient;

        public QuoteCompletionWebhookDispatcher(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<QuoteCompletionWebhookDispatchResult> DispatchQuoteCompletedAsync(
            QuoteSessionEntity session,
            TenantEntity tenant,
            ICatalogService catalog,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenant.WebhookCallbackUrl))
            {
                return new QuoteCompletionWebhookDispatchResult
                {
                    Succeeded = false,
                    Error = "Tenant webhook callback URL is not configured."
                };
            }

            var payload = BuildPayload(session, catalog);

            try
            {
                var response = await _httpClient.PostAsJsonAsync(tenant.WebhookCallbackUrl, payload, cancellationToken);
                return new QuoteCompletionWebhookDispatchResult
                {
                    Succeeded = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Error = response.IsSuccessStatusCode ? null : $"Webhook returned {(int)response.StatusCode}."
                };
            }
            catch (Exception ex)
            {
                return new QuoteCompletionWebhookDispatchResult
                {
                    Succeeded = false,
                    Error = ex.Message
                };
            }
        }

        private static QuoteCompletedPayload BuildPayload(QuoteSessionEntity session, ICatalogService catalog)
        {
            var orderGroups = session.Items
                .Where(i => !string.IsNullOrWhiteSpace(i.ProductLineKey))
                .GroupBy(i => i.ProductLineKey, StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var first = g.First();
                    var catalogEntry = catalog.GetProductLine(first.ProductLineKey);
                    var items = g.OrderBy(i => i.LineItemNumber).Select(ToCompletedItem).ToList();

                    return new QuoteCompletedOrderGroup
                    {
                        ProductLineKey = first.ProductLineKey,
                        ProductLineName = catalogEntry?.Name ?? first.ProductLineKey,
                        ManufacturerName = catalogEntry?.ManufacturerName ?? string.Empty,
                        Items = items,
                        GroupAuthoritativePrice = items.Sum(i => i.AuthoritativePrice ?? 0m)
                    };
                })
                .ToList();

            return new QuoteCompletedPayload
            {
                OccurredAt = DateTime.UtcNow,
                Session = new QuoteCompletedSessionSummary
                {
                    Id = session.Id,
                    TenantId = session.TenantId,
                    ExternalReferenceId = session.ExternalReferenceId,
                    CustomerEmail = session.CustomerEmail,
                    CompletedAt = session.CompletedAt ?? DateTime.UtcNow,
                    ItemCount = session.Items.Count,
                    TotalAuthoritativePrice = session.Items.Sum(i => i.AuthoritativePrice ?? 0m)
                },
                OrderGroups = orderGroups
            };
        }

        private static QuoteCompletedItem ToCompletedItem(ConfiguredWindowItemEntity item)
        {
            return new QuoteCompletedItem
            {
                Id = item.Id,
                LineItemNumber = item.LineItemNumber,
                Location = item.Location,
                MeetsEgress = item.MeetsEgress,
                SectionCount = item.SectionCount,
                AuthoritativePrice = item.AuthoritativePrice,
                Measurements = new QuoteCompletedMeasurements
                {
                    Frame = ToDimension(item.FrameWidthDecimal, item.FrameHeightDecimal),
                    RoughOpening = item.RoughOpeningWidthDecimal.HasValue || item.RoughOpeningHeightDecimal.HasValue
                        ? ToDimension(item.RoughOpeningWidthDecimal, item.RoughOpeningHeightDecimal)
                        : null,
                    Outside = ToDimension(item.OutsideMeasureWidthDecimal, item.OutsideMeasureHeightDecimal)
                },
                Configuration = TryParseConfiguration(item.ConfigurationJson)
            };
        }

        private static QuoteCompletedDimension ToDimension(decimal? width, decimal? height)
        {
            return new QuoteCompletedDimension
            {
                WidthInches = width,
                HeightInches = height,
                WidthDisplay = width?.ToString("0.####", CultureInfo.InvariantCulture),
                HeightDisplay = height?.ToString("0.####", CultureInfo.InvariantCulture)
            };
        }

        private static object? TryParseConfiguration(string? configurationJson)
        {
            if (string.IsNullOrWhiteSpace(configurationJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<object>(configurationJson);
            }
            catch
            {
                return configurationJson;
            }
        }
    }
}
