using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WindowConfigurator.Data.Catalog;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Pricing;
using WindowConfigurator.Data.Repositories;
using WindowConfigurator.Data.Validation;
using WindowConfigurator.Models;
using WindowConfigurator.Web.Service;

namespace WindowConfigurator.Controllers
{
    public class OrderItemController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> CreateDevSession()
        {
            var session = new QuoteSessionEntity
            {
                TenantId = Guid.Empty,
                DefaultProductLineKey = "energysaver-2500"
            };
            
            await _sessionRepository.AddAsync(session);
            await _sessionRepository.AddItemAsync(session.Id, new ConfiguredWindowItemEntity
            {
                ProductLineKey = "energysaver-2500",
                Location = "Dev Window"
            });
            await _sessionRepository.SaveChangesAsync();
            
            return RedirectToAction("Index", new { id = session.Id.ToString() });
        }


        private readonly IQuoteSessionRepository _sessionRepository;
        private readonly ICatalogService _catalog;
        private readonly ITemplateReader _templateReader;
        private readonly IPricingService _pricingService;
        private readonly ITenantRepository _tenantRepository;
        private readonly ICompletionValidationService _completionValidation;

        public OrderItemController(
            IQuoteSessionRepository sessionRepository,
            ICatalogService catalog,
            ITemplateReader templateReader,
            IPricingService pricingService,
            ITenantRepository tenantRepository,
            ICompletionValidationService completionValidation)
        {
            _sessionRepository = sessionRepository;
            _catalog = catalog;
            _templateReader = templateReader;
            _pricingService = pricingService;
            _tenantRepository = tenantRepository;
            _completionValidation = completionValidation;
        }

        public async Task<IActionResult> Index(string id)
        {
            var session = await ResolveIndexSessionAsync(id);
            if (session == null)
                return NotFound();

            if (!string.Equals(id, session.Id.ToString(), StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", new { id = session.Id.ToString() });

            return View(new OrderItemViewModel { Id = session.Id.ToString() });
        }

        [HttpPost]
        public async Task<IActionResult> Complete(string id, [FromBody] JsonElement payload)
        {
            if (!Guid.TryParse(id, out var sessionId))
                return NotFound();

            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
                return NotFound();

            var item = await ResolveOrCreateItemAsync(session, payload);
            var productLineKey = ResolveProductLineKey(session, item, payload);
            var catalogEntry = _catalog.GetProductLine(productLineKey);
            if (catalogEntry == null)
            {
                return BadRequest(new
                {
                    validationErrors = new[]
                    {
                        new CompletionValidationError("productLine", "The selected product line is not supported.")
                    }
                });
            }

            var tenant = await _tenantRepository.GetByIdAsync(session.TenantId);
            var itemTemplateJson = await _templateReader.ReadTemplateAsync(catalogEntry.ItemTemplateFile);
            var validation = _completionValidation.Validate(payload, productLineKey, tenant, itemTemplateJson);
            if (!validation.IsValid)
                return BadRequest(new { validationErrors = validation.Errors });

            var pricingInput = BuildPricingInput(payload, productLineKey);

            decimal authoritativePrice;
            try
            {
                authoritativePrice = _pricingService.CalculatePrice(pricingInput);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            var completedAt = DateTime.UtcNow;
            ApplyPayloadToItem(item, payload, productLineKey);
            MarkCompleted(item, authoritativePrice, completedAt);
            UpdateSessionState(session, completedAt);

            await _sessionRepository.SaveChangesAsync();

            return Ok(new
            {
                sessionId = session.Id,
                sessionStatus = session.Status.ToString(),
                authoritativePrice,
                completedAt = session.CompletedAt
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetOverview(string id)
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var sessionId))
                return NotFound();

            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null) return NotFound();

            var productLineKey = session.Items.FirstOrDefault()?.ProductLineKey
                ?? session.DefaultProductLineKey;

            var catalogEntry = productLineKey != null ? _catalog.GetProductLine(productLineKey) : null;
            var templateFile = catalogEntry?.ItemTemplateFile ?? "energySaverItemTemplate.json";

            var json = await _templateReader.ReadTemplateAsync(templateFile);
            return Ok(json);
        }

        [HttpGet]
        public async Task<IActionResult> PriceInfo(string id)
        {
            var json = await _templateReader.ReadTemplateAsync("priceInfo.json");
            return Ok(json);
        }

        [HttpGet]
        public async Task<IActionResult> SectionTemplate(string id)
        {
            if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var sessionId))
                return NotFound();

            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null) return NotFound();

            var productLineKey = session.Items.FirstOrDefault()?.ProductLineKey
                ?? session.DefaultProductLineKey;

            var catalogEntry = productLineKey != null ? _catalog.GetProductLine(productLineKey) : null;
            var templateFile = catalogEntry?.SectionTemplateFile ?? "energySaverSectionTemplate.json";

