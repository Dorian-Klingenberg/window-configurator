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

    [Fact]
    public async Task GetSessionStatusAsync_WhenSessionIsCompleted_ReturnsStatusWithTotalPrice()
    {
        var sessionId = Guid.NewGuid();
        var completedAt = new DateTime(2026, 5, 27, 14, 0, 0, DateTimeKind.Utc);

        var handler = new StubHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("crm-demo-api-key", request.Headers.GetValues("X-Api-Key").Single());
            Assert.Equal($"http://localhost:5149/api/v1/quote-sessions/{sessionId}", request.RequestUri!.ToString());

            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    sessionId,
                    status = "Completed",
                    items = new[]
                    {
                        new { authoritativePrice = (decimal?)850.00m, completedAt = (DateTime?)completedAt },
                        new { authoritativePrice = (decimal?)620.50m, completedAt = (DateTime?)completedAt }
                    }
                })
            };
        });

        var httpClient = new HttpClient(handler);
        var options = Options.Create(new MockCrmOptions
        {
            WindowConfiguratorBaseUrl = "http://localhost:5149",
            TenantId = Guid.NewGuid(),
            ApiKey = "crm-demo-api-key",
            DefaultProductLineKey = "apex"
        });

        var sut = new CrmQuoteSessionClient(httpClient, options);

        var result = await sut.GetSessionStatusAsync(sessionId);

        Assert.NotNull(result);
        Assert.Equal(sessionId, result!.Value.SessionId);
        Assert.Equal("Completed", result.Value.Status);
        Assert.Equal(2, result.Value.ItemCount);
        Assert.Equal(1470.50m, result.Value.TotalPrice);
        Assert.Equal(completedAt, result.Value.CompletedAt);
    }

    [Fact]
    public async Task GetSessionStatusAsync_WhenSessionNotFound_ReturnsNull()
    {
        var sessionId = Guid.NewGuid();

        var handler = new StubHttpMessageHandler((_, _) =>
            new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));

        var httpClient = new HttpClient(handler);
        var options = Options.Create(new MockCrmOptions
        {
            WindowConfiguratorBaseUrl = "http://localhost:5149",
            TenantId = Guid.NewGuid(),
            ApiKey = "crm-demo-api-key",
            DefaultProductLineKey = "apex"
        });

        var sut = new CrmQuoteSessionClient(httpClient, options);

        var result = await sut.GetSessionStatusAsync(sessionId);

        Assert.Null(result);
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
