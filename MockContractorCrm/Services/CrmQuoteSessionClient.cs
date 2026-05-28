using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace MockContractorCrm.Services;

public sealed class CrmQuoteSessionClient
{
    private readonly HttpClient _httpClient;
    private readonly MockCrmOptions _options;

    public CrmQuoteSessionClient(HttpClient httpClient, IOptions<MockCrmOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<StartQuoteResult> StartQuoteSessionAsync(string opportunityNumber, string customerEmail, string? productLineKey = null)
    {
        var baseUrl = _options.WindowConfiguratorBaseUrl.TrimEnd('/');
        var requestUrl = $"{baseUrl}/api/v1/quote-sessions";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        request.Headers.Add("X-Api-Key", _options.ApiKey);
        request.Content = JsonContent.Create(new
        {
            tenantId = _options.TenantId,
            defaultProductLineKey = string.IsNullOrWhiteSpace(productLineKey) ? _options.DefaultProductLineKey : productLineKey.Trim(),
            externalReferenceId = opportunityNumber,
            customerEmail
        });

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<CreateQuoteSessionResponse>();
        if (payload == null || string.IsNullOrWhiteSpace(payload.SessionUrl))
            throw new InvalidOperationException("Quote session response did not include a session URL.");

        return new StartQuoteResult(
            payload.SessionId,
            $"{baseUrl}/{payload.SessionUrl.TrimStart('/')}");
    }

    public async Task<QuoteStatusResult?> GetSessionStatusAsync(Guid sessionId)
    {
        var baseUrl = _options.WindowConfiguratorBaseUrl.TrimEnd('/');
        var requestUrl = $"{baseUrl}/api/v1/quote-sessions/{sessionId}";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        request.Headers.Add("X-Api-Key", _options.ApiKey);

        using var response = await _httpClient.SendAsync(request);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<QuoteSessionStatusResponse>();
        if (payload == null)
            return null;

        var totalPrice = payload.Items
            .Where(i => i.AuthoritativePrice.HasValue)
            .Select(i => i.AuthoritativePrice!.Value)
            .DefaultIfEmpty(0m)
            .Sum();

        var latestCompletedAt = payload.Items
            .Select(i => i.CompletedAt)
            .Where(d => d.HasValue)
            .Select(d => d!.Value)
            .OrderByDescending(d => d)
            .Cast<DateTime?>()
            .FirstOrDefault();

        return new QuoteStatusResult(
            sessionId,
            payload.Status,
            payload.Items.Count,
            totalPrice > 0 ? totalPrice : null,
            latestCompletedAt);
    }

    private sealed class CreateQuoteSessionResponse
    {
        public Guid SessionId { get; set; }
        public string SessionUrl { get; set; } = string.Empty;
    }

    private sealed class QuoteSessionStatusResponse
    {
        public string Status { get; set; } = string.Empty;
        public List<QuoteSessionStatusItemResponse> Items { get; set; } = [];
    }

    private sealed class QuoteSessionStatusItemResponse
    {
        public decimal? AuthoritativePrice { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}

public readonly record struct StartQuoteResult(Guid SessionId, string LaunchUrl);

public readonly record struct QuoteStatusResult(
    Guid SessionId,
    string Status,
    int ItemCount,
    decimal? TotalPrice,
    DateTime? CompletedAt);
