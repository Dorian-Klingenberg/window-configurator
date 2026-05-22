using MockContractorCrm.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MockCrmOptions>(builder.Configuration.GetSection(MockCrmOptions.SectionName));
builder.Services.AddHttpClient<CrmQuoteSessionClient>();
builder.Services.AddSingleton<ICrmOpportunityStore, InMemoryCrmOpportunityStore>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/opportunities", async (CreateOpportunityRequest request, ICrmOpportunityStore store) =>
{
    var opportunity = await store.CreateAsync(request.CustomerName, request.CustomerEmail);
    return Results.Ok(opportunity);
});

app.MapGet("/api/opportunities", async (ICrmOpportunityStore store) =>
{
    return Results.Ok(await store.ListAsync());
});

app.MapPost("/api/opportunities/{id:guid}/start-quote", async (Guid id, StartQuoteRequest request, ICrmOpportunityStore store, CrmQuoteSessionClient client) =>
{
    var opportunity = await store.GetAsync(id);
    if (opportunity == null)
        return Results.NotFound(new { error = "Opportunity not found." });

    var started = await client.StartQuoteSessionAsync(opportunity.OpportunityNumber, opportunity.CustomerEmail, request.ProductLineKey);
    await store.MarkQuoteStartedAsync(id, started.LaunchUrl, started.SessionId);
    return Results.Ok(started);
});

app.Run();
