using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using MockContractorCrm.Services;

namespace WindowConfigurator.Tests.Services;

public class CrmQuoteSessionClientTests
{
    [Fact]
    public async Task StartQuoteSessionAsync_UsesApiKeyAuthAndReturnsAbsoluteLaunchUrl()
    {
        var tenantId = Guid.NewGuid();
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("crm-demo-api-key", request.Headers.GetValues("X-Api-Key").Single());
            Assert.Equal("http://localhost:5149/api/v1/quote-sessions", request.RequestUri!.ToString());

            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent.Create(new
                {
                    sessionId = Guid.NewGuid(),
                    sessionUrl = "/aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
                })
            };
        });

        var httpClient = new HttpClient(handler);
        var options = Options.Create(new MockCrmOptions
        {
            WindowConfiguratorBaseUrl = "http://localhost:5149",
            TenantId = tenantId,
            ApiKey = "crm-demo-api-key",
            DefaultProductLineKey = "apex"
        });

        var sut = new CrmQuoteSessionClient(httpClient, options);

        var result = await sut.StartQuoteSessionAsync("OPP-1001", "owner@example.com");

        Assert.Equal("http://localhost:5149/aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", result.LaunchUrl);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _onSend;

        public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> onSend)
        {
            _onSend = onSend;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_onSend(request, cancellationToken));
        }
    }
}
