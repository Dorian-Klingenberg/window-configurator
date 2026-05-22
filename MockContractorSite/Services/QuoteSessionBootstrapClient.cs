using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace MockContractorSite.Services;

public sealed class QuoteSessionBootstrapClient
{
    private readonly HttpClient _httpClient;
    private readonly MockContractorOptions _options;

    public QuoteSessionBootstrapClient(HttpClient httpClient, IOptions<MockContractorOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> CreateSessionLaunchAsync(string ctaVariant)
    {
        var baseUrl = _options.WindowConfiguratorBaseUrl.TrimEnd('/');
        var requestUrl = $"{baseUrl}/api/v1/quote-sessions";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        request.Headers.Add("X-Api-Key", _options.ApiKey);
        request.Content = JsonContent.Create(new
        {
            tenantId = _options.TenantId,
            defaultProductLineKey = _options.DefaultProductLineKey,
            externalReferenceId = $"mock-site-{ctaVariant}-{DateTime.UtcNow:yyyyMMddHHmmss}"
        });

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<CreateQuoteSessionResponse>();
        if (payload == null || string.IsNullOrWhiteSpace(payload.SessionUrl))
            throw new InvalidOperationException("Quote session response did not include a session URL.");

        return $"{baseUrl}/{payload.SessionUrl.TrimStart('/')}";
    }

    private sealed class CreateQuoteSessionResponse
    {
        public string SessionUrl { get; set; } = string.Empty;
    }
}
