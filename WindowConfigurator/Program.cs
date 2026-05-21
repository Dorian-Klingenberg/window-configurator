using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WindowConfigurator.Data;
using WindowConfigurator.Data.Catalog;
using WindowConfigurator.Data.Entities;
using WindowConfigurator.Data.Pricing;
using WindowConfigurator.Data.Repositories;
using WindowConfigurator.Data.Validation;
using WindowConfigurator.Web.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<WindowConfiguratorDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IQuoteSessionRepository, EfQuoteSessionRepository>();
builder.Services.AddScoped<ITenantRepository, EfTenantRepository>();
builder.Services.AddSingleton<ICatalogService, CatalogService>();
builder.Services.AddSingleton<WindowConfiguratorDataHelper>();
builder.Services.AddSingleton<ITemplateReader>(sp => sp.GetRequiredService<WindowConfiguratorDataHelper>());

// Load priceInfo.json once at startup and share it with server pricing and validation.
builder.Services.AddSingleton(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var path = Path.Combine(env.ContentRootPath, "AppData", "priceInfo.json");
    var json = File.ReadAllText(path);
    var priceInfoRoot = JsonSerializer.Deserialize<PriceInfoRoot>(
        json,
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
    return priceInfoRoot;
});
builder.Services.AddSingleton<IPricingService>(sp => new PricingService(sp.GetRequiredService<PriceInfoRoot>()));
builder.Services.AddSingleton<ICompletionValidationService>(sp =>
    new CompletionValidationService(sp.GetRequiredService<PriceInfoRoot>()));

var app = builder.Build();

// Ensure the database schema exists and, in Development, seed a demo session.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WindowConfiguratorDbContext>();
    db.Database.EnsureCreated();

    if (app.Environment.IsDevelopment() && !db.QuoteSessions.Any())
    {
        var demoTenantId = new Guid("00000000-0000-0000-0000-000000000002");
        var demoSessionId = new Guid("00000000-0000-0000-0000-000000000001");

        db.Tenants.Add(new TenantEntity
        {
            Id = demoTenantId,
            Name = "Demo Dealer",
            ApiKey = "demo-api-key",
            WebhookCallbackUrl = "http://localhost/webhook",
            AllowedProductLineKeys = ["energysaver-2500", "apex", "carriage"],
            MixedProductLinesAllowed = true
        });

        db.QuoteSessions.Add(new QuoteSessionEntity
        {
            Id = demoSessionId,
            TenantId = demoTenantId,
            DefaultProductLineKey = "energysaver-2500",
            Status = QuoteSessionStatus.Draft,
            ExternalReferenceId = "DEMO-001"
        });

        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{id?}",
    defaults: new { controller = "orderitem", action = "index"});

app.MapControllerRoute(
    name: "createSession",
    pattern: "new",
    defaults: new { controller = "orderitem", action = "CreateDevSession"});

app.MapControllerRoute(
    name: "api",
    pattern: "{controller}/{Action}/{id?}");


app.Run();
