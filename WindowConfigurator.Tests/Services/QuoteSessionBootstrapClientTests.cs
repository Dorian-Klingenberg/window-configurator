using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using MockContractorSite.Services;

namespace WindowConfigurator.Tests.Services
{
    public class QuoteSessionBootstrapClientTests
    {
        [Fact]
        public async Task CreateSessionLaunchAsync_UsesConfiguredApiKeyAndReturnsAbsoluteLaunchUrl()
        {
            var tenantId = Guid.NewGuid();
            var handler = new StubHttpMessageHandler((request, _) =>
            {
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.Equal("demo-api-key", request.Headers.GetValues("X-Api-Key").Single());
                Assert.Equal("http://localhost:5149/api/v1/quote-sessions", request.RequestUri!.ToString());
                return new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = JsonContent.Create(new
                    {
                        sessionId = Guid.NewGuid(),
                        sessionUrl = "/12345678-1234-1234-1234-123456789012"
                    })
                };
            });

            var httpClient = new HttpClient(handler);
            var options = Options.Create(new MockContractorOptions
            {
                WindowConfiguratorBaseUrl = "http://localhost:5149",
                TenantId = tenantId,
                ApiKey = "demo-api-key",
                DefaultProductLineKey = "energysaver-2500"
            });

            var sut = new QuoteSessionBootstrapClient(httpClient, options);

            var launchUrl = await sut.CreateSessionLaunchAsync("CTA-A");

            Assert.Equal("http://localhost:5149/12345678-1234-1234-1234-123456789012", launchUrl);
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
}
