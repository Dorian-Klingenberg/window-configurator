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

    private sealed class CreateQuoteSessionResponse
    {
        public Guid SessionId { get; set; }
        public string SessionUrl { get; set; } = string.Empty;
    }
}

public readonly record struct StartQuoteResult(Guid SessionId, string LaunchUrl);
