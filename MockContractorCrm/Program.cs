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

app.MapPost("/api/opportunities/{id:guid}/refresh-status", async (Guid id, ICrmOpportunityStore store, CrmQuoteSessionClient client) =>
{
    var opportunity = await store.GetAsync(id);
    if (opportunity == null)
        return Results.NotFound(new { error = "Opportunity not found." });

    if (opportunity.QuoteSessionId == null)
        return Results.BadRequest(new { error = "No quote session started for this opportunity." });

    QuoteStatusResult? statusResult;
    try
    {
        statusResult = await client.GetSessionStatusAsync(opportunity.QuoteSessionId.Value);
    }
    catch (Exception)
    {
        return Results.Problem("Unable to reach the configurator. Check that it is running.", statusCode: 502);
    }

    if (statusResult == null)
        return Results.NotFound(new { error = "Quote session not found in configurator." });

    var status = statusResult.Value;
    if (status.Status is "Completed" or "Submitted")
    {
        var submittedAt = status.CompletedAt ?? DateTime.UtcNow;
        await store.MarkQuoteSubmittedAsync(id, submittedAt, status.ItemCount, status.TotalPrice);
    }

    return Results.Ok(new
    {
        sessionId = status.SessionId,
        status = status.Status,
        itemCount = status.ItemCount,
        totalPrice = status.TotalPrice,
        completedAt = status.CompletedAt
    });
});

app.Run();
