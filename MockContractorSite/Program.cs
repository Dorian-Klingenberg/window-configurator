using MockContractorSite.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MockContractorOptions>(
    builder.Configuration.GetSection(MockContractorOptions.SectionName));
builder.Services.AddHttpClient<QuoteSessionBootstrapClient>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/session-launch", async (SessionLaunchRequest request, QuoteSessionBootstrapClient client) =>
{
    var launchUrl = await client.CreateSessionLaunchAsync(request.CtaVariant);
    return Results.Ok(new { launchUrl });
});

app.Run();