            var json = await _templateReader.ReadTemplateAsync(templateFile);
            return Ok(json);
        }

        private ConfiguredWindowItemEntity? ResolveDraftItem(QuoteSessionEntity session, JsonElement payload)
        {
            if (TryReadGuid(payload, "id") is Guid itemId)
                return session.Items.FirstOrDefault(item => item.Id == itemId);

            return session.Items.FirstOrDefault();
        }

        private async Task<QuoteSessionEntity?> ResolveIndexSessionAsync(string id)
        {
            if (!string.IsNullOrWhiteSpace(id) && Guid.TryParse(id, out var sessionId))
            {
                var session = await _sessionRepository.GetByIdAsync(sessionId);
                if (session != null)
                    return session;
            }

            return await _sessionRepository.GetFirstAsync();
        }

        private async Task<ConfiguredWindowItemEntity> ResolveOrCreateItemAsync(QuoteSessionEntity session, JsonElement payload)
        {
            var item = ResolveDraftItem(session, payload);
            if (item != null)
                return item;

            item = new ConfiguredWindowItemEntity
            {
                QuoteSessionId = session.Id
            };
            await _sessionRepository.AddItemAsync(session.Id, item);

            if (!session.Items.Contains(item))
                session.Items.Add(item);

            return item;
        }

        private static void ApplyPayloadToItem(ConfiguredWindowItemEntity item, JsonElement payload, string productLineKey)
        {
            item.Location = TryReadString(payload, "location") ?? item.Location;
            item.LineItemNumber = TryReadInt32(payload, "lineItemNumber") ?? item.LineItemNumber;
            item.MeetsEgress = TryReadBoolean(payload, "meetsEgress") ?? item.MeetsEgress;
            item.ProductLineKey = productLineKey;
            item.FrameWidthDecimal = TryReadMeasurementDecimal(payload, "frameWidth") ?? item.FrameWidthDecimal;
            item.FrameHeightDecimal = TryReadMeasurementDecimal(payload, "frameHeight") ?? item.FrameHeightDecimal;
            item.RoughOpeningWidthDecimal = TryReadMeasurementDecimal(payload, "roWidth") ?? item.RoughOpeningWidthDecimal;
            item.RoughOpeningHeightDecimal = TryReadMeasurementDecimal(payload, "roHeight") ?? item.RoughOpeningHeightDecimal;
            item.OutsideMeasureWidthDecimal = TryReadMeasurementDecimal(payload, "osmWidth") ?? item.OutsideMeasureWidthDecimal;
            item.OutsideMeasureHeightDecimal = TryReadMeasurementDecimal(payload, "osmHeight") ?? item.OutsideMeasureHeightDecimal;
            item.SectionCount = TryReadSectionCount(payload) ?? item.SectionCount;
            item.ConfigurationJson = payload.GetRawText();
        }

        private static void MarkCompleted(ConfiguredWindowItemEntity item, decimal authoritativePrice, DateTime completedAt)
        {
            item.Status = ConfiguredWindowItemStatus.Completed;
            item.AuthoritativePrice = authoritativePrice;
            item.PricingComputedAt = completedAt;
            item.CompletedAt = completedAt;
        }

        private void UpdateSessionState(QuoteSessionEntity session, DateTime? completedAt = null)
        {
            var allItemsCompleted = session.Items.Count > 0 &&
                                    session.Items.All(item => item.Status == ConfiguredWindowItemStatus.Completed);

            if (allItemsCompleted)
            {
                session.Status = QuoteSessionStatus.Completed;
                session.CompletedAt = completedAt ?? DateTime.UtcNow;
                return;
            }

            session.Status = QuoteSessionStatus.Draft;
            session.CompletedAt = null;
        }

        private WindowPricingInput BuildPricingInput(JsonElement payload, string productLineKey)
        {
            var catalogEntry = !string.IsNullOrWhiteSpace(productLineKey)
                ? _catalog.GetProductLine(productLineKey)
                : null;

            var paneConfigurationName = TryReadNestedString(payload, "paneConfiguration", "name") ?? string.Empty;

            var input = new WindowPricingInput
            {
                ManufacturerName = catalogEntry?.ManufacturerName
                    ?? TryReadNestedString(payload, "productLine", "manufacturerName")
                    ?? string.Empty,
                ProductLineName = catalogEntry?.Name
                    ?? TryReadNestedString(payload, "productLine", "name")
                    ?? string.Empty,
                FrameWidthDecimal = TryReadMeasurementDecimal(payload, "frameWidth") ?? 0m,
                FrameHeightDecimal = TryReadMeasurementDecimal(payload, "frameHeight") ?? 0m,
                BrickmouldPricingWidthDecimal = TryReadMeasurementDecimal(payload, "width")
                    ?? TryReadMeasurementDecimal(payload, "frameWidth")
                    ?? 0m,
                BrickmouldPricingHeightDecimal = TryReadMeasurementDecimal(payload, "height")
                    ?? TryReadMeasurementDecimal(payload, "frameHeight")
                    ?? 0m,
                OutsideWidthDecimal = TryReadMeasurementDecimal(payload, "osmWidth") ?? 0m,
                OutsideHeightDecimal = TryReadMeasurementDecimal(payload, "osmHeight") ?? 0m,
                FrameColorName = TryReadNestedString(payload, "frameColor", "name") ?? string.Empty,
                BrickmouldStyleName = TryReadNestedString(payload, "brickmouldStyle", "name") ?? string.Empty
            };

            if (payload.TryGetProperty("sections", out var sections) && sections.ValueKind == JsonValueKind.Array)
            {
                foreach (var section in sections.EnumerateArray())
                {
                    input.Sections.Add(new SectionPricingInput
                    {
                        WidthDecimal = TryReadMeasurementDecimal(section, "width") ?? 0m,
                        HeightDecimal = TryReadMeasurementDecimal(section, "height") ?? 0m,
                        StyleName = TryReadNestedString(section, "style", "name") ?? string.Empty,
                        GrillePatternName = TryReadNestedString(section, "grillePattern", "name") ?? string.Empty,
                        SdlPatternName = TryReadNestedString(section, "sdlPattern", "name") ?? string.Empty,
                        CrankName = TryReadNestedString(section, "crank", "name") ?? string.Empty,
                        PaneConfigurationName = paneConfigurationName
                    });
                }
            }

            return input;
        }

        private string ResolveProductLineKey(QuoteSessionEntity session, ConfiguredWindowItemEntity item, JsonElement payload)
        {
            var productLineKey = TryReadNestedString(payload, "productLine", "key");
            if (!string.IsNullOrWhiteSpace(productLineKey))
            {
                var catalogEntry = _catalog.GetProductLine(productLineKey);
                if (catalogEntry != null)
                    return catalogEntry.Key;
            }

            var productLineName = TryReadNestedString(payload, "productLine", "name");
            var manufacturerName = TryReadNestedString(payload, "productLine", "manufacturerName");

            var catalogMatch = _catalog.ListAll().FirstOrDefault(entry =>
                string.Equals(entry.Name, productLineName, StringComparison.OrdinalIgnoreCase) &&
                (string.IsNullOrWhiteSpace(manufacturerName) ||
                 string.Equals(entry.ManufacturerName, manufacturerName, StringComparison.OrdinalIgnoreCase)));

            if (catalogMatch != null)
                return catalogMatch.Key;

            if (!string.IsNullOrWhiteSpace(item.ProductLineKey))
                return item.ProductLineKey;

            return session.DefaultProductLineKey ?? string.Empty;
        }

        private static string? TryReadString(JsonElement payload, string propertyName)
        {
            return payload.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        private static string? TryReadNestedString(JsonElement payload, string parentProperty, string childProperty)
        {
            if (!payload.TryGetProperty(parentProperty, out var parent) || parent.ValueKind != JsonValueKind.Object)
                return null;

            return parent.TryGetProperty(childProperty, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        private static Guid? TryReadGuid(JsonElement payload, string propertyName)
        {
            var text = TryReadString(payload, propertyName);
            return Guid.TryParse(text, out var value) ? value : null;
        }

        private static int? TryReadInt32(JsonElement payload, string propertyName)
        {
            if (!payload.TryGetProperty(propertyName, out var value))
                return null;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var parsedNumber))
                return parsedNumber;

            var text = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedText)
                ? parsedText
                : null;
        }

        private static bool? TryReadBoolean(JsonElement payload, string propertyName)
        {
            if (!payload.TryGetProperty(propertyName, out var value))
                return null;

            if (value.ValueKind == JsonValueKind.True)
                return true;

            if (value.ValueKind == JsonValueKind.False)
                return false;

            var text = value.ValueKind == JsonValueKind.String ? value.GetString() : null;
            return bool.TryParse(text, out var parsed) ? parsed : null;
        }

        private static decimal? TryReadMeasurementDecimal(JsonElement payload, string propertyName)
        {
            if (!payload.TryGetProperty(propertyName, out var measurement) || measurement.ValueKind != JsonValueKind.Object)
                return null;

            if (!TryReadDecimal(measurement, "sign", out var sign) ||
                !TryReadDecimal(measurement, "whole", out var whole) ||
                !TryReadDecimal(measurement, "numerator", out var numerator) ||
                !TryReadDecimal(measurement, "denominator", out var denominator) ||
                denominator == 0m)
            {
                return null;
            }

            var magnitude = whole + (numerator / denominator);
            return sign < 0 ? -magnitude : magnitude;
        }

        private static bool TryReadDecimal(JsonElement payload, string propertyName, out decimal value)
        {
            value = default;
            if (!payload.TryGetProperty(propertyName, out var element))
                return false;

            if (element.ValueKind == JsonValueKind.Number)
                return element.TryGetDecimal(out value);

            if (element.ValueKind == JsonValueKind.String)
                return decimal.TryParse(element.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);

            return false;
        }

        private static int? TryReadSectionCount(JsonElement payload)
        {
            if (!payload.TryGetProperty("sections", out var sections) || sections.ValueKind != JsonValueKind.Array)
                return null;

            return sections.GetArrayLength();
        }
    }
}
