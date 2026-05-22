using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WindowConfigurator.Controllers.Api.V1.Security;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Repositories;

namespace WindowConfigurator.Tests.Controllers
{
    public class ApiKeyAuthorizeFilterTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly WindowConfiguratorDbContext _context;
        private readonly ITenantRepository _tenantRepository;

        public ApiKeyAuthorizeFilterTests()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<WindowConfiguratorDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new WindowConfiguratorDbContext(options);
            _context.Database.EnsureCreated();
            _tenantRepository = new EfTenantRepository(_context);
        }

        [Fact]
        public async Task OnActionExecutionAsync_WithoutApiKey_ReturnsUnauthorized()
        {
            var filter = new ApiKeyAuthorizeFilter(_tenantRepository);
            var context = BuildContext();

            await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

            Assert.IsType<UnauthorizedObjectResult>(context.Result);
        }

        [Fact]
        public async Task OnActionExecutionAsync_WithValidApiKey_SetsTenantContext()
        {
            var tenant = new TenantEntity { Name = "Auth", ApiKey = "valid-key", WebhookCallbackUrl = "https://hook" };
            await _tenantRepository.AddAsync(tenant);
            await _tenantRepository.SaveChangesAsync();

            var filter = new ApiKeyAuthorizeFilter(_tenantRepository);
            var context = BuildContext();
            context.HttpContext.Request.Headers["X-Api-Key"] = "valid-key";

            await filter.OnActionExecutionAsync(context, () =>
            {
                var executed = new ActionExecutedContext(context, new List<IFilterMetadata>(), context.Controller);
                return Task.FromResult(executed);
            });

            Assert.Null(context.Result);
            Assert.Equal(tenant.Id, context.HttpContext.Items[ApiKeyAuthorizeFilter.TenantIdItemKey]);
        }

        private static ActionExecutingContext BuildContext()
        {
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteData();
            var actionDescriptor = new ControllerActionDescriptor();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?>(),
                new object());
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }
    }
}
