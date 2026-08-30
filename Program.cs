using Label.Mcp.Regulatory.Configuration;
using Label.Mcp.Regulatory.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<AzureSearchOptions>()
    .Bind(builder.Configuration.GetSection(
        AzureSearchOptions.SectionName))
    .Validate(
        options => Uri.TryCreate(
            options.Endpoint,
            UriKind.Absolute,
            out _),
        "AzureSearch:Endpoint must be a valid absolute URL.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(
            options.IndexName),
        "AzureSearch:IndexName is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(
            options.ContentField),
        "AzureSearch:ContentField is required.")
    .ValidateOnStart();

builder.Services.AddSingleton<IRegulatorySearchService,
    RegulatorySearchService>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

app.MapGet("/", () => Results.Ok(new
{
    service = "Label.Mcp.Regulatory",
    status = "running",
    mcpEndpoint = "/mcp",
    healthEndpoint = "/health"
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "Label.Mcp.Regulatory",
    timestampUtc = DateTimeOffset.UtcNow
}));

app.MapMcp("/mcp");

app.Run();